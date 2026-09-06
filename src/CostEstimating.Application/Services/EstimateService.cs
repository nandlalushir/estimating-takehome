using CostEstimating.Application.Abstractions;
using CostEstimating.Application.DTOs;
using CostEstimating.Application.Exceptions;
using CostEstimating.Domain.Entities;
using CostEstimating.Domain.Enums;
using CostEstimating.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CostEstimating.Application.Services;

public sealed class EstimateService
{
    private readonly IAppDbContext _db;
    private readonly IAuthorizationService _authorization;
    private readonly RateResolver _rateResolver;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EstimateService> _logger;

    public EstimateService(
        IAppDbContext db,
        IAuthorizationService authorization,
        RateResolver rateResolver,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        ILogger<EstimateService> logger)
    {
        _db = db;
        _authorization = authorization;
        _rateResolver = rateResolver;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProjectDto>> ListProjectsAsync(CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserAsync(cancellationToken);

        return _db.Projects
            .Where(p => _db.ProjectAssignments.Any(a => a.ProjectId == p.Id && a.UserId == user.Id))
            .OrderBy(p => p.Name)
            .Select(p => new ProjectDto(p.Id, p.Name))
            .ToList();
    }

    public async Task<Guid> CreateAsync(
        Guid projectId,
        CreateEstimateRequest request,
        CancellationToken cancellationToken)
    {
        await _authorization.EnsureProjectAccessAsync(projectId, cancellationToken);
        var user = await _authorization.GetCurrentUserAsync(cancellationToken);

        if (user.Role != UserRole.Estimator)
            throw new ForbiddenException("Only estimators can create estimates.");

        var estimate = new Estimate(
            Guid.NewGuid(),
            projectId,
            request.Description,
            request.PricingDate,
            user.Id);

        if (estimate.PricingDate > DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime))
            throw new DomainException("Pricing date cannot be in the future.");

        if (_db is not IEstimateWriter writer)
            throw new InvalidOperationException("The configured data context does not support estimate creation.");

        writer.AddEstimate(estimate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return estimate.Id;
    }

    public async Task<EstimateDto> GetAsync(Guid estimateId, CancellationToken cancellationToken)
    {
        var estimate = _db.Estimates.FirstOrDefault(x => x.Id == estimateId)
            ?? throw new NotFoundException("Estimate not found.");

        await _authorization.EnsureProjectAccessAsync(estimate.ProjectId, cancellationToken);

        var showLabour = await _authorization.CanViewLabourCostAsync(estimate.ProjectId, cancellationToken);

        var lines = (
            from line in _db.EstimateLines
            join item in _db.CatalogueItems on line.CatalogueItemId equals item.Id
            where line.EstimateId == estimate.Id
            orderby line.Id
            select new { line, item }
        ).ToList();

        var resultLines = lines.Select(x => new EstimateLineDto(
            x.line.Id,
            x.line.CatalogueItemId,
            x.item.Code,
            x.line.Description,
            x.line.UnitOfMeasure,
            x.line.Quantity,
            x.line.MarkupPercentage,
            x.line.Rate,
            showLabour ? x.line.LabourCost : null,
            x.line.NetAmount,
            x.line.MarkupAmount,
            x.line.TotalAmount)).ToList();

        var total = decimal.Round(resultLines.Sum(x => x.TotalAmount), 2, MidpointRounding.AwayFromZero);

        return new EstimateDto(
            estimate.Id,
            estimate.ProjectId,
            estimate.Description,
            estimate.PricingDate,
            estimate.Status,
            total,
            resultLines);
    }

    public async Task AddOrUpdateLineAsync(
        Guid estimateId,
        Guid catalogueItemId,
        UpsertLineRequest request,
        CancellationToken cancellationToken)
    {
        var estimate = await GetEstimateEntityAsync(estimateId, cancellationToken);
        await _authorization.EnsureCanEditDraftAsync(estimate, cancellationToken);

        var item = _db.CatalogueItems.FirstOrDefault(x => x.Id == catalogueItemId)
            ?? throw new NotFoundException("Catalogue item not found.");

        var rate = await _rateResolver.ResolveAsync(catalogueItemId, estimate.PricingDate, cancellationToken);

        estimate.AddOrUpdateLine(
            item.Id,
            item.Description,
            item.UnitOfMeasure,
            request.Quantity,
            request.MarkupPercentage,
            rate.Rate,
            rate.LabourCost);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Estimate line added or updated. EstimateId={EstimateId}, CatalogueItemId={CatalogueItemId}, User={User}",
            estimateId, catalogueItemId, _currentUser.Email);
    }

