using CostEstimating.Application.Abstractions;
using CostEstimating.Application.Exceptions;
using CostEstimating.Application.Services;
using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Enums;
using Xunit;

namespace CostEstimating.Tests;

public sealed class AuthorizationServiceTests
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid EstimatorId = Guid.NewGuid();
    private static readonly Guid ReviewerId = Guid.NewGuid();
    private static readonly Guid ViewerId = Guid.NewGuid();
    private static readonly Guid ManagerId = Guid.NewGuid();

    private static FakeDb CreateDb()
    {
        var db = new FakeDb();
        db.UsersList.Add(
            new User(EstimatorId, "estimator@example.com", UserRole.Estimator));
        db.UsersList.Add(
            new User(ReviewerId, "reviewer@example.com", UserRole.Reviewer));
        db.UsersList.Add(
            new User(ViewerId, "viewer@example.com", UserRole.Viewer));
        db.UsersList.Add(
            new User(ManagerId, "manager@example.com", UserRole.Estimator));

        db.ProjectsList.Add(new Project(ProjectId, "Project"));
        db.AssignmentsList.Add(
            new ProjectAssignment(ProjectId, EstimatorId));
        db.AssignmentsList.Add(
            new ProjectAssignment(ProjectId, ReviewerId));
        db.AssignmentsList.Add(
            new ProjectAssignment(ProjectId, ViewerId));
        db.AssignmentsList.Add(
            new ProjectAssignment(ProjectId, ManagerId, true));

        return db;
    }

    [Fact]
    public async Task ViewerCannotEditDraft()
    {
        var db = CreateDb();
        var current = new FakeCurrentUser("viewer@example.com");
        var service = new AuthorizationService(db, current);
        var estimate = new Estimate(Guid.NewGuid(), ProjectId, "Estimate", new DateOnly(2026, 8, 1), EstimatorId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.EnsureCanEditDraftAsync(estimate, CancellationToken.None));
    }

    [Fact]
    public async Task ReviewerCannotApproveOwnEstimate()
    {
        var db = CreateDb();
        var current = new FakeCurrentUser("reviewer@example.com");
        var service = new AuthorizationService(db, current);
        var estimate = new Estimate(Guid.NewGuid(), ProjectId, "Estimate", new DateOnly(2026, 8, 1), ReviewerId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.EnsureCanApproveAsync(estimate, CancellationToken.None));
    }

    [Fact]
    public async Task UnassignedUserCannotAccessProject()
    {
        var db = CreateDb();
        var outsider = new User(Guid.NewGuid(), "outsider@example.com", UserRole.Viewer);
        db.UsersList.Add(outsider);
        var service = new AuthorizationService(db, new FakeCurrentUser(outsider.Email));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.EnsureProjectAccessAsync(ProjectId, CancellationToken.None));
    }

    [Fact]
    public async Task EstimatorCannotSeeLabourCost()
    {
        var db = CreateDb();
        var service = new AuthorizationService(db, new FakeCurrentUser("estimator@example.com"));

        Assert.False(await service.CanViewLabourCostAsync(ProjectId, CancellationToken.None));
    }

    [Fact]
    public async Task ReviewerCanSeeLabourCost()
    {
        var db = CreateDb();
        var service = new AuthorizationService(db, new FakeCurrentUser("reviewer@example.com"));

        Assert.True(await service.CanViewLabourCostAsync(ProjectId, CancellationToken.None));
    }

    [Fact]
    public async Task ProjectManagerCanSeeLabourCost()
    {
        var db = CreateDb();
        var service = new AuthorizationService(db, new FakeCurrentUser("manager@example.com"));

        Assert.True(await service.CanViewLabourCostAsync(ProjectId, CancellationToken.None));
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public FakeCurrentUser(string email) => Email = email;
        public string Email { get; }
    }

    private sealed class FakeDb : IAppDbContext
    {
        public List<Project> ProjectsList { get; } = new();
        public List<User> UsersList { get; } = new();
        public List<ProjectAssignment> AssignmentsList { get; } = new();
        public List<CatalogueItem> CatalogueItemsList { get; } = new();
        public List<CatalogueRate> CatalogueRatesList { get; } = new();
        public List<Estimate> EstimatesList { get; } = new();
        public List<EstimateLine> EstimateLinesList { get; } = new();
        public List<EstimateAuditEvent> AuditList { get; } = new();

        public IQueryable<Project> Projects => ProjectsList.AsQueryable();
        public IQueryable<User> Users => UsersList.AsQueryable();
        public IQueryable<ProjectAssignment> ProjectAssignments => AssignmentsList.AsQueryable();
        public IQueryable<CatalogueItem> CatalogueItems => CatalogueItemsList.AsQueryable();
        public IQueryable<CatalogueRate> CatalogueRates => CatalogueRatesList.AsQueryable();
        public IQueryable<Estimate> Estimates => EstimatesList.AsQueryable();
        public IQueryable<EstimateLine> EstimateLines => EstimateLinesList.AsQueryable();
        public IQueryable<EstimateAuditEvent> EstimateAuditEvents => AuditList.AsQueryable();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }
}
