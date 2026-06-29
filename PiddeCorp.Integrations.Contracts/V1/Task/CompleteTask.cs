namespace PiddeCorp.Integrations.Contracts.V1.Task;

/// <summary>
/// Nothing here yet.
/// </summary>
public sealed class CompleteTask {
    /// <summary>
    /// The name of the queue this message is published to and consumed from.
    /// </summary>
    public const string QueueName = "tasks.complete";
}
