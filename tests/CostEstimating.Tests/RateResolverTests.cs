using CostEstimating.Application.Abstractions;
using CostEstimating.Application.Services;
using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Exceptions;
using Xunit;

namespace CostEstimating.Tests;

public sealed class RateResolverTests
{
    [Fact]
    public async Task ResolvesLatestRateEffectiveOnPricingDate()
    {
        var itemId = Guid.NewGuid();
        var db = new FakeDb();
        db.Rates.Add(
            new CatalogueRate(Guid.NewGuid(), itemId, new DateOnly(2026, 1, 1), 1450m, 400m));
        db.Rates.Add(
            new CatalogueRate(Guid.NewGuid(), itemId, new DateOnly(2026, 7, 1), 1512.50m, 420m));

        var resolver = new RateResolver(db);

        var rate = await resolver.ResolveAsync(itemId, new DateOnly(2026, 8, 1), CancellationToken.None);

        Assert.Equal(1512.50m, rate.Rate);
        Assert.Equal(420m, rate.LabourCost);
    }

    [Fact]
    public async Task DoesNotUseFutureRate()
    {
        var itemId = Guid.NewGuid();
        var db = new FakeDb();
        db.Rates.Add(new CatalogueRate(Guid.NewGuid(), itemId, new DateOnly(2026, 7, 1), 1512.50m, 420m));

        var resolver = new RateResolver(db);

        await Assert.ThrowsAsync<CostEstimating.Application.Exceptions.NotFoundException>(() =>
            resolver.ResolveAsync(itemId, new DateOnly(2026, 6, 30), CancellationToken.None));
    }

    private sealed class FakeDb : IAppDbContext
    {
        public List<CatalogueRate> Rates { get; } = new();

        public IQueryable<CostEstimating.Domain.Entities.Project> Projects => Array.Empty<CostEstimating.Domain.Entities.Project>().AsQueryable();
        public IQueryable<CostEstimating.Domain.Entities.User> Users => Array.Empty<CostEstimating.Domain.Entities.User>().AsQueryable();
        public IQueryable<CostEstimating.Domain.Entities.ProjectAssignment> ProjectAssignments => Array.Empty<CostEstimating.Domain.Entities.ProjectAssignment>().AsQueryable();
        public IQueryable<CostEstimating.Domain.Entities.CatalogueItem> CatalogueItems => Array.Empty<CostEstimating.Domain.Entities.CatalogueItem>().AsQueryable();
        public IQueryable<CostEstimating.Domain.Entities.CatalogueRate> CatalogueRates => Rates.AsQueryable();
        public IQueryable<CostEstimating.Domain.Entities.Estimate> Estimates => Array.Empty<CostEstimating.Domain.Entities.Estimate>().AsQueryable();
        public IQueryable<CostEstimating.Domain.Entities.EstimateLine> EstimateLines => Array.Empty<CostEstimating.Domain.Entities.EstimateLine>().AsQueryable();
        public IQueryable<CostEstimating.Domain.Entities.EstimateAuditEvent> EstimateAuditEvents => Array.Empty<CostEstimating.Domain.Entities.EstimateAuditEvent>().AsQueryable();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
