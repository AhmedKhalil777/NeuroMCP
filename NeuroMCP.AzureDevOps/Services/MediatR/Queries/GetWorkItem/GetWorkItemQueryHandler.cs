using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using NeuroMCP.AzureDevOps.Services.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetWorkItem;

/// <summary>
/// Handler for getting work item details
/// </summary>
public class GetWorkItemQueryHandler : AzureDevOpsRequestHandler<GetWorkItemQuery, WorkItem>
{
    public GetWorkItemQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetWorkItemQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get work item details
    /// </summary>
    public override async Task<WorkItem> Handle(GetWorkItemQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        var workItem = await witClient.GetWorkItemAsync(
            request.WorkItemId,
            expand: WorkItemExpand.All,
            cancellationToken: cancellationToken);

        return workItem;
    }
}