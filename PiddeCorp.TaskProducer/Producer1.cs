using RabbitMQ.Client;
using PiddeCorp.RabbitMQ;
using PiddeCorp.Integrations.Contracts.V1.Task;
using PiddeCorp.Integrations.Contracts.V1.Task.Type;

namespace PiddeCorp.TaskProducer;

public static class Producer1 {
    public static async Task Run() {
        (IConnection conn, IChannel channel) =
            await Queue.Connect("amqp://guest:guest@localhost:5672/", OpenTask.QueueName);
        Console.WriteLine("Queue connected.");
        

        OpenTask todoTask = new() {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            Type = TaskType.Todo,
            Due = DateTime.Now.AddDays(5),
            Data = new Todo() {
                IsOrderedByPriority = true,
                Items = ["Wash dishes", "Scrub toilet"],
            },
        };
        await Queue.SendMessage(channel,
                                System.Text.Encoding.UTF8.GetBytes(todoTask.Serialize()),
                                OpenTask.QueueName
        );
        Console.WriteLine("Todo task sent.");
        
        OpenTask honeyDoTask = new() {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            Type = TaskType.HoneyDo,
            Due = DateTime.Now.AddHours(1),
            Data = new HoneyDo() {
                Description = "Clean the gutters! xoxo",
            },
        };
        
        await Queue.SendMessage(channel,
                                System.Text.Encoding.UTF8.GetBytes(honeyDoTask.Serialize()),
                                OpenTask.QueueName
        );
        Console.WriteLine("Honey Do task sent.");
        
        OpenTask mountainDewTask = new() {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            Type = TaskType.MountainDew,
            Due = DateTime.Now.AddHours(1),
            Data = new MountainDew() {
                ExtremeCaffeine = true,
                Flavor = "Voltage Raspberry",
                Comment = "Chug before doing anything else or you're fired.",
            },
        };
        
        await Queue.SendMessage(channel,
                                System.Text.Encoding.UTF8.GetBytes(mountainDewTask.Serialize()),
                                OpenTask.QueueName
        );
        Console.WriteLine("Mountain Dew task sent.");
        
        Queue.Disconnect(conn, channel);
        Console.WriteLine("Queue disconnected.");
    }
}
