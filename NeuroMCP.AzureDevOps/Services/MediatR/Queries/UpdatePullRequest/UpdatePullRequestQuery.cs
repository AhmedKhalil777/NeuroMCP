using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.UpdatePullRequest;

/// <summary>
/// Query to update an existing pull request
/// </summary>
public class UpdatePullRequestQuery : AzureDevOpsRequest<UpdatePullRequestResult>
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the pull request to update
    /// </summary>
    public int PullRequestId { get; set; }

    /// <summary>
    /// The updated title of the pull request
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The updated description of the pull request
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The updated status of the pull request (active, abandoned, completed)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Whether the pull request should be marked as a draft (true) or unmarked (false)
    /// </summary>
    public bool? IsDraft { get; set; }

    /// <summary>
    /// List of reviewer email addresses or IDs to add
    /// </summary>
    public IEnumerable<string>? AddReviewers { get; set; }

    /// <summary>
    /// List of reviewer email addresses or IDs to remove
    /// </summary>
    public IEnumerable<string>? RemoveReviewers { get; set; }

    /// <summary>
    /// List of work item IDs to link to the pull request
    /// </summary>
    public IEnumerable<int>? AddWorkItemIds { get; set; }

    /// <summary>
    /// List of work item IDs to unlink from the pull request
    /// </summary>
    public IEnumerable<int>? RemoveWorkItemIds { get; set; }

    /// <summary>
    /// Additional properties to update on the pull request
    /// </summary>
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}