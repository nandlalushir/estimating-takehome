namespace CostEstimating.Domain.Entities;

public sealed class CatalogueRate
{
    private CatalogueRate() { }

    public CatalogueRate(Guid id, Guid catalogueItemId, DateOnly effectiveFrom, decimal rate, decimal labourCost)
    {
        if (rate < 0) throw new ArgumentOutOfRangeException(nameof(rate));
        if (labourCost < 0) throw new ArgumentOutOfRangeException(nameof(labourCost));

        Id = id;
        CatalogueItemId = catalogueItemId;
        EffectiveFrom = effectiveFrom;
        Rate = decimal.Round(rate, 2, MidpointRounding.AwayFromZero);
        LabourCost = decimal.Round(labourCost, 2, MidpointRounding.AwayFromZero);
    }

    public Guid Id { get; private set; }
    public Guid CatalogueItemId { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public decimal Rate { get; private set; }
    public decimal LabourCost { get; private set; }
}
