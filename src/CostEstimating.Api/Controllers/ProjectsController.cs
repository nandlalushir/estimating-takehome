using CostEstimating.Application.DTOs;
using CostEstimating.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CostEstimating.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly EstimateService _service;

    public ProjectsController(EstimateService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _service.ListProjectsAsync(cancellationToken));
}
