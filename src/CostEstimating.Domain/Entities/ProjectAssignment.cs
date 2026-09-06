namespace CostEstimating.Domain.Entities;

public sealed class ProjectAssignment
{
    private ProjectAssignment() { }

    public ProjectAssignment(Guid projectId, Guid userId, bool isProjectManager = false)
    {
        ProjectId = projectId;
        UserId = userId;
        IsProjectManager = isProjectManager;
    }

    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsProjectManager { get; private set; }
}
