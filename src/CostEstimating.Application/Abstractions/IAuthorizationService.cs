using CostEstimating.Domain.Entities;

namespace CostEstimating.Application.Abstractions;

public interface IAuthorizationService
{
    Task<User> GetCurrentUserAsync(CancellationToken cancellationToken);
    Task EnsureProjectAccessAsync(Guid projectId, CancellationToken cancellationToken);
    Task EnsureCanEditDraftAsync(Estimate estimate, CancellationToken cancellationToken);
    Task EnsureCanSubmitAsync(Estimate estimate, CancellationToken cancellationToken);
    Task EnsureCanApproveAsync(Estimate estimate, CancellationToken cancellationToken);
    Task EnsureCanRejectAsync(Estimate estimate, CancellationToken cancellationToken);
    Task EnsureCanEditApprovedDescriptionAsync(Estimate estimate, CancellationToken cancellationToken);
    Task<bool> CanViewLabourCostAsync(Guid projectId, CancellationToken cancellationToken);
}
