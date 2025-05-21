using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWiki;

/// <summary>
/// Result model for wiki search
/// </summary>
public class SearchWikiResult
{
    /// <summary>
    /// Total count of matching wiki results
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Wiki search results
    /// </summary>
    public IEnumerable<WikiSearchResult> Results { get; set; } = new List<WikiSearchResult>();

    /// <summary>
    /// Facets for filtering
    /// </summary>
    public Dictionary<string, object> Facets { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Individual wiki search result
/// </summary>
public class WikiSearchResult
{
    /// <summary>
    /// Wiki name where the match was found
    /// </summary>
    public string WikiName { get; set; } = string.Empty;

    /// <summary>
    /// Project where the match was found
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Path to the wiki page containing the match
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Title of the wiki page containing the match
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content snippet containing the match
    /// </summary>
    public string Snippet { get; set; } = string.Empty;

    /// <summary>
    /// URL to view the wiki page in the Azure DevOps web interface
    /// </summary>
    public string Url { get; set; } = string.Empty;


    public WikiMetaDataResult WikiMetaDataResult { get; set; }
}


public class WikiMetaDataResult
{
    //
    // Summary:
    //     Name of the wiki.
    public string Name { get; set; }

    //
    // Summary:
    //     Id of the wiki.
    public string Id { get; set; }

    //
    // Summary:
    //     Mapped path for the wiki.
    public string MappedPath { get; set; }

    //
    // Summary:
    //     Version for wiki.

    public string Version { get; set; }
}