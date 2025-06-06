using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.CreatePullRequest;

/// <summary>
/// Result from creating a pull request
/// </summary>
public class CreatePullRequestResult
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
    public bool IsDraft { get; set; }

    /// <summary>
    /// Creator of the pull request
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Created date
    /// </summary>
    public string CreationDate { get; set; } = string.Empty;

    /// <summary>
    /// Repository information
    /// </summary>
    public RepositoryInfo Repository { get; set; } = new RepositoryInfo();

    /// <summary>
    /// Reviewers for the pull request
    /// </summary>
    public List<ReviewerInfo> Reviewers { get; set; } = new List<ReviewerInfo>();

    /// <summary>
    /// URL to the pull request in the web UI
    /// </summary>
    public string WebUrl { get; set; } = string.Empty;
}

/// <summary>
/// Information about the repository
/// </summary>
public class RepositoryInfo
{
    /// <summary>
    /// Repository ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Repository name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Repository URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Project information
    /// </summary>
    public ProjectInfo Project { get; set; } = new ProjectInfo();
}

/// <summary>
/// Information about a project
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Project ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;
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