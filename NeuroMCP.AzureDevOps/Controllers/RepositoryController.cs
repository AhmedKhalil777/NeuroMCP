using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetFileContent;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetRepository;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetRepositoryDetails;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepositoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public RepositoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List all repositories in a project
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GitRepository>>> ListRepositoriesAsync(
        [FromQuery] string projectId,
        [FromQuery] string? organizationId = null)
    {
        var query = new ListRepositoriesQuery
        {
            ProjectId = projectId,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get repository details
    /// </summary>
    [HttpGet("{repositoryId}")]
    public async Task<ActionResult<GitRepository>> GetRepositoryAsync(
        [FromRoute] string repositoryId,
        [FromQuery] string projectId,
        [FromQuery] string? organizationId = null)
    {
        var query = new GetRepositoryQuery
        {
            RepositoryId = repositoryId,
            ProjectId = projectId,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed repository information including refs and statistics
    /// </summary>
    [HttpGet("{repositoryId}/details")]
    public async Task<ActionResult<GetRepositoryDetailsResult>> GetRepositoryDetailsAsync(
        [FromRoute] string repositoryId,
        [FromQuery] string projectId,
        [FromQuery] bool includeRefs = false,
        [FromQuery] bool includeStatistics = false,
        [FromQuery] string? branchName = null,
        [FromQuery] string? organizationId = null)
    {
        var query = new GetRepositoryDetailsQuery
        {
            RepositoryId = repositoryId,
            ProjectId = projectId,
            IncludeRefs = includeRefs,
            IncludeStatistics = includeStatistics,
            BranchName = branchName,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get content of a file or folder from a repository
    /// </summary>
    [HttpGet("{repositoryId}/content")]
    public async Task<ActionResult<GitItem>> GetFileContentAsync(
        [FromRoute] string repositoryId,
        [FromQuery] string path,
        [FromQuery] string projectId,
        [FromQuery] string? organizationId = null)
    {
        var query = new GetFileContentQuery
        {
            RepositoryId = repositoryId,
            Path = path,
            ProjectId = projectId,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}