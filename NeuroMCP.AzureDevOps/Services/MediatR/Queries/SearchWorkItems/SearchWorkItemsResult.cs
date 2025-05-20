using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWorkItems;

/// <summary>
/// Result model for work item search
/// </summary>
public class SearchWorkItemsResult
{
    /// <summary>
    /// Total count of matching work item results
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Work item search results
    /// </summary>
    public IEnumerable<WorkItemSearchResult> Results { get; set; } = new List<WorkItemSearchResult>();

    /// <summary>
    /// Facets for filtering
    /// </summary>
    public Dictionary<string, object> Facets { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Individual work item search result
/// </summary>
public class WorkItemSearchResult
{
    /// <summary>
    /// Work item ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Work item type (Bug, Task, User Story, etc.)
    /// </summary>
    public string WorkItemType { get; set; } = string.Empty;

    /// <summary>
    /// Work item title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Work item state (New, Active, Closed, etc.)
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Project where the work item is defined
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Area path of the work item
    /// </summary>
    public string AreaPath { get; set; } = string.Empty;

    /// <summary>
    /// Iteration path of the work item
    /// </summary>
    public string IterationPath { get; set; } = string.Empty;

    /// <summary>
    /// User assigned to the work item
    /// </summary>
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// URL to view the work item in the Azure DevOps web interface
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Snippet showing a match in the work item content
    /// </summary>
    public string Snippet { get; set; } = string.Empty;
}