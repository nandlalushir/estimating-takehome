using CostEstimating.Application.Abstractions;
using CostEstimating.Application.Exceptions;
using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Enums;

namespace CostEstimating.Application.Services;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AuthorizationService(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.Email))
            throw new UnauthorizedException("X-User-Email header is required.");

        var user = _db.Users.FirstOrDefault(x => x.Email == _currentUser.Email.Trim().ToLowerInvariant());
        return await Task.FromResult(user ?? throw new UnauthorizedException("The supplied user identity is not registered."));
    }

    public async Task EnsureProjectAccessAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var assigned = _db.ProjectAssignments.Any(x => x.ProjectId == projectId && x.UserId == user.Id);
        if (!assigned)
            throw new ForbiddenException("You are not assigned to this project.");
    }

    public async Task EnsureCanEditDraftAsync(Estimate estimate, CancellationToken cancellationToken)
    {
        await EnsureProjectAccessAsync(estimate.ProjectId, cancellationToken);
        var user = await GetCurrentUserAsync(cancellationToken);

        if (user.Role != UserRole.Estimator)
            throw new ForbiddenException("Only estimators can edit draft estimates.");

        if (estimate.Status != EstimateStatus.Draft)
            throw new ForbiddenException("Only draft estimates can be edited.");
    }

    public async Task EnsureCanSubmitAsync(Estimate estimate, CancellationToken cancellationToken)
    {
        await EnsureProjectAccessAsync(estimate.ProjectId, cancellationToken);
        var user = await GetCurrentUserAsync(cancellationToken);

        if (user.Role != UserRole.Estimator)
            throw new ForbiddenException("Only estimators can submit estimates.");

        if (estimate.CreatedByUserId != user.Id)
            throw new ForbiddenException("Only the estimate creator can submit this estimate.");
    }

    public async Task EnsureCanApproveAsync(Estimate estimate, CancellationToken cancellationToken)
    {
        await EnsureProjectAccessAsync(estimate.ProjectId, cancellationToken);
        var user = await GetCurrentUserAsync(cancellationToken);

        if (user.Role != UserRole.Reviewer)
            throw new ForbiddenException("Only reviewers can approve estimates.");

        if (estimate.CreatedByUserId == user.Id)
            throw new ForbiddenException("A reviewer cannot approve an estimate they created.");
    }

    public async Task EnsureCanRejectAsync(Estimate estimate, CancellationToken cancellationToken)
    {
        await EnsureProjectAccessAsync(estimate.ProjectId, cancellationToken);
        var user = await GetCurrentUserAsync(cancellationToken);

        if (user.Role != UserRole.Reviewer)
            throw new ForbiddenException("Only reviewers can reject estimates.");

        if (estimate.CreatedByUserId == user.Id)
            throw new ForbiddenException("A reviewer cannot reject an estimate they created.");
    }

    public async Task EnsureCanEditApprovedDescriptionAsync(Estimate estimate, CancellationToken cancellationToken)
    {
        await EnsureProjectAccessAsync(estimate.ProjectId, cancellationToken);
        var user = await GetCurrentUserAsync(cancellationToken);
        var assignment = _db.ProjectAssignments.FirstOrDefault(x => x.ProjectId == estimate.ProjectId && x.UserId == user.Id);

        if (assignment?.IsProjectManager != true)
            throw new ForbiddenException("Only the project manager assigned to the project can correct an approved description.");
    }

    public async Task<bool> CanViewLabourCostAsync(Guid projectId, CancellationToken cancellationToken)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var user = await GetCurrentUserAsync(cancellationToken);

        if (user.Role == UserRole.Reviewer)
            return true;

        var assignment = _db.ProjectAssignments.FirstOrDefault(x => x.ProjectId == projectId && x.UserId == user.Id);
        return assignment?.IsProjectManager == true;
    }
}
