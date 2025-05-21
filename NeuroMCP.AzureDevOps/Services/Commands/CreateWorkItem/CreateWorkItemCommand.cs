using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common.Models.Commands;
using System.Runtime.Serialization;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Commands.CreateWorkItem;

/// <summary>
/// Command for creating a work item
/// </summary>
public class CreateWorkItemCommand : AzureDevOpsRequest<CreateWorkItemCommandResult>
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

/// <summary>
/// Result of creating a work item
/// </summary>
public class CreateWorkItemCommandResult
{
    //
    // Summary:
    //     The work item ID.
    public int? Id { get; set; }

    //
    // Summary:
    //     Revision number of the work item.
    public int? Rev { get; set; }

    //
    // Summary:
    //     Map of field and values for the work item.
    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>(); 

    //
    // Summary:
    //     Relations of the work item.
    public IList<WorkItemRelation> Relations { get; set; }

    //
    // Summary:
    //     Reference to a specific version of the comment added/edited/deleted in this revision.
    public WorkItemCommentVersionRef CommentVersionRef { get; set; }

}