using CostEstimating.Domain.Exceptions;

namespace CostEstimating.Domain.Entities;

public sealed class EstimateLine
{
    private EstimateLine() { }

    public EstimateLine(
        Guid id,
        Guid estimateId,
        Guid catalogueItemId,
        string description,
        string unitOfMeasure,
        decimal quantity,
        decimal markupPercentage,
        decimal rate,
        decimal labourCost)
    {
        ValidateQuantity(quantity);
        ValidateMarkup(markupPercentage);

        Id = id;
        EstimateId = estimateId;
        CatalogueItemId = catalogueItemId;
        Description = description.Trim();
        UnitOfMeasure = unitOfMeasure.Trim();
        Quantity = quantity;
        MarkupPercentage = markupPercentage;
        Rate = Money(rate);
        LabourCost = Money(labourCost);
        Recalculate();
    }

    public Guid Id { get; private set; }
    public Guid EstimateId { get; private set; }
    public Guid CatalogueItemId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal MarkupPercentage { get; private set; }
    public decimal Rate { get; private set; }
    public decimal LabourCost { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal MarkupAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    public void ChangeQuantityAndMarkup(decimal quantity, decimal markupPercentage)
    {
        ValidateQuantity(quantity);
        ValidateMarkup(markupPercentage);

        Quantity = quantity;
        MarkupPercentage = markupPercentage;
        Recalculate();
    }

    private void Recalculate()
    {
        NetAmount = Money(Quantity * Rate);
        MarkupAmount = Money(NetAmount * MarkupPercentage / 100m);
        TotalAmount = Money(NetAmount + MarkupAmount);
    }

    private static void ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (decimal.Round(quantity, 3) != quantity)
            throw new DomainException("Quantity may have at most 3 decimal places.");
    }

    private static void ValidateMarkup(decimal markupPercentage)
    {
        if (markupPercentage is < 0 or > 100)
            throw new DomainException("Markup percentage must be between 0 and 100.");

        if (decimal.Round(markupPercentage, 2) != markupPercentage)
            throw new DomainException("Markup percentage may have at most 2 decimal places.");
    }

    private static decimal Money(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
