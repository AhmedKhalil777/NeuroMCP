using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using NeuroMCP.AzureDevOps.Services.MediatR;
using NeuroMCP.AzureDevOps.Services.Queries.SearchWorkItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.Queries.SearchWorkItems;

/// <summary>
/// Handler for searching work items
/// </summary>
public class SearchWorkItemsQueryHandler : AzureDevOpsRequestHandler<SearchWorkItemsQuery, SearchWorkItemsResult>
{
    public SearchWorkItemsQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<SearchWorkItemsQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to search work items
    /// </summary>
    public override async Task<SearchWorkItemsResult> Handle(SearchWorkItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await GetConnectionAsync(request.OrganizationId);
            var witClient = await connection.GetClientAsync<Microsoft.TeamFoundation.WorkItemTracking.WebApi.WorkItemTrackingHttpClient>();

            // We need to adapt our approach since the WorkItemSearchHttpClient doesn't exist or is different in this version
            // Use the regular WIQL query instead
            var wiql = new Wiql
            {
                // Simple WIQL query to search by Title or Description
                Query = $"SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State], [System.TeamProject], [System.AreaPath], [System.IterationPath], [System.AssignedTo] " +
                        $"FROM WorkItems " +
                        $"WHERE [System.Title] CONTAINS '{request.Model.SearchText}' OR [System.Description] CONTAINS '{request.Model.SearchText}' " +
                        (string.IsNullOrEmpty(request.ProjectId) ? "" : $"AND [System.TeamProject] = '{request.ProjectId}' ")
            };

            var queryResult = await witClient.QueryByWiqlAsync(wiql, cancellationToken: cancellationToken);

            // Get detailed work item data for the results
            var workItemIds = queryResult.WorkItems?.Select(wi => wi.Id).ToArray() ?? Array.Empty<int>();

            if (workItemIds.Length == 0)
            {
                return new SearchWorkItemsResult
                {
                    Count = 0,
                    Results = new List<WorkItemSearchResult>()
                };
            }

            // Apply top/skip if specified
            if (request.Model.Skip.HasValue || request.Model.Top.HasValue)
            {
                int skip = request.Model.Skip ?? 0;
                int take = request.Model.Top ?? workItemIds.Length;

                workItemIds = workItemIds
                    .Skip(skip)
                    .Take(Math.Min(take, workItemIds.Length - skip))
                    .ToArray();
            }

            // Get work item details
            var workItems = await witClient.GetWorkItemsAsync(
                workItemIds,
                expand: WorkItemExpand.All,
                cancellationToken: cancellationToken);

            // Convert to our result model
            var result = new SearchWorkItemsResult
            {
                Count = workItems.Count
            };

            result.Results = workItems.Select(wi => new WorkItemSearchResult
            {
                Id = wi.Id.Value,
                Title = wi.Fields.TryGetValue("System.Title", out var title) ? title?.ToString() : string.Empty,
                WorkItemType = wi.Fields.TryGetValue("System.WorkItemType", out var type) ? type?.ToString() : string.Empty,
                State = wi.Fields.TryGetValue("System.State", out var state) ? state?.ToString() : string.Empty,
                Url = wi.Url?.ToString(),
                Fields = wi.Fields.ToDictionary(f => f.Key, f => f.Value)
            }).ToList();

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching work items with query '{SearchText}'", request.Model.SearchText);
            return new SearchWorkItemsResult
            {
                Count = 0,
                Results = new List<WorkItemSearchResult>()
            };
        }
    }
}