using PiddeCorp.Integrations.Contracts.V1.Task;

namespace PiddeCorp.TaskManager.API.Models;

public class OpenTaskDto {
    public Guid Id { get; init; }
    public DateTime Created { get; init; }
    public TaskType Type { get; init; }
    public string TypeName => Type.ToString();
    public DateTime Due { get; init; }
    public string WorkflowStatus { get; init; }
    public string? AssignedTo { get; init; }
}
