using CostEstimating.Domain.Enums;

namespace CostEstimating.Domain.Entities;

public sealed class EstimateAuditEvent
{
    private EstimateAuditEvent() { }

    public EstimateAuditEvent(Guid id, Guid estimateId, EstimateStatus fromStatus, EstimateStatus toStatus, Guid changedByUserId)
    {
        Id = id;
        EstimateId = estimateId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedByUserId = changedByUserId;
        ChangedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid EstimateId { get; private set; }
    public EstimateStatus FromStatus { get; private set; }
    public EstimateStatus ToStatus { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
}
