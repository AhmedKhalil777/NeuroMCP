using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchCode;

/// <summary>
/// Result model for code search
/// </summary>
public class SearchCodeResult
{
    /// <summary>
    /// Total count of matching code results
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Code search results
    /// </summary>
    public IEnumerable<CodeSearchResult> Results { get; set; } = new List<CodeSearchResult>();

    /// <summary>
    /// Facets for filtering
    /// </summary>
    public Dictionary<string, object> Facets { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Individual code search result
/// </summary>
public class CodeSearchResult
{
    /// <summary>
    /// Repository where the match was found
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Project where the match was found
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Path to the file containing the match
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// File name containing the match
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Code snippet containing the match
    /// </summary>
    public string Snippet { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the match was found
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// URL to view the file in the Azure DevOps web interface
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Commit or branch where the match was found
    /// </summary>
    public string Version { get; set; } = string.Empty;
}