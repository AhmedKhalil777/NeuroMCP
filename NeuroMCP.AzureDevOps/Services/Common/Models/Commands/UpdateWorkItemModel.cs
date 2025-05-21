using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.Common.Models.Commands;

/// <summary>
/// Model for updating an existing work item
/// </summary>
public class UpdateWorkItemModel
{
    /// <summary>
    /// The ID of the work item to update
    /// </summary>
    public int WorkItemId { get; set; }

    /// <summary>
    /// The fields to update
    /// </summary>
    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}