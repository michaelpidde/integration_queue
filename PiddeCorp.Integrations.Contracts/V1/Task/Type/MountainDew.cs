namespace PiddeCorp.Integrations.Contracts.V1.Task.Type;

/// <summary>
/// Represents the task-specific payload for a <see cref="TaskType.MountainDew"/> open task.
/// </summary>
public class MountainDew : TaskData {
    /// <summary>
    /// Indicates whether this task requires an extreme level of caffeine to complete.
    /// </summary>
    public bool ExtremeCaffeine { get; set; }
    
    /// <summary>
    /// The Mountain Dew flavor required to complete this task.
    /// </summary>
    public string Flavor { get; set; } = string.Empty;
    
    /// <summary>
    /// Any additional comments about this task's Mountain Dew requirements.
    /// </summary>
    public string Comment { get; set; } = string.Empty;
}
