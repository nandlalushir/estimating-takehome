using CostEstimating.Domain.Entities;

namespace CostEstimating.Application.Abstractions;

public interface IAppDbContext
{
    IQueryable<Project> Projects { get; }
    IQueryable<User> Users { get; }
    IQueryable<ProjectAssignment> ProjectAssignments { get; }
    IQueryable<CatalogueItem> CatalogueItems { get; }
    IQueryable<CatalogueRate> CatalogueRates { get; }
    IQueryable<Estimate> Estimates { get; }
    IQueryable<EstimateLine> EstimateLines { get; }
    IQueryable<EstimateAuditEvent> EstimateAuditEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
