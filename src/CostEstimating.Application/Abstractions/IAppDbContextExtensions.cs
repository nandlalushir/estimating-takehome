using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Enums;

namespace CostEstimating.Application.Abstractions;

public static class AppDbContextExtensions
{
    public static void AddAudit(this IAppDbContext db, Guid estimateId, EstimateStatus fromStatus, EstimateStatus toStatus, Guid userId)
    {
        if (db is IAuditWriter writer)
            writer.AddAudit(new EstimateAuditEvent(Guid.NewGuid(), estimateId, fromStatus, toStatus, userId));
        else
            throw new InvalidOperationException("The configured data context does not support audit writes.");
    }
}

public interface IAuditWriter
{
    void AddAudit(EstimateAuditEvent auditEvent);
}


public interface IEstimateWriter
{
    void AddEstimate(CostEstimating.Domain.Entities.Estimate estimate);
}
