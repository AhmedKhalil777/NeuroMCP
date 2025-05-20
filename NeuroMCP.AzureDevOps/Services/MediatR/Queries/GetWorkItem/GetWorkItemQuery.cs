using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetWorkItem;

/// <summary>
/// Query to get work item details
/// </summary>
public class GetWorkItemQuery : AzureDevOpsRequest<WorkItem>
{
    /// <summary>
    /// The ID of the work item
    /// </summary>
    public int WorkItemId { get; set; }
}