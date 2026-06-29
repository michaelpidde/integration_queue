namespace PiddeCorp.Integrations.Contracts.V1.Task.Type;

/// <summary>
/// Represents the task-specific payload for a <see cref="TaskType.HoneyDo"/> open task.
/// </summary>
public class HoneyDo : TaskData {
    private bool _isUrgent = true;
    
    /// <summary>
    /// Indicates whether this task is urgent. Always <c>true</c>. 
    /// Attempts to set this to <c>false</c> will be silently ignored, 
    /// because it's always urgent.
    /// </summary>
    public bool IsUrgent {
        get => _isUrgent;
        set => _isUrgent = true;
    }
    
    /// <summary>
    /// A description of the task to be completed.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
