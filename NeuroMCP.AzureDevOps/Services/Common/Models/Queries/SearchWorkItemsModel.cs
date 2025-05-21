using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.Common.Models.Queries;

/// <summary>
/// Model for searching work items
/// </summary>
public class SearchWorkItemsModel
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
    public int? Skip { get; set; } = 0;

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Top { get; set; } = 100;

    /// <summary>
    /// Whether to include faceting in results
    /// </summary>
    public bool IncludeFacets { get; set; } = true;

    /// <summary>
    /// Options for sorting search results
    /// </summary>
    public List<SortOption>? OrderBy { get; set; }

    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}

/// <summary>
/// Sort option for search results
/// </summary>
public class SortOption
{
    /// <summary>
    /// Field to sort by
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Sort direction (ASC or DESC)
    /// </summary>
    public string SortOrder { get; set; } = "ASC";
}