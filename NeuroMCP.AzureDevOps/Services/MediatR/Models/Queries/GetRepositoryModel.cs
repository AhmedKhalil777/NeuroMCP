namespace NeuroMCP.AzureDevOps.Services.MediatR.Models.Queries;

/// <summary>
/// Model for getting repository details
/// </summary>
public class GetRepositoryModel
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
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}