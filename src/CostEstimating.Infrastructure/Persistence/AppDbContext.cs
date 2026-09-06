using CostEstimating.Application.Abstractions;
using CostEstimating.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CostEstimating.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IAppDbContext, IAuditWriter, IEstimateWriter, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> ProjectsSet => Set<Project>();
    public DbSet<User> UsersSet => Set<User>();
    public DbSet<ProjectAssignment> ProjectAssignmentsSet => Set<ProjectAssignment>();
    public DbSet<CatalogueItem> CatalogueItemsSet => Set<CatalogueItem>();
    public DbSet<CatalogueRate> CatalogueRatesSet => Set<CatalogueRate>();
    public DbSet<Estimate> EstimatesSet => Set<Estimate>();
    public DbSet<EstimateLine> EstimateLinesSet => Set<EstimateLine>();
    public DbSet<EstimateAuditEvent> EstimateAuditEventsSet => Set<EstimateAuditEvent>();

    IQueryable<Project> IAppDbContext.Projects => ProjectsSet;
    IQueryable<User> IAppDbContext.Users => UsersSet;
    IQueryable<ProjectAssignment> IAppDbContext.ProjectAssignments => ProjectAssignmentsSet;
    IQueryable<CatalogueItem> IAppDbContext.CatalogueItems => CatalogueItemsSet;
    IQueryable<CatalogueRate> IAppDbContext.CatalogueRates => CatalogueRatesSet;
    IQueryable<Estimate> IAppDbContext.Estimates => EstimatesSet;
    IQueryable<EstimateLine> IAppDbContext.EstimateLines => EstimateLinesSet;
    IQueryable<EstimateAuditEvent> IAppDbContext.EstimateAuditEvents => EstimateAuditEventsSet;

    void IAuditWriter.AddAudit(EstimateAuditEvent auditEvent) => EstimateAuditEventsSet.Add(auditEvent);
    void IEstimateWriter.AddEstimate(Estimate estimate) => EstimatesSet.Add(estimate);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(b =>
        {
            b.ToTable("projects");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<ProjectAssignment>(b =>
        {
            b.ToTable("project_assignments");
            b.HasKey(x => new { x.ProjectId, x.UserId });
            b.Property(x => x.IsProjectManager).IsRequired();
            b.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<CatalogueItem>(b =>
        {
            b.ToTable("catalogue_items");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Description).HasMaxLength(500).IsRequired();
            b.Property(x => x.UnitOfMeasure).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<CatalogueRate>(b =>
        {
            b.ToTable("catalogue_rates");
            b.HasKey(x => x.Id);
            b.Property(x => x.EffectiveFrom).IsRequired();
            b.Property(x => x.Rate).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.LabourCost).HasPrecision(18, 2).IsRequired();
            b.HasIndex(x => new { x.CatalogueItemId, x.EffectiveFrom });
            b.HasOne<CatalogueItem>().WithMany(x => x.Rates).HasForeignKey(x => x.CatalogueItemId);
        });

        modelBuilder.Entity<Estimate>(b =>
        {
            b.ToTable("estimates");
            b.HasKey(x => x.Id);
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.PricingDate).IsRequired();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            b.Property(x => x.Version).IsConcurrencyToken();
            b.HasIndex(x => x.ProjectId);
            b.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId);
        });

        modelBuilder.Entity<EstimateLine>(b =>
        {
            b.ToTable("estimate_lines");
            b.HasKey(x => x.Id);
            b.Property(x => x.Description).HasMaxLength(500).IsRequired();
            b.Property(x => x.UnitOfMeasure).HasMaxLength(30).IsRequired();
            b.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
            b.Property(x => x.MarkupPercentage).HasPrecision(5, 2).IsRequired();
            b.Property(x => x.Rate).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.LabourCost).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.NetAmount).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.MarkupAmount).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.TotalAmount).HasPrecision(18, 2).IsRequired();
            b.HasIndex(x => new { x.EstimateId, x.CatalogueItemId }).IsUnique();
            b.HasIndex(x => x.EstimateId);
            b.HasOne<Estimate>().WithMany().HasForeignKey(x => x.EstimateId);
            b.HasOne<CatalogueItem>().WithMany().HasForeignKey(x => x.CatalogueItemId);
        });

        modelBuilder.Entity<EstimateAuditEvent>(b =>
        {
            b.ToTable("estimate_audit_events");
            b.HasKey(x => x.Id);
            b.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            b.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            b.Property(x => x.ChangedAtUtc).IsRequired();
            b.HasIndex(x => x.EstimateId);
        });
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        await action();
        await transaction.CommitAsync(cancellationToken);
    }
}
