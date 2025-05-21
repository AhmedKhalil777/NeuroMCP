using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace NeuroMCP.AzureDevOps.Services.Commands.ManageWorkItemLink;

/// <summary>
/// Command to add, remove, or update links between work items
/// </summary>
public class ManageWorkItemLinkCommand : AzureDevOpsRequest<WorkItem>
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
    /// The relation type
    /// </summary>
    public string RelationType { get; set; } = string.Empty;

    /// <summary>
    /// The operation to perform (add, remove, update)
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Optional comment for the link
    /// </summary>
    public string? Comment { get; set; }
}