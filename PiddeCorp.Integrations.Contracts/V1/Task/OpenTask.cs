using System.Diagnostics.CodeAnalysis;
using PiddeCorp.Integrations.Contracts.V1.Task.Type;
using System.Text.Json;

namespace PiddeCorp.Integrations.Contracts.V1.Task;

internal static class SerializerOptions {
    public static readonly JsonSerializerOptions Default = new() {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = false,
    };
}

/**
 * This class is used internally for serialize/deserialization
 */
internal sealed class OpenTaskEnvelope {
    public required Guid Id { get; set; }
    public required DateTime Created { get; set; }
    public required TaskType Type { get; set; }
    
    public required DateTime Due { get; set; }
    
    // This is typed as object so when serialized it will use the underlying runtime type, not the abastract TaskData type
    public required object Data { get; set; }
}

/// <summary>
/// Abstract base class for task-specific payload types.
/// Concrete implementations are determined by <see cref="TaskType"/> —
/// see <see cref="Todo"/>, <see cref="HoneyDo"/>, and <see cref="MountainDew"/>.
/// </summary>
public abstract class TaskData { }

/// <summary>
/// Represents an open task message published to the <see cref="QueueName"/> queue.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="Serialize"/> to produce a JSON payload for publishing, and
/// <see cref="Deserialize"/> to reconstruct a fully hydrated instance from a consumed message.
/// </para>
/// <para>
/// The <see cref="Data"/> property is typed as <see cref="TaskData"/>, an abstract type.
/// The concrete type is determined by <see cref="Type"/> — see <see cref="TaskType"/> for
/// available values and their corresponding types in the Task.Type namespace.
/// </para>
/// </remarks>
public sealed class OpenTask {
    public OpenTask() { }
    
    [SetsRequiredMembers]
    public OpenTask(Guid id, DateTime created, TaskType type, DateTime due, TaskData data) {
        Id = id;
        Created = created;
        Type = type;
        Due = due;
        Data = data;
    }
    
    /// <summary>
    /// The name of the queue this message is published to and consumed from.
    /// </summary>
    public const string QueueName = "tasks.open";
    
    /// <summary>
    /// Unique identifier for this task.
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// The datetime this task was created.
    /// </summary>
    public required DateTime Created { get; set; }
    
    /// <summary>
    /// Determines the concrete type of <see cref="Data"/>.
    /// </summary>
    public required TaskType Type { get; set; }
    
    /// <summary>
    /// The datetime by which this task is due.
    /// </summary>
    public required DateTime Due { get; set; }
    
    /// <summary>
    /// The task-specific payload. Cast to the concrete type indicated by <see cref="Type"/>.
    /// </summary>
    public required TaskData Data { get; set; }
    
    /// <summary>
    /// Serializes this instance to a JSON string suitable for publishing to the queue.
    /// </summary>
    /// <returns>A JSON string representation of this open task.</returns>
    public string Serialize() {
        var envelope = new OpenTaskEnvelope {
            Id = Id,
            Created = Created,
            Type = Type,
            Due = Due,
            Data = Data
        };
        return JsonSerializer.Serialize(envelope, SerializerOptions.Default);
    }
    
    /// <summary>
    /// Deserializes a JSON string consumed from the queue into a fully hydrated <see cref="OpenTask"/> instance.
    /// </summary>
    /// <remarks>
    /// Deserialization is performed in two passes. The first pass resolves the envelope and reads
    /// <see cref="Type"/>. The second pass uses <see cref="Type"/> to deserialize <see cref="Data"/>
    /// into its correct concrete type. Consumers do not need to perform any additional deserialization.
    /// </remarks>
    /// <param name="json">The raw JSON string consumed from the queue.</param>
    /// <returns>A fully hydrated <see cref="OpenTask"/> instance with <see cref="Data"/> resolved to its concrete type.</returns>
    /// <exception cref="NotSupportedException">Thrown when <see cref="Type"/> does not map to a known <see cref="TaskData"/> implementation.</exception>
    public static OpenTask Deserialize(string json) {
        var envelope = JsonSerializer.Deserialize<OpenTaskEnvelope>(json);
        var dataElement = (JsonElement)envelope!.Data;
        TaskData data = envelope!.Type switch {
            TaskType.Todo => JsonSerializer.Deserialize<Todo>(dataElement.GetRawText())!,
            TaskType.HoneyDo => JsonSerializer.Deserialize<HoneyDo>(dataElement.GetRawText())!,
            TaskType.MountainDew => JsonSerializer.Deserialize<MountainDew>(dataElement.GetRawText())!,
            _ => throw new NotSupportedException($"Unknown task type: {envelope.Type}")
        };
        return new OpenTask {
            Id = envelope.Id,
            Created = envelope.Created,
            Type = envelope.Type,
            Due = envelope.Due,
            Data = data
        };
    }
}
