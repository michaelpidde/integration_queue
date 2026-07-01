namespace PiddeCorp.Integrations.Contracts.V1.Task.Type;

/// <summary>
/// Represents the task-specific payload for a <see cref="TaskType.MountainDew"/> open task.
/// </summary>
public class MountainDew : TaskData {
    private const int _flavorMaxLength = 500;
    private const int _commentMaxLength = 500;
    
    /// <summary>
    /// Controls string truncation behavior for properties with length constraints.
    /// When <c>true</c>, strings exceeding the maximum length are silently truncated.
    /// When <c>false</c>, an <see cref="ArgumentException"/> is thrown instead.
    /// Defaults to <c>false</c>. Must be set via the constructor before assigning
    /// string properties to ensure correct behavior.
    /// </summary>
    public bool AllowStringTruncation { get; init; } = false;
    
    /// <summary>
    /// Indicates whether this task requires an extreme level of caffeine to complete.
    /// </summary>
    public bool ExtremeCaffeine { get; set; }
    
    /// <summary>
    /// The Mountain Dew flavor required to complete this task.
    /// Maximum length is 500 characters. Behavior when exceeded is controlled
    /// by <see cref="AllowStringTruncation"/> — either silently truncated or
    /// an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    public string Flavor {
        get;
        set {
            if(value?.Length > _flavorMaxLength) {
                if(AllowStringTruncation) {
                    field = value[.._flavorMaxLength];
                } else {
                    throw new ArgumentException("Flavor length must be less than or equal to " +
                                                _flavorMaxLength
                    );
                }
            }
        }
    } = string.Empty;
    
    /// <summary>
    /// Any additional comments about this task's Mountain Dew requirements.
    /// Maximum length is 500 characters. Behavior when exceeded is controlled
    /// by <see cref="AllowStringTruncation"/> — either silently truncated or
    /// an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    public string Comment {
        get;
        set {
            if(value?.Length > _commentMaxLength) {
                if(AllowStringTruncation) {
                    field = value[.._commentMaxLength];
                } else {
                    throw new ArgumentException("Comment length must be less than or equal to " +
                                                _commentMaxLength
                    );
                }
            }
        }
    } = string.Empty;
    
    /// <summary>
    /// Initializes a new instance of <see cref="MountainDew"/>.
    /// </summary>
    /// <param name="allowStringTruncation">
    /// When <c>true</c>, strings exceeding maximum length constraints are silently truncated.
    /// When <c>false</c> (default), an <see cref="ArgumentException"/> is thrown instead.
    /// Must be set before assigning string properties.
    /// </param>
    public MountainDew(bool allowStringTruncation = false) => AllowStringTruncation = allowStringTruncation;
}
