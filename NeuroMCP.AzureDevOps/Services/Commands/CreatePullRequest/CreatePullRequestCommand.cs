using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace NeuroMCP.AzureDevOps.Services.Commands.CreatePullRequest;

/// <summary>
/// Command to create a new pull request
/// </summary>
public class CreatePullRequestCommand : AzureDevOpsRequest<GitPullRequest>
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
}