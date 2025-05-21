namespace NeuroMCP.AzureDevOps.Services.Common.Models.Queries;

/// <summary>
/// Model for getting content of a file or folder from a repository
/// </summary>
public class GetFileContentModel
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// Path to the file or folder
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}