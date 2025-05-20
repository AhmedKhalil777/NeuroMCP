using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Commands.UpdateWorkItem;

/// <summary>
/// Command to update an existing work item
/// </summary>
public class UpdateWorkItemCommand : AzureDevOpsRequest<WorkItem>
{
    /// <summary>
    /// The ID of the work item to update
    /// </summary>
    public int WorkItemId { get; set; }

    /// <summary>
    /// The fields to update
    /// </summary>
    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();
}