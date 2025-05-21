namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListPullRequests;

/// <summary>
/// Query to list pull requests in a repository
/// </summary>
public class ListPullRequestsQuery : AzureDevOpsRequest<ListPullRequestsResult>
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// Filter by pull request status (active, completed, abandoned, all)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by creator ID
    /// </summary>
    public string? CreatorId { get; set; }

    /// <summary>
    /// Filter by reviewer ID
    /// </summary>
    public string? ReviewerId { get; set; }

    /// <summary>
    /// Filter by source branch name
    /// </summary>
    public string? SourceRefName { get; set; }

    /// <summary>
    /// Filter by target branch name
    /// </summary>
    public string? TargetRefName { get; set; }

    /// <summary>
    /// Number of pull requests to skip for pagination
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Maximum number of pull requests to return
    /// </summary>
    public int Top { get; set; } = 10;
}