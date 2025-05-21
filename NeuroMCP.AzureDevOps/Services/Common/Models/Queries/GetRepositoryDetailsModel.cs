namespace NeuroMCP.AzureDevOps.Services.Common.Models.Queries;

/// <summary>
/// Model for getting detailed repository information including refs and statistics
/// </summary>
public class GetRepositoryDetailsModel
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include repository refs
    /// </summary>
    public bool IncludeRefs { get; set; } = false;

    /// <summary>
    /// Whether to include branch statistics
    /// </summary>
    public bool IncludeStatistics { get; set; } = false;

    /// <summary>
    /// Name of specific branch to get statistics for (if includeStatistics is true)
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}