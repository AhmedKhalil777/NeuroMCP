namespace NeuroMCP.AzureDevOps.Services.Common.Models.Commands;

/// <summary>
/// Model for adding a comment to a pull request
/// </summary>
public class AddPullRequestCommentModel
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the pull request
    /// </summary>
    public int PullRequestId { get; set; }

    /// <summary>
    /// The content of the comment in markdown format
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The ID of an existing thread to add the comment to (optional)
    /// </summary>
    public int? ThreadId { get; set; }

    /// <summary>
    /// The ID of a parent comment when replying to an existing comment (optional)
    /// </summary>
    public int? ParentCommentId { get; set; }

    /// <summary>
    /// The path of the file to comment on when creating a new thread (optional)
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// The line number to comment on when creating a new thread (optional)
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// The status to set for a new thread (e.g., "active", "fixed", "wontFix", "closed", "pending") (optional)
    /// </summary>
    public string? Status { get; set; }
}