using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Search.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi.Contracts.Wiki;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWiki;

/// <summary>
/// Handler for searching content across wiki pages
/// </summary>
public class SearchWikiQueryHandler : AzureDevOpsRequestHandler<SearchWikiQuery, SearchWikiResult>
{
    public SearchWikiQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<SearchWikiQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to search wiki content
    /// </summary>
    public override async Task<SearchWikiResult> Handle(SearchWikiQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await GetConnectionAsync(request.OrganizationId);
            var searchClient = await connection.GetClientAsync<WikiSearchHttpClient>();

            // Build wiki search request
            var searchRequest = new WikiSearchRequest
            {
                SearchText = request.SearchText,
                SkipResults = request.Skip,
                TakeResults = request.Top,
                IncludeFacets = request.IncludeFacets
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

            // Execute the wiki search
            var searchResults = await searchClient.FetchWikiSearchResultsAsync(searchRequest, cancellationToken: cancellationToken);

            // Map the results to our model
            var result = new SearchWikiResult
            {
                Count = searchResults.Count
            };

            if (searchResults.Results != null)
            {
                result.Results = searchResults.Results.Select(r => new WikiSearchResult
                {
                    WikiName = r.Wiki?.Name ?? string.Empty,
                    Project = r.Project?.Name ?? string.Empty,
                    Path = r.Path?.Path ?? string.Empty,
                    Title = r.Title ?? string.Empty,
                    Snippet = r.Matches?.FirstOrDefault()?.MatchText ?? string.Empty,
                    Url = r.Path?.Url ?? string.Empty
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
            Logger.LogError(ex, "Error searching wiki with query '{SearchText}'", request.SearchText);
            return new SearchWikiResult
            {
                Count = 0,
                Results = Array.Empty<WikiSearchResult>()
            };
        }
    }
}