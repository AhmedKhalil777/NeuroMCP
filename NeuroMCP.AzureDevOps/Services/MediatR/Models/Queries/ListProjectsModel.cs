namespace NeuroMCP.AzureDevOps.Services.MediatR.Models.Queries;

/// <summary>
/// Model for listing projects in an organization
/// </summary>
public class ListProjectsModel
{
    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}