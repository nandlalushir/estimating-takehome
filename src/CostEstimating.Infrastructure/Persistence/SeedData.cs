using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CostEstimating.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.UsersSet.AnyAsync(cancellationToken))
            return;

        var estimator = new User(Guid.Parse("10000000-0000-0000-0000-000000000001"), "estimator@example.com", UserRole.Estimator);
        var reviewer = new User(Guid.Parse("10000000-0000-0000-0000-000000000002"), "reviewer@example.com", UserRole.Reviewer);
        var viewer = new User(Guid.Parse("10000000-0000-0000-0000-000000000003"), "viewer@example.com", UserRole.Viewer);
        var manager = new User(Guid.Parse("10000000-0000-0000-0000-000000000004"), "manager@example.com", UserRole.Estimator);

        var project = new Project(Guid.Parse("20000000-0000-0000-0000-000000000001"), "Office Building - Mumbai");

        var concrete = new CatalogueItem(Guid.Parse("30000000-0000-0000-0000-000000000001"), "CONC-C30", "Concrete C30", "m3");
        var steel = new CatalogueItem(Guid.Parse("30000000-0000-0000-0000-000000000002"), "STEEL-REBAR", "Reinforcement Steel", "kg");
        var labour = new CatalogueItem(Guid.Parse("30000000-0000-0000-0000-000000000003"), "LAB-GEN", "General Labour", "hr");

        db.UsersSet.AddRange(estimator, reviewer, viewer, manager);
        db.ProjectsSet.Add(project);
        db.ProjectAssignmentsSet.AddRange(
            new ProjectAssignment(project.Id, estimator.Id),
            new ProjectAssignment(project.Id, reviewer.Id),
            new ProjectAssignment(project.Id, viewer.Id),
            new ProjectAssignment(project.Id, manager.Id, true));

        db.CatalogueItemsSet.AddRange(concrete, steel, labour);
        db.CatalogueRatesSet.AddRange(
            new CatalogueRate(Guid.NewGuid(), concrete.Id, new DateOnly(2026, 1, 1), 1450m, 400m),
            new CatalogueRate(Guid.NewGuid(), concrete.Id, new DateOnly(2026, 7, 1), 1512.50m, 420m),
            new CatalogueRate(Guid.NewGuid(), steel.Id, new DateOnly(2026, 1, 1), 92.50m, 18m),
            new CatalogueRate(Guid.NewGuid(), labour.Id, new DateOnly(2026, 1, 1), 650m, 650m));

        var estimate = new Estimate(
            Guid.Parse("40000000-0000-0000-0000-000000000001"),
            project.Id,
            "Office construction estimate",
            new DateOnly(2026, 8, 1),
            estimator.Id);

        db.EstimatesSet.Add(estimate);
        await db.SaveChangesAsync(cancellationToken);

        estimate.AddOrUpdateLine(concrete.Id, concrete.Description, concrete.UnitOfMeasure, 10m, 10m, 1512.50m, 420m);
        estimate.AddOrUpdateLine(steel.Id, steel.Description, steel.UnitOfMeasure, 1000m, 5m, 92.50m, 18m);
        await db.SaveChangesAsync(cancellationToken);
    }
}
