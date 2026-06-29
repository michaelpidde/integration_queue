using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using PiddeCorp.Integrations.Contracts.V1.Task;
using PiddeCorp.Integrations.Contracts.V1.Task.Type;
using PiddeCorp.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PiddeCorp.TaskConsumer;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if(logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
        
        string rabbitMq = configuration.GetConnectionString("RabbitMQ");
        (IConnection conn, IChannel channel) = await Queue.Connect(rabbitMq, OpenTask.QueueName);
        logger.LogInformation("Queue connected.");
        
        string sqlserver = configuration.GetConnectionString("SqlServer");
        await using SqlConnection sqlConn = new(sqlserver);
        await sqlConn.OpenAsync(stoppingToken);
        
        AsyncEventingBasicConsumer consumer = new(channel);
        
        consumer.ReceivedAsync += async (model, ea) => {
            string json = Encoding.UTF8.GetString(ea.Body.ToArray());
            OpenTask task = OpenTask.Deserialize(json);
            
            try {
                await sqlConn.ExecuteAsync(
                    "INSERT INTO OpenTask (Id, Created, Type, Due) OUTPUT INSERTED.id VALUES (@Id, @Created, @Type, @Due)",
                    new {
                        task.Id,
                        task.Created,
                        Type = (int)task.Type,
                        task.Due,
                    }
                );
                logger.LogDebug($"Inserted OpenTask with ID {task.Id}");
                
                await sqlConn.ExecuteAsync(
                    "INSERT INTO OpenTaskWorkflow (OpenTaskId) VALUES (@OpenTaskId)",
                    new {
                        OpenTaskId = task.Id,
                    }
                );
                logger.LogDebug($"Inserted OpenTaskWorkflow with ID {task.Id}");
                
                switch(task.Type) {
                    case TaskType.Todo:
                        Todo todo = (Todo)task.Data;
                        int todoId = sqlConn.QuerySingle<int>(
                            "INSERT INTO Todo (OpenTaskId, IsOrderedByPriority) OUTPUT INSERTED.id VALUES (@OpenTaskId, @IsOrderedByPriority)",
                            new {
                                OpenTaskId = task.Id,
                                todo.IsOrderedByPriority,
                            }
                        );
                        logger.LogDebug($"Inserted Todo with ID {todoId}");
                        
                        foreach(string item in todo.Items) {
                            await sqlConn.ExecuteAsync(
                                "INSERT INTO TodoItem (TodoId, Item) VALUES (@TodoId, @Item)",
                                new {
                                    todoId,
                                    item,
                                }
                            );
                            logger.LogDebug($"Inserted TodoItem for Todo ID {todoId}");
                        }
                        
                        break;
                    case TaskType.HoneyDo:
                        HoneyDo honeyDo = (HoneyDo)task.Data;
                        int honeyDoId = sqlConn.QuerySingle<int>(
                            "INSERT INTO HoneyDo (OpenTaskId, IsUrgent, Description) OUTPUT INSERTED.id VALUES (@OpenTaskId, @IsUrgent, @Description)",
                            new {
                                OpenTaskId = task.Id,
                                honeyDo.IsUrgent,
                                honeyDo.Description,
                            }
                        );
                        logger.LogDebug($"Inserted HoneyDo with ID {honeyDoId}");
                        break;
                    case TaskType.MountainDew:
                        MountainDew mountainDew = (MountainDew)task.Data;
                        int mountainDewId = sqlConn.QuerySingle<int>(
                            "INSERT INTO MountainDew (OpenTaskId, ExtremeCaffeine, Flavor, Comment) OUTPUT INSERTED.id VALUES (@OpenTaskId, @ExtremeCaffeine, @Flavor, @Comment)",
                            new {
                                OpenTaskId = task.Id,
                                mountainDew.ExtremeCaffeine,
                                mountainDew.Flavor,
                                mountainDew.Comment,
                            }
                        );
                        logger.LogDebug($"Inserted MountainDew with ID {mountainDewId}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            } catch(SqlException ex) when(ex.Number == 2627) { // unique constraint violation
                logger.LogWarning("Duplicate message received for task {Id}, discarding.", task.Id);
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            } catch(Exception ex) {
                logger.LogError(ex, "Failed to process message");
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, cancellationToken: stoppingToken);
            }
        };
        
        // Consume 1 message at a time - increase consumers for higher throughput
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);
        
        await channel.BasicConsumeAsync(
            OpenTask.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken
        );
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
