using RabbitMQ.Client;
using PiddeCorp.RabbitMQ;
using PiddeCorp.Integrations.Contracts.V1.Task;
using PiddeCorp.Integrations.Contracts.V1.Task.Type;

namespace PiddeCorp.TaskProducer;

public static class Producer2 {
    private static OpenTask GenerateRandomTask() {
        Random rng = new();
        int random = rng.Next(1, 4);
        
        OpenTask openTask = random switch {
            1 => new OpenTask {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Type = TaskType.Todo,
                Due = DateTime.Now.AddDays(5),
                Data = new Todo() {
                    IsOrderedByPriority = true,
                    Items = ["Wash dishes", "Scrub toilet"],
                },
            },
            2 => new OpenTask {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Type = TaskType.HoneyDo,
                Due = DateTime.Now.AddHours(1),
                Data = new HoneyDo() { Description = "Clean the gutters! xoxo", },
            },
            _ => new OpenTask {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Type = TaskType.MountainDew,
                Due = DateTime.Now.AddHours(1),
                Data = new MountainDew() {
                    ExtremeCaffeine = true,
                    Flavor = "Voltage Raspberry",
                    Comment = "Chug before doing anything else or you're fired.",
                },
            },
        };
        
        return openTask;
    }
    
    private static async Task PublishTask(IConnection conn, OpenTask openTask) {
        await using IChannel channel = await conn.CreateChannelAsync();
        await Queue.SendMessage(channel,
                                System.Text.Encoding.UTF8.GetBytes(openTask.Serialize()),
                                OpenTask.QueueName
        );
        Console.WriteLine($"Published message {openTask.Id}");
    }
    
    public static async Task Run() {
        IEnumerable<int> ids = Enumerable.Range(0, 100000);
        
        (IConnection conn, IChannel channel) =
            await Queue.Connect("amqp://guest:guest@localhost:5672/", OpenTask.QueueName);
        
        await Parallel.ForEachAsync(ids, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (i, ct) => {
                OpenTask task = GenerateRandomTask();
                await PublishTask(conn, task);
            }
        );
        
        Queue.Disconnect(conn, channel);
    }
}
