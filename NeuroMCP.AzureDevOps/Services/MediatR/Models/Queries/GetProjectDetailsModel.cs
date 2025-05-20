namespace NeuroMCP.AzureDevOps.Services.MediatR.Models.Queries;

/// <summary>
/// Model for getting comprehensive project details
/// </summary>
public class GetProjectDetailsModel
{
    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include team information
    /// </summary>
    public bool IncludeTeams { get; set; } = false;

    /// <summary>
    /// Whether to include process information
    /// </summary>
    public bool IncludeProcess { get; set; } = false;

    /// <summary>
    /// Whether to include work item type information
    /// </summary>
    public bool IncludeWorkItemTypes { get; set; } = false;

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}