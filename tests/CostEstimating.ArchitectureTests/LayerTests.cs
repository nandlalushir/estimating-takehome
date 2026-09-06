using NetArchTest.Rules;
using Xunit;

namespace CostEstimating.ArchitectureTests;

public sealed class LayerTests
{
    [Fact]
    public void DomainMustNotDependOnAspNetOrEfCore()
    {
        var result = Types.InAssembly(typeof(CostEstimating.Domain.Entities.Estimate).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames is null ? "Architecture rule failed." : string.Join(Environment.NewLine, result.FailingTypeNames));
    }

    [Fact]
    public void DomainMustNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(typeof(CostEstimating.Domain.Entities.Estimate).Assembly)
            .ShouldNot()
            .HaveDependencyOn("CostEstimating.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames is null ? "Architecture rule failed." : string.Join(Environment.NewLine, result.FailingTypeNames));
    }
}
