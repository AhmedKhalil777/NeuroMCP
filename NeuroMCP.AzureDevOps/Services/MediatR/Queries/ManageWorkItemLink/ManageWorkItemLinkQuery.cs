using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ManageWorkItemLink;

/// <summary>
/// Query to manage links between work items
/// </summary>
public class ManageWorkItemLinkQuery : AzureDevOpsRequest<WorkItem>
{
    /// <summary>
    /// The ID of the source work item
    /// </summary>
    public int SourceWorkItemId { get; set; }

    /// <summary>
    /// The ID of the target work item
    /// </summary>
    public int TargetWorkItemId { get; set; }

    /// <summary>
    /// The reference name of the relation type (e.g., "System.LinkTypes.Hierarchy-Forward")
    /// </summary>
    public string RelationType { get; set; } = string.Empty;

    /// <summary>
    /// The operation to perform on the link (add, remove, update)
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Optional comment explaining the link
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// The new relation type to use when updating a link
    /// </summary>
    public string? NewRelationType { get; set; }
}