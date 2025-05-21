namespace NeuroMCP.AzureDevOps.Services.Common.Models.Queries;

/// <summary>
/// Model for getting work item details
/// </summary>
public class GetWorkItemModel
{
    /// <summary>
    /// The ID of the work item
    /// </summary>
    public int WorkItemId { get; set; }

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}