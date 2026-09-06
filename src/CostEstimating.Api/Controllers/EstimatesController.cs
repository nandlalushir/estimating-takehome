using CostEstimating.Application.DTOs;
using CostEstimating.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CostEstimating.Api.Controllers;

[ApiController]
[Route("api/estimates")]
public sealed class EstimatesController : ControllerBase
{
    private readonly EstimateService _service;

    public EstimatesController(EstimateService service) => _service = service;

    [HttpPost("/api/projects/{projectId:guid}/estimates")]
    public async Task<ActionResult<Guid>> Create(
        Guid projectId,
        [FromBody] CreateEstimateRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _service.CreateAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { estimateId = id }, id);
    }

    [HttpGet("{estimateId:guid}")]
    public async Task<ActionResult<EstimateDto>> Get(Guid estimateId, CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(estimateId, cancellationToken));

    [HttpPut("{estimateId:guid}/lines/{catalogueItemId:guid}")]
    public async Task<IActionResult> UpsertLine(
        Guid estimateId,
        Guid catalogueItemId,
        [FromBody] UpsertLineRequest request,
        CancellationToken cancellationToken)
    {
        await _service.AddOrUpdateLineAsync(estimateId, catalogueItemId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{estimateId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid estimateId, CancellationToken cancellationToken)
    {
        await _service.SubmitAsync(estimateId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{estimateId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid estimateId, CancellationToken cancellationToken)
    {
        await _service.ApproveAsync(estimateId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{estimateId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid estimateId, CancellationToken cancellationToken)
    {
        await _service.RejectAsync(estimateId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{estimateId:guid}/return-to-draft")]
    public async Task<IActionResult> ReturnToDraft(Guid estimateId, CancellationToken cancellationToken)
    {
        await _service.ReturnToDraftAsync(estimateId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{estimateId:guid}/description")]
    public async Task<IActionResult> CorrectDescription(
        Guid estimateId,
        [FromBody] CorrectDescriptionRequest request,
        CancellationToken cancellationToken)
    {
        await _service.CorrectDescriptionAsync(estimateId, request, cancellationToken);
        return NoContent();
    }
}
