using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Search.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi.Contracts.WorkItem;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWorkItems;

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
        var connection = await GetConnectionAsync(request.OrganizationId);
        var searchClient = await connection.GetClientAsync<WorkItemSearchHttpClient>();

        // Build work item search request
        var searchRequest = new WorkItemSearchRequest
        {
            SearchText = request.SearchText,
            SkipResults = request.Skip,
            TakeResults = request.Top,
            IncludeFacets = request.IncludeFacets
        };

        // Add project filter if specified in the request
        if (!string.IsNullOrEmpty(request.ProjectId))
        {
            if (searchRequest.Filters == null)
            {
                searchRequest.Filters = new Dictionary<string, IEnumerable<string>>();
            }

            searchRequest.Filters["System.TeamProject"] = new[] { request.ProjectId };
        }

        // Add additional filters if specified
        if (request.Filters != null && request.Filters.Count > 0)
        {
            if (searchRequest.Filters == null)
            {
                searchRequest.Filters = new Dictionary<string, IEnumerable<string>>();
            }

            foreach (var filter in request.Filters)
            {
                if (filter.Value.Count > 0 && !searchRequest.Filters.ContainsKey(filter.Key))
                {
                    searchRequest.Filters[filter.Key] = filter.Value;
                }
            }
        }

        // Add sort options if specified
        if (request.OrderBy != null && request.OrderBy.Count > 0)
        {
            searchRequest.OrderBy = request.OrderBy.Select(o => new SortOption
            {
                Field = o.Field,
                SortOrder = o.SortOrder
            });
        }

        try
        {
            // Execute the work item search
            var searchResults = await searchClient.FetchWorkItemSearchResultsAsync(searchRequest, cancellationToken: cancellationToken);

            // Map the results to our model
            var result = new SearchWorkItemsResult
            {
                Count = searchResults.Count
            };

            if (searchResults.Results != null)
            {
                result.Results = searchResults.Results.Select(r => new WorkItemSearchResult
                {
                    Id = r.Fields.TryGetValue("System.Id", out var id) && id != null ? Convert.ToInt32(id) : 0,
                    WorkItemType = r.Fields.TryGetValue("System.WorkItemType", out var type) && type != null ? type.ToString() : string.Empty,
                    Title = r.Fields.TryGetValue("System.Title", out var title) && title != null ? title.ToString() : string.Empty,
                    State = r.Fields.TryGetValue("System.State", out var state) && state != null ? state.ToString() : string.Empty,
                    Project = r.Fields.TryGetValue("System.TeamProject", out var project) && project != null ? project.ToString() : string.Empty,
                    AreaPath = r.Fields.TryGetValue("System.AreaPath", out var areaPath) && areaPath != null ? areaPath.ToString() : string.Empty,
                    IterationPath = r.Fields.TryGetValue("System.IterationPath", out var iterationPath) && iterationPath != null ? iterationPath.ToString() : string.Empty,
                    AssignedTo = r.Fields.TryGetValue("System.AssignedTo", out var assignedTo) && assignedTo != null ? assignedTo.ToString() : string.Empty,
                    Url = r.Url ?? string.Empty,
                    Snippet = r.Matches?.FirstOrDefault()?.MatchText ?? string.Empty
                }).ToList();
            }

            // Map the facets if available
            if (searchResults.Facets != null)
            {
                foreach (var facet in searchResults.Facets)
                {
                    result.Facets[facet.Key] = facet.Value;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching work items with query '{SearchText}'", request.SearchText);
            return new SearchWorkItemsResult
            {
                Count = 0,
                Results = Array.Empty<WorkItemSearchResult>()
            };
        }
    }
}