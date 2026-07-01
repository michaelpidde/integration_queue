using Bogus;
using PiddeCorp.Integrations.Contracts.V1.CustomType;
using RabbitMQ.Client;
using PiddeCorp.RabbitMQ;
using PiddeCorp.Integrations.Contracts.V1.Task;
using PiddeCorp.Integrations.Contracts.V1.Task.Type;

namespace PiddeCorp.TaskProducer;

public static class Producer2 {
    private static BoundedStringList GenerateRandomStrings(int capacity) {
        BoundedStringList strings = [];
        Faker faker = new();
        for(int i = 0; i < capacity; ++i) {
            strings.Add(string.Join(" ", faker.Lorem.Words(capacity)));
        }
        
        return strings;
    }
    
    private static OpenTask GenerateRandomTask() {
        Random rng = new();
        int random = rng.Next(1, 4);
        
        OpenTask openTask = random switch {
            1 => new OpenTask {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Type = TaskType.Todo,
                Due = DateTime.Now.AddDays(rng.Next(1, 32)),
                Data = new Todo() {
                    IsOrderedByPriority = true,
                    Items = GenerateRandomStrings(rng.Next(2, 6)),
                },
            },
            2 => new OpenTask {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Type = TaskType.HoneyDo,
                Due = DateTime.Now.AddHours(rng.Next(1, 51)),
                Data = new HoneyDo() { Description = new Faker().Company.Bs() },
            },
            _ => new OpenTask {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Type = TaskType.MountainDew,
                Due = DateTime.Now.AddHours(rng.Next(1, 11)),
                Data = new MountainDew() {
                    ExtremeCaffeine = Convert.ToBoolean(rng.Next(0, 2)),
                    Flavor = new Faker().Commerce.ProductName(),
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
