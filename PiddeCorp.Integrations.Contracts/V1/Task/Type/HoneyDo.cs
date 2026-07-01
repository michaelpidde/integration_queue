namespace PiddeCorp.Integrations.Contracts.V1.Task.Type;

/// <summary>
/// Represents the task-specific payload for a <see cref="TaskType.HoneyDo"/> open task.
/// </summary>
public class HoneyDo : TaskData {
    private const int _descriptionMaxLength = 500;
    
    /// <summary>
    /// Controls string truncation behavior for properties with length constraints.
    /// When <c>true</c>, strings exceeding the maximum length are silently truncated.
    /// When <c>false</c>, an <see cref="ArgumentException"/> is thrown instead.
    /// Defaults to <c>false</c>. Must be set via the constructor before assigning
    /// string properties to ensure correct behavior.
    /// </summary>
    public bool AllowStringTruncation { get; init; } = false;
    
    /// <summary>
    /// Indicates whether this task is urgent. Always <c>true</c>.
    /// Attempts to set this to <c>false</c> will be silently ignored,
    /// because it's always urgent.
    /// </summary>
    public bool IsUrgent {
        get;
        set => field = true;
    } = true;
    
    /// <summary>
    /// A description of the task to be completed.
    /// Maximum length is 500 characters. Behavior when exceeded is controlled
    /// by <see cref="AllowStringTruncation"/> — either silently truncated or
    /// an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    public string Description {
        get;
        set {
            if(value?.Length > _descriptionMaxLength) {
                if(AllowStringTruncation) {
                    field = value[.._descriptionMaxLength];
                } else {
                    throw new ArgumentException("Description length must be less than or equal to " +
                                                _descriptionMaxLength
                    );
                }
            }
        }
    } = string.Empty;
    
    /// <summary>
    /// Initializes a new instance of <see cref="HoneyDo"/>.
    /// </summary>
    /// <param name="allowStringTruncation">
    /// When <c>true</c>, strings exceeding maximum length constraints are silently truncated.
    /// When <c>false</c> (default), an <see cref="ArgumentException"/> is thrown instead.
    /// Must be set before assigning string properties.
    /// </param>
    public HoneyDo(bool allowStringTruncation = false) => AllowStringTruncation = allowStringTruncation;
}
