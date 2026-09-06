using CostEstimating.Domain.Enums;
using CostEstimating.Domain.Exceptions;

namespace CostEstimating.Domain.Entities;

public sealed class Estimate
{
    private Estimate() { }

    public Estimate(Guid id, Guid projectId, string description, DateOnly pricingDate, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        Id = id;
        ProjectId = projectId;
        Description = description.Trim();
        PricingDate = pricingDate;
        Status = EstimateStatus.Draft;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateOnly PricingDate { get; private set; }
    public EstimateStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }

    private readonly List<EstimateLine> _lines = new();
    public IReadOnlyCollection<EstimateLine> Lines => _lines.AsReadOnly();

    public decimal Total => decimal.Round(_lines.Sum(x => x.TotalAmount), 2, MidpointRounding.AwayFromZero);

    public void AddOrUpdateLine(
        Guid catalogueItemId,
        string description,
        string unitOfMeasure,
        decimal quantity,
        decimal markupPercentage,
        decimal rate,
        decimal labourCost)
    {
        EnsureEditable();

        var existing = _lines.SingleOrDefault(x => x.CatalogueItemId == catalogueItemId);
        if (existing is not null)
        {
            existing.ChangeQuantityAndMarkup(quantity, markupPercentage);
        }
        else
        {
            _lines.Add(new EstimateLine(
                Guid.NewGuid(),
                Id,
                catalogueItemId,
                description,
                unitOfMeasure,
                quantity,
                markupPercentage,
                rate,
                labourCost));
        }

        Touch();
    }

    public void Submit(DateOnly today)
    {
        if (Status != EstimateStatus.Draft)
            throw new InvalidStateTransitionException("Only draft estimates can be submitted.");

        if (_lines.Count == 0)
            throw new DomainException("An estimate must contain at least one line before submission.");

        if (PricingDate > today)
            throw new DomainException("Pricing date cannot be in the future.");

        Status = EstimateStatus.Submitted;
        Touch();
    }

    public void Approve()
    {
        if (Status != EstimateStatus.Submitted)
            throw new InvalidStateTransitionException("Only submitted estimates can be approved.");

        Status = EstimateStatus.Approved;
        Touch();
    }

    public void Reject()
    {
        if (Status != EstimateStatus.Submitted)
            throw new InvalidStateTransitionException("Only submitted estimates can be rejected.");

        Status = EstimateStatus.Rejected;
        Touch();
    }

    public void ReturnToDraft()
    {
        if (Status != EstimateStatus.Rejected)
            throw new InvalidStateTransitionException("Only rejected estimates can return to draft.");

        Status = EstimateStatus.Draft;
        Touch();
    }

    public void CorrectApprovedDescription(string description)
    {
        if (Status != EstimateStatus.Approved)
            throw new InvalidStateTransitionException("Only approved estimates can use the approved-description correction path.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        Description = description.Trim();
        Touch();
    }

    private void EnsureEditable()
    {
        if (Status != EstimateStatus.Draft)
            throw new InvalidStateTransitionException("Only draft estimates can be edited.");
    }

    private void Touch() { UpdatedAtUtc = DateTime.UtcNow; Version++; }
}
