using CostEstimating.Application.Abstractions;
using CostEstimating.Application.Exceptions;
using CostEstimating.Domain.Entities;

namespace CostEstimating.Application.Services;

public sealed class RateResolver
{
    private readonly IAppDbContext _db;

    public RateResolver(IAppDbContext db) => _db = db;

    public async Task<CatalogueRate> ResolveAsync(
        Guid catalogueItemId,
        DateOnly pricingDate,
        CancellationToken cancellationToken)
    {
        var rate = _db.CatalogueRates
            .Where(x => x.CatalogueItemId == catalogueItemId && x.EffectiveFrom <= pricingDate)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();

        if (rate is null)
            throw new NotFoundException($"No catalogue rate exists for item {catalogueItemId} on pricing date {pricingDate:yyyy-MM-dd}.");

        return await Task.FromResult(rate);
    }
}
