using PiddeCorp.Integrations.Contracts.V1.CustomType;

namespace PiddeCorp.Integrations.Contracts.V1.Task.Type;

/// <summary>
/// Represents the task-specific payload for a <see cref="TaskType.Todo"/> open task.
/// </summary>
public class Todo : TaskData {
    /// <summary>
    /// The list of items to be completed for this task.
    /// Each item is enforced to a maximum length of 500 characters. Behavior when exceeded
    /// is controlled by the <c>allowStringTruncation</c> constructor parameter — either
    /// silently truncated or an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    public BoundedStringList Items { get; set; } = [];
    
    /// <summary>
    /// Indicates whether <see cref="Items"/> are ordered by priority,
    /// with the highest priority item first.
    /// </summary>
    public bool IsOrderedByPriority { get; set; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="Todo"/>.
    /// </summary>
    /// <param name="allowStringTruncation">
    /// When <c>true</c>, strings exceeding maximum length constraints are silently truncated.
    /// When <c>false</c> (default), an <see cref="ArgumentException"/> is thrown instead.
    /// This behavior is passed through to <see cref="Items"/> and applies to each item added.
    /// </param>
    public Todo(bool allowStringTruncation = false) {
        Items = new BoundedStringList(allowStringTruncation);
    }
}
