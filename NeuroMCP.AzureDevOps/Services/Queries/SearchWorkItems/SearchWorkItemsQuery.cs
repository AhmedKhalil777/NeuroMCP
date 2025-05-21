using NeuroMCP.AzureDevOps.Services.Common.Models.Queries;
using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.Queries.SearchWorkItems;

/// <summary>
/// Query for searching work items
/// </summary>
public class SearchWorkItemsQuery : AzureDevOpsRequest<SearchWorkItemsResult>
{
    /// <summary>
    /// The search parameters
    /// </summary>
    public required SearchWorkItemsModel Model { get; set; }
}

/// <summary>
/// Result of searching work items
/// </summary>
public class SearchWorkItemsResult
{
    /// <summary>
    /// The count of work items found
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The work items found
    /// </summary>
    public List<WorkItemSearchResult> Results { get; set; } = new();

    /// <summary>
    /// Facets for the search results
    /// </summary>
    public Dictionary<string, List<FacetValue>>? Facets { get; set; }
}

/// <summary>
/// A work item search result
/// </summary>
public class WorkItemSearchResult
{
    /// <summary>
    /// The ID of the work item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The URL of the work item
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The title of the work item
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The type of work item
    /// </summary>
    public string? WorkItemType { get; set; }

    /// <summary>
    /// The state of the work item
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// The fields of the work item
    /// </summary>
    public Dictionary<string, object>? Fields { get; set; }
}

/// <summary>
/// A facet value for search results
/// </summary>
public class FacetValue
{
    /// <summary>
    /// The name of the facet value
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The count of items with this facet value
    /// </summary>
    public int Count { get; set; }
} 