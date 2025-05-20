using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetRepositoryDetails;

/// <summary>
/// Result model for detailed repository information
/// </summary>
public class GetRepositoryDetailsResult
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
    /// Project name or ID containing the repository
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Default branch of the repository (e.g., "refs/heads/main")
    /// </summary>
    public string DefaultBranch { get; set; } = string.Empty;

    /// <summary>
    /// Size of the repository in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Remote URL for the repository
    /// </summary>
    public string RemoteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Web URL for the repository
    /// </summary>
    public string WebUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether the repository is a fork
    /// </summary>
    public bool IsFork { get; set; }

    /// <summary>
    /// References (branches and tags) in the repository
    /// </summary>
    public IEnumerable<GitReferenceInfo>? Refs { get; set; }

    /// <summary>
    /// Statistics for branches in the repository
    /// </summary>
    public BranchStatistics? Statistics { get; set; }
}

/// <summary>
/// Reference information for Git branches and tags
/// </summary>
public class GitReferenceInfo
{
    /// <summary>
    /// Name of the reference (e.g., "refs/heads/main")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Object ID (SHA-1) that the reference points to
    /// </summary>
    public string ObjectId { get; set; } = string.Empty;

    /// <summary>
    /// URL for the reference
    /// </summary>
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Statistics for a branch
/// </summary>
public class BranchStatistics
{
    /// <summary>
    /// Branch name
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Number of commits
    /// </summary>
    public int CommitCount { get; set; }

    /// <summary>
    /// Number of files
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Size of the branch in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Ahead/behind statistics compared to the default branch
    /// </summary>
    public AheadBehindStatistics? AheadBehind { get; set; }
}

/// <summary>
/// Ahead/behind statistics for a branch
/// </summary>
public class AheadBehindStatistics
{
    /// <summary>
    /// Number of commits ahead of the comparison branch
    /// </summary>
    public int Ahead { get; set; }

    /// <summary>
    /// Number of commits behind the comparison branch
    /// </summary>
    public int Behind { get; set; }
}