namespace NeuroMCP.AzureDevOps.Services.Common.Models.Commands;

/// <summary>
/// Model for managing links between work items
/// </summary>
public class ManageWorkItemLinkModel
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

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}