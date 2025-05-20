using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetAllRepositoriesTree;

/// <summary>
/// Result for getting a hierarchical tree of files and directories from multiple repositories
/// </summary>
public class GetAllRepositoriesTreeResult
{
    /// <summary>
    /// Collection of repositories and their file tree
    /// </summary>
    public List<RepositoryTreeInfo> Repositories { get; set; } = new List<RepositoryTreeInfo>();
}

/// <summary>
/// Information about a repository and its file tree
/// </summary>
public class RepositoryTreeInfo
{
    /// <summary>
    /// Repository ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Repository name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Repository default branch
    /// </summary>
    public string DefaultBranch { get; set; } = string.Empty;

    /// <summary>
    /// Root of the file tree
    /// </summary>
    public DirectoryTreeItem Root { get; set; } = new DirectoryTreeItem { Name = "/" };
}

/// <summary>
/// Base class for tree items
/// </summary>
public abstract class TreeItem
{
    /// <summary>
    /// Name of the item
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full path of the item
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// URL to view the item in Azure DevOps
    /// </summary>
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents a directory in the file tree
/// </summary>
public class DirectoryTreeItem : TreeItem
{
    /// <summary>
    /// Child items in this directory
    /// </summary>
    public List<TreeItem> Children { get; set; } = new List<TreeItem>();
}

/// <summary>
/// Represents a file in the file tree
/// </summary>
public class FileTreeItem : TreeItem
{
    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Content type of the file
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}