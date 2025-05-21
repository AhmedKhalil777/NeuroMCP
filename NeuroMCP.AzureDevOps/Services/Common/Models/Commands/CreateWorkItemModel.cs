namespace NeuroMCP.AzureDevOps.Services.Common.Models.Commands;

/// <summary>
/// Model for creating a work item
/// </summary>
public class CreateWorkItemModel
{
    /// <summary>
    /// The type of work item to create (e.g., "Bug", "Task", "User Story", "Feature")
    /// </summary>
    public string WorkItemType { get; set; } = string.Empty;

    /// <summary>
    /// The title of the work item
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description of the work item in HTML format (optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The area path for the work item (optional)
    /// </summary>
    public string? AreaPath { get; set; }

    /// <summary>
    /// The iteration path for the work item (optional)
    /// </summary>
    public string? IterationPath { get; set; }

    /// <summary>
    /// The email or name of the user to assign the work item to (optional)
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// The priority of the work item (optional)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Additional fields to set on the work item (optional)
    /// </summary>
    public IDictionary<string, object>? AdditionalFields { get; set; }

    /// <summary>
    /// The ID of the parent work item to create a relationship with (optional)
    /// </summary>
    public int? ParentId { get; set; }
}