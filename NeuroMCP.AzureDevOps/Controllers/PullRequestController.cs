using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.MediatR.Commands.CreatePullRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PullRequestController : ControllerBase
{
    private readonly IMediator _mediator;

    public PullRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new pull request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GitPullRequest>> CreatePullRequestAsync(
        [FromBody] CreatePullRequestRequest request)
    {
        var command = new CreatePullRequestCommand
        {
            RepositoryId = request.RepositoryId,
            SourceRefName = request.SourceRefName,
            TargetRefName = request.TargetRefName,
            Title = request.Title,
            Description = request.Description,
            IsDraft = request.IsDraft,
            Reviewers = request.Reviewers,
            WorkItemIds = request.WorkItemIds,
            ProjectId = request.ProjectId,
            OrganizationId = request.OrganizationId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

/// <summary>
/// Request model for creating a pull request
/// </summary>
public class CreatePullRequestRequest
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// The source branch name (e.g., refs/heads/feature-branch)
    /// </summary>
    public string SourceRefName { get; set; } = string.Empty;

    /// <summary>
    /// The target branch name (e.g., refs/heads/main)
    /// </summary>
    public string TargetRefName { get; set; } = string.Empty;

    /// <summary>
    /// The title of the pull request
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description of the pull request
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the pull request is a draft
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    /// List of reviewer email addresses or IDs
    /// </summary>
    public IEnumerable<string>? Reviewers { get; set; }

    /// <summary>
    /// List of work item IDs to link
    /// </summary>
    public IEnumerable<int>? WorkItemIds { get; set; }

    /// <summary>
    /// The project ID or name
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// The organization ID or URL
    /// </summary>
    public string? OrganizationId { get; set; }
}