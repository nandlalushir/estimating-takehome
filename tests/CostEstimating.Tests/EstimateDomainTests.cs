using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Enums;
using CostEstimating.Domain.Exceptions;
using Xunit;

namespace CostEstimating.Tests;

public sealed class EstimateDomainTests
{
    private static Estimate CreateEstimate()
    {
        return new Estimate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test estimate",
            new DateOnly(2026, 8, 1),
            Guid.NewGuid());
    }

    [Fact]
    public void AddLine_CalculatesNetMarkupAndTotal()
    {
        var estimate = CreateEstimate();

        estimate.AddOrUpdateLine(
            Guid.NewGuid(), "Concrete", "m3",
            10m, 10m, 1450m, 400m);

        var line = Assert.Single(estimate.Lines);

        Assert.Equal(14500m, line.NetAmount);
        Assert.Equal(1450m, line.MarkupAmount);
        Assert.Equal(15950m, line.TotalAmount);
        Assert.Equal(15950m, estimate.Total);
    }

    [Fact]
    public void AddSameCatalogueItem_UpdatesExistingLine()
    {
        var estimate = CreateEstimate();
        var item = Guid.NewGuid();

        estimate.AddOrUpdateLine(item, "Concrete", "m3", 10m, 10m, 1450m, 400m);
        estimate.AddOrUpdateLine(item, "Concrete", "m3", 20m, 5m, 1450m, 400m);

        var line = Assert.Single(estimate.Lines);
        Assert.Equal(20m, line.Quantity);
        Assert.Equal(5m, line.MarkupPercentage);
    }

    [Fact]
    public void QuantityMustBePositive()
    {
        var estimate = CreateEstimate();

        Assert.Throws<DomainException>(() =>
            estimate.AddOrUpdateLine(Guid.NewGuid(), "Concrete", "m3", 0m, 5m, 100m, 20m));
    }

    [Fact]
    public void MarkupMustBeBetweenZeroAndHundred()
    {
        var estimate = CreateEstimate();

        Assert.Throws<DomainException>(() =>
            estimate.AddOrUpdateLine(Guid.NewGuid(), "Concrete", "m3", 1m, 100.01m, 100m, 20m));
    }

    [Fact]
    public void LifecycleAllowsDraftSubmittedApproved()
    {
        var estimate = CreateEstimate();
        estimate.AddOrUpdateLine(Guid.NewGuid(), "Concrete", "m3", 1m, 0m, 100m, 20m);

        estimate.Submit(new DateOnly(2026, 8, 2));
        Assert.Equal(EstimateStatus.Submitted, estimate.Status);

        estimate.Approve();
        Assert.Equal(EstimateStatus.Approved, estimate.Status);
    }

    [Fact]
    public void CannotSubmitEmptyEstimate()
    {
        var estimate = CreateEstimate();

        Assert.Throws<DomainException>(() => estimate.Submit(new DateOnly(2026, 8, 2)));
    }

    [Fact]
    public void CannotSubmitWithFuturePricingDate()
    {
        var estimate = new Estimate(Guid.NewGuid(), Guid.NewGuid(), "Future", new DateOnly(2026, 9, 10), Guid.NewGuid());
        estimate.AddOrUpdateLine(Guid.NewGuid(), "Concrete", "m3", 1m, 0m, 100m, 20m);

        Assert.Throws<DomainException>(() => estimate.Submit(new DateOnly(2026, 9, 9)));
    }

    [Fact]
    public void ApprovedEstimateCannotChangeLines()
    {
        var estimate = CreateEstimate();
        var item = Guid.NewGuid();
        estimate.AddOrUpdateLine(item, "Concrete", "m3", 1m, 0m, 100m, 20m);
        estimate.Submit(new DateOnly(2026, 8, 2));
        estimate.Approve();

        Assert.Throws<InvalidStateTransitionException>(() =>
            estimate.AddOrUpdateLine(item, "Concrete", "m3", 2m, 0m, 100m, 20m));
    }

    [Fact]
    public void RejectedEstimateCanReturnToDraft()
    {
        var estimate = CreateEstimate();
        estimate.AddOrUpdateLine(Guid.NewGuid(), "Concrete", "m3", 1m, 0m, 100m, 20m);
        estimate.Submit(new DateOnly(2026, 8, 2));
        estimate.Reject();
        estimate.ReturnToDraft();

        Assert.Equal(EstimateStatus.Draft, estimate.Status);
    }

    [Fact]
    public void ApprovedDescriptionCanBeCorrected()
    {
        var estimate = CreateEstimate();
        estimate.AddOrUpdateLine(Guid.NewGuid(), "Concrete", "m3", 1m, 0m, 100m, 20m);
        estimate.Submit(new DateOnly(2026, 8, 2));
        estimate.Approve();

        estimate.CorrectApprovedDescription("Corrected description");

        Assert.Equal("Corrected description", estimate.Description);
    }
}
