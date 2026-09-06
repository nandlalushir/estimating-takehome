using CostEstimating.Domain.Enums;

namespace CostEstimating.Application.DTOs;

public sealed record ProjectDto(Guid Id, string Name);

public sealed record EstimateLineDto(
    Guid Id,
    Guid CatalogueItemId,
    string Code,
    string Description,
    string UnitOfMeasure,
    decimal Quantity,
    decimal MarkupPercentage,
    decimal Rate,
    decimal? LabourCost,
    decimal NetAmount,
    decimal MarkupAmount,
    decimal TotalAmount);

public sealed record EstimateDto(
    Guid Id,
    Guid ProjectId,
    string Description,
    DateOnly PricingDate,
    EstimateStatus Status,
    decimal Total,
    IReadOnlyList<EstimateLineDto> Lines);

public sealed record CreateEstimateRequest(string Description, DateOnly PricingDate);

public sealed record UpsertLineRequest(decimal Quantity, decimal MarkupPercentage);

public sealed record SubmitEstimateRequest();

public sealed record CorrectDescriptionRequest(string Description);

public sealed record ErrorResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    string TraceId);
