using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchCode;

/// <summary>
/// Query to search for code across repositories
/// </summary>
public class SearchCodeQuery : AzureDevOpsRequest<SearchCodeResult>
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
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}