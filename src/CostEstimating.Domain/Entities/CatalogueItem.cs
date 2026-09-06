namespace CostEstimating.Domain.Entities;

public sealed class CatalogueItem
{
    private CatalogueItem() { }

    public CatalogueItem(Guid id, string code, string description, string unitOfMeasure)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("Unit of measure is required.", nameof(unitOfMeasure));

        Id = id;
        Code = code.Trim().ToUpperInvariant();
        Description = description.Trim();
        UnitOfMeasure = unitOfMeasure.Trim();
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public List<CatalogueRate> Rates { get; private set; } = new();
}
