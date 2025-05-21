using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.Core.WebApi;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProject;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProjectDetails;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListProjects;

namespace NeuroMCP.AzureDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List all projects in an organization
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamProjectReference>>> ListProjectsAsync(
        [FromQuery] string? organizationId = null)
    {
        var query = new ListProjectsQuery
        {
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get project details
    /// </summary>
    [HttpGet("{projectId}")]
    public async Task<ActionResult<TeamProject>> GetProjectAsync(
        [FromRoute] string projectId,
        [FromQuery] string? organizationId = null)
    {
        var query = new GetProjectQuery
        {
            ProjectId = projectId,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get comprehensive project details including teams, process, etc.
    /// </summary>
    [HttpGet("{projectId}/details")]
    public async Task<ActionResult<GetProjectDetailsResult>> GetProjectDetailsAsync(
        [FromRoute] string projectId,
        [FromQuery] bool includeTeams = false,
        [FromQuery] bool includeProcess = false,
        [FromQuery] bool includeWorkItemTypes = false,
        [FromQuery] string? organizationId = null)
    {
        var query = new GetProjectDetailsQuery
        {
            ProjectId = projectId,
            IncludeTeams = includeTeams,
            IncludeProcess = includeProcess,
            IncludeWorkItemTypes = includeWorkItemTypes,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}