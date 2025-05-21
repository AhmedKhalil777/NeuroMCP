namespace NeuroMCP.AzureDevOps.Services.Common.Models.Queries;

/// <summary>
/// Model for listing repositories in a project
/// </summary>
public class ListRepositoriesModel
{
    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}