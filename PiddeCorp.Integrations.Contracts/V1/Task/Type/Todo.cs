namespace PiddeCorp.Integrations.Contracts.V1.Task.Type;

/// <summary>
/// Represents the task-specific payload for a <see cref="TaskType.Todo"/> open task.
/// </summary>
public class Todo : TaskData {
    /// <summary>
    /// The list of items to be completed for this task.
    /// </summary>
    public List<string> Items { get; set; } = new();
    
    /// <summary>
    /// Indicates whether <see cref="Items"/> are ordered by priority, 
    /// with the highest priority item first.
    /// </summary>
    public bool IsOrderedByPriority { get; set; }
}