    public async Task SubmitAsync(Guid estimateId, CancellationToken cancellationToken)
    {
        var estimate = await GetEstimateEntityAsync(estimateId, cancellationToken);
        await _authorization.EnsureCanSubmitAsync(estimate, cancellationToken);

        var from = estimate.Status;
        estimate.Submit(DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime));

        var user = await _authorization.GetCurrentUserAsync(cancellationToken);
        _db.AddAudit(estimate.Id, from, estimate.Status, user.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Estimate submitted. EstimateId={EstimateId}, User={User}",
            estimateId, _currentUser.Email);
    }

    public async Task ApproveAsync(Guid estimateId, CancellationToken cancellationToken)
    {
        var estimate = await GetEstimateEntityAsync(estimateId, cancellationToken);
        await _authorization.EnsureCanApproveAsync(estimate, cancellationToken);

        if (estimate.Status == EstimateStatus.Approved)
            return;

        var from = estimate.Status;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            estimate.Approve();

            var user = await _authorization.GetCurrentUserAsync(cancellationToken);
            _db.AddAudit(estimate.Id, from, estimate.Status, user.Id);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ConflictException("The estimate was changed by another request. Retry the operation.", ex);
            }
        }, cancellationToken);

        _logger.LogInformation("Estimate approved. EstimateId={EstimateId}, User={User}",
            estimateId, _currentUser.Email);
    }

    public async Task RejectAsync(Guid estimateId, CancellationToken cancellationToken)
    {
        var estimate = await GetEstimateEntityAsync(estimateId, cancellationToken);
        await _authorization.EnsureCanRejectAsync(estimate, cancellationToken);

        var from = estimate.Status;
        estimate.Reject();

        var user = await _authorization.GetCurrentUserAsync(cancellationToken);
        _db.AddAudit(estimate.Id, from, estimate.Status, user.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReturnToDraftAsync(Guid estimateId, CancellationToken cancellationToken)
    {
        var estimate = await GetEstimateEntityAsync(estimateId, cancellationToken);
        await _authorization.EnsureProjectAccessAsync(estimate.ProjectId, cancellationToken);

        var user = await _authorization.GetCurrentUserAsync(cancellationToken);
        if (user.Role != UserRole.Estimator || estimate.CreatedByUserId != user.Id)
            throw new ForbiddenException("Only the estimate creator can return a rejected estimate to draft.");

        var from = estimate.Status;
        estimate.ReturnToDraft();

        _db.AddAudit(estimate.Id, from, estimate.Status, user.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CorrectDescriptionAsync(Guid estimateId, CorrectDescriptionRequest request, CancellationToken cancellationToken)
    {
        var estimate = await GetEstimateEntityAsync(estimateId, cancellationToken);
        await _authorization.EnsureCanEditApprovedDescriptionAsync(estimate, cancellationToken);
        estimate.CorrectApprovedDescription(request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Estimate> GetEstimateEntityAsync(Guid estimateId, CancellationToken cancellationToken)
    {
        var estimate = _db.Estimates.FirstOrDefault(x => x.Id == estimateId);
        return await Task.FromResult(estimate ?? throw new NotFoundException("Estimate not found."));
    }
}
