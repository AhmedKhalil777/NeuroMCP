using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.UpdatePullRequest;

/// <summary>
/// Result from updating a pull request
/// </summary>
public class UpdatePullRequestResult
{
    /// <summary>
    /// The pull request ID
    /// </summary>
    public int PullRequestId { get; set; }

    /// <summary>
    /// The pull request title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The pull request description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The source branch name
    /// </summary>
    public string SourceRefName { get; set; } = string.Empty;

    /// <summary>
    /// The target branch name
    /// </summary>
    public string TargetRefName { get; set; } = string.Empty;

    /// <summary>
    /// The status of the pull request
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether the pull request is a draft
    /// </summary>
    public bool? IsDraft { get; set; }

    /// <summary>
    /// URL to the pull request in the web UI
    /// </summary>
    public string WebUrl { get; set; } = string.Empty;

    /// <summary>
    /// List of reviewers
    /// </summary>
    public List<ReviewerInfo> Reviewers { get; set; } = new List<ReviewerInfo>();

    /// <summary>
    /// List of linked work item IDs
    /// </summary>
    public List<int> WorkItemIds { get; set; } = new List<int>();
}

/// <summary>
/// Information about a pull request reviewer
/// </summary>
public class ReviewerInfo
{
    /// <summary>
    /// Reviewer ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reviewer display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Vote (-10: Rejected, -5: Waiting for author, 0: No vote, 5: Approved with suggestions, 10: Approved)
    /// </summary>
    public int Vote { get; set; }

    /// <summary>
    /// Is the reviewer required
    /// </summary>
    public bool IsRequired { get; set; }
}