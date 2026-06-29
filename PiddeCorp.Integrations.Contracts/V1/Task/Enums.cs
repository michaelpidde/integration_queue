namespace PiddeCorp.Integrations.Contracts.V1.Task;

/// <summary>
/// Specifies the type of task, which determines the concrete <see cref="TaskData"/> 
/// implementation used in an <see cref="OpenTask"/> message.
/// </summary>
public enum TaskType {
    /// <summary>
    /// A general to-do task. See <see cref="Todo"/> for the corresponding payload type.
    /// </summary>
    Todo,
    
    /// <summary>
    /// A honey-do task. Urgent by definition. See <see cref="HoneyDo"/> for the corresponding payload type.
    /// </summary>
    HoneyDo,
    
    /// <summary>
    /// A Mountain Dew task. See <see cref="MountainDew"/> for the corresponding payload type.
    /// </summary>
    MountainDew
}
