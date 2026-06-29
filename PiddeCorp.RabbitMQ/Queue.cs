using RabbitMQ.Client;

namespace PiddeCorp.RabbitMQ;

public static class Queue {
    public static async Task<(IConnection, IChannel)> Connect(
        string connectionString,
        string queueName
    ) {
        ConnectionFactory factory = new() {
            Uri = new Uri(connectionString),
        };
        
        IConnection conn = await factory.CreateConnectionAsync();
        IChannel channel = await conn.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(queueName, true, false, false, null);
        
        return (conn, channel);
    }
    
    public static async Task SendMessage(
        IChannel channel,
        byte[] message,
        string routingKey
    ) {
        BasicProperties props = new() {
            ContentType = "text/json",
            DeliveryMode = DeliveryModes.Persistent
        };
        await channel.BasicPublishAsync("", routingKey, mandatory: true, basicProperties: props, body: message);
    }
    
    public static void Disconnect(
        IConnection conn,
        IChannel channel
    ) {
        channel.Dispose();
        conn.Dispose();
    }
}
