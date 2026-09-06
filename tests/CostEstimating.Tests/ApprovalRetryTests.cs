using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Enums;
using Xunit;

namespace CostEstimating.Tests;

public sealed class ApprovalRetryTests
{
    [Fact]
    public void SecondApprovalCannotCreateAnotherTransition()
    {
        var estimate = new Estimate(Guid.NewGuid(), Guid.NewGuid(), "Estimate", new DateOnly(2026, 8, 1), Guid.NewGuid());
        estimate.AddOrUpdateLine(Guid.NewGuid(), "Concrete", "m3", 1m, 0m, 100m, 20m);
        estimate.Submit(new DateOnly(2026, 8, 1));
        estimate.Approve();

        Assert.Equal(EstimateStatus.Approved, estimate.Status);
        Assert.Throws<CostEstimating.Domain.Exceptions.InvalidStateTransitionException>(() => estimate.Approve());
    }
}
