using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWiki;

/// <summary>
/// Query to search for content across wiki pages
/// </summary>
public class SearchWikiQuery : AzureDevOpsRequest<SearchWikiResult>
{
    /// <summary>
    /// The text to search for
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// The ID or name of the project (optional)
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Optional filters to narrow search results
    /// </summary>
    public Dictionary<string, List<string>>? Filters { get; set; }

    /// <summary>
    /// Number of results to skip for pagination
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int Top { get; set; } = 100;

    /// <summary>
    /// Whether to include faceting in results
    /// </summary>
    public bool IncludeFacets { get; set; } = true;
}