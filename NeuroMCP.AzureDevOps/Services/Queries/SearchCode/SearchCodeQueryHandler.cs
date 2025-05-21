using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi.Contracts.Code;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchCode;

/// <summary>
/// Handler for searching code across repositories
/// </summary>
public class SearchCodeQueryHandler : AzureDevOpsRequestHandler<SearchCodeQuery, SearchCodeResult>
{
    public SearchCodeQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<SearchCodeQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to search code
    /// </summary>
    public override async Task<SearchCodeResult> Handle(SearchCodeQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var searchClient = await connection.GetClientAsync<SearchHttpClient>();

        // Build code search request
        var searchRequest = new CodeSearchRequest
        {
            SearchText = request.SearchText,
            Skip = request.Skip,
            Top = request.Top,
            IncludeFacets = true
        };

        // Add project filter if specified
        if (!string.IsNullOrEmpty(request.ProjectId))
        {
            if (searchRequest.Filters == null)
            {
                searchRequest.Filters = new Dictionary<string, IEnumerable<string>>();
            }

            searchRequest.Filters["Project"] = new[] { request.ProjectId };
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

        try
        {
            // Execute the code search
            var searchResults = await searchClient.FetchCodeSearchResultsAsync(searchRequest, cancellationToken: cancellationToken);

            // Map the results to our model
            var result = new SearchCodeResult
            {
                Count = searchResults.Count
            };

            if (searchResults.Results != null)
            {
                result.Results = searchResults.Results.Select(r => new CodeSearchResult
                {
                    Repository = r.Repository?.Name ?? string.Empty,
                    Project = r.Project?.Name ?? string.Empty,
                    Path = r.Path ?? string.Empty,
                    FileName = System.IO.Path.GetFileName(r.Path ?? string.Empty),
                    LineNumber = 0, // No direct line number available in the new API
                    FileUrl = r.Path ?? string.Empty,
                    Version = r.Versions?.FirstOrDefault()?.BranchName ?? string.Empty
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
            Logger.LogError(ex, "Error searching code with query '{SearchText}'", request.SearchText);
            return new SearchCodeResult
            {
                Count = 0,
                Results = Array.Empty<CodeSearchResult>()
            };
        }
    }
}