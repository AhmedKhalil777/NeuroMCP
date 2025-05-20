namespace NeuroMCP.AzureDevOps.Services.MediatR.Models.Commands;

/// <summary>
/// Model for updating a pull request
/// </summary>
public class UpdatePullRequestModel
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
    /// The updated title of the pull request (optional)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The updated description of the pull request (optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The updated status of the pull request (optional, e.g., "active", "abandoned", "completed")
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Whether the pull request is a draft (optional)
    /// </summary>
    public bool? IsDraft { get; set; }

    /// <summary>
    /// List of reviewer email addresses or IDs to add (optional)
    /// </summary>
    public IEnumerable<string>? AddReviewers { get; set; }

    /// <summary>
    /// List of reviewer email addresses or IDs to remove (optional)
    /// </summary>
    public IEnumerable<string>? RemoveReviewers { get; set; }

    /// <summary>
    /// List of work item IDs to link (optional)
    /// </summary>
    public IEnumerable<int>? AddWorkItemIds { get; set; }

    /// <summary>
    /// List of work item IDs to unlink (optional)
    /// </summary>
    public IEnumerable<int>? RemoveWorkItemIds { get; set; }
}