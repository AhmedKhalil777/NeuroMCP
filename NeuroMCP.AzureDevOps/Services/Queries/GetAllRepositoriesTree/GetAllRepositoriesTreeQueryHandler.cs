using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetAllRepositoriesTree;

/// <summary>
/// Handler for getting a hierarchical tree of files and directories from multiple repositories
/// </summary>
public class GetAllRepositoriesTreeQueryHandler : AzureDevOpsRequestHandler<GetAllRepositoriesTreeQuery, GetAllRepositoriesTreeResult>
{
    public GetAllRepositoriesTreeQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetAllRepositoriesTreeQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get a hierarchical tree of files and directories
    /// </summary>
    public override async Task<GetAllRepositoriesTreeResult> Handle(GetAllRepositoriesTreeQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        // Determine project ID
        var projectId = request.ProjectId ?? ConnectionProvider.GetDefaultProject();
        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Project ID is required");
        }

        try
        {
            // Get all repositories in the project
            var repositories = await gitClient.GetRepositoriesAsync(projectId, cancellationToken: cancellationToken);

            // Filter repositories if pattern specified
            if (!string.IsNullOrEmpty(request.RepositoryPattern))
            {
                var repoPattern = WildcardToRegex(request.RepositoryPattern);
                repositories = repositories.Where(r => Regex.IsMatch(r.Name, repoPattern, RegexOptions.IgnoreCase)).ToList();
            }

            // Build the result
            var result = new GetAllRepositoriesTreeResult();

            // Process each repository
            foreach (var repo in repositories)
            {
                // Skip empty repos or ones without a default branch
                if (string.IsNullOrEmpty(repo.DefaultBranch))
                {
                    Logger.LogWarning("Repository {RepositoryName} has no default branch", repo.Name);
                    continue;
                }

                // Create repository info
                var repoInfo = new RepositoryTreeInfo
                {
                    Id = repo.Id.ToString(),
                    Name = repo.Name,
                    DefaultBranch = repo.DefaultBranch
                };

                // Get the root items
                try
                {
                    var rootItems = await gitClient.GetItemsAsync(
                        projectId,
                        repo.Id,
                        recursionLevel: request.Depth == 0 ? VersionControlRecursionType.Full : VersionControlRecursionType.OneLevel,
                        versionDescriptor: new GitVersionDescriptor { Version = repo.DefaultBranch, VersionType = GitVersionType.Branch },
                        cancellationToken: cancellationToken);

                    // Build the tree
                    if (request.Depth == 0)
                    {
                        // If depth is unlimited, we can build the tree in one go
                        BuildTreeFromFlatList(repoInfo.Root, rootItems, request.Pattern);
                    }
                    else
                    {
                        // Otherwise, we need to traverse the tree manually with depth limit
                        await BuildTreeWithDepthLimit(
                            gitClient,
                            repoInfo.Root,
                            repo.Id,
                            projectId,
                            repo.DefaultBranch,
                            1, // Start at depth 1 since we're at root
                            request.Depth,
                            request.Pattern,
                            cancellationToken);
                    }

                    result.Repositories.Add(repoInfo);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error retrieving items from repository {RepositoryName}", repo.Name);
                    // Continue with the next repository
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting repository tree for project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Builds a tree from a flat list of items (when recursion is full)
    /// </summary>
    private void BuildTreeFromFlatList(DirectoryTreeItem root, List<GitItem> items, string? pattern = null)
    {
        // Create a lookup of path -> directory item
        var directoryLookup = new Dictionary<string, DirectoryTreeItem>
        {
            ["/"] = root // Root directory
        };

        // Sort items by path for consistency
        var sortedItems = items.OrderBy(i => i.Path).ToList();

        foreach (var item in sortedItems)
        {
            // Skip if doesn't match pattern
            if (!string.IsNullOrEmpty(pattern) && !item.IsFolder && !IsFileMatchingPattern(item.Path, pattern))
            {
                continue;
            }

            // Skip the root item
            if (string.IsNullOrEmpty(item.Path) || item.Path == "/")
            {
                continue;
            }

            // Get parent directory path
            var parentPath = GetParentPath(item.Path);
            var itemName = GetItemName(item.Path);

            // If parent doesn't exist, create the directory structure
            if (!directoryLookup.ContainsKey(parentPath))
            {
                CreateDirectoryStructure(directoryLookup, parentPath);
            }

            // Get parent directory
            var parentDir = directoryLookup[parentPath];

            // Create directory or file
            if (item.IsFolder)
            {
                var dir = new DirectoryTreeItem
                {
                    Name = itemName,
                    Path = item.Path,
                    Url = item.Url
                };

                directoryLookup[item.Path] = dir;
                parentDir.Children.Add(dir);
            }
            else
            {
                var file = new FileTreeItem
                {
                    Name = itemName,
                    Path = item.Path,
                    Url = item.Url,
                    ContentType = item.ContentMetadata?.ContentType ?? string.Empty
                };

                parentDir.Children.Add(file);
            }
        }
    }

    /// <summary>
    /// Builds a tree with a depth limit by making multiple API calls
    /// </summary>
    private async Task BuildTreeWithDepthLimit(
        GitHttpClient gitClient,
        DirectoryTreeItem directory,
        Guid repositoryId,
        string projectId,
        string branchName,
        int currentDepth,
        int maxDepth,
        string? pattern,
        CancellationToken cancellationToken)
    {
        // If we've reached the max depth, stop recursing
        if (maxDepth > 0 && currentDepth > maxDepth)
        {
            return;
        }

        try
        {
            // Get items for this directory
            var items = await gitClient.GetItemsAsync(
                projectId,
                repositoryId,
                scopePath: directory.Path == "/" ? null : directory.Path,
                recursionLevel: VersionControlRecursionType.OneLevel,
                versionDescriptor: new GitVersionDescriptor { Version = branchName, VersionType = GitVersionType.Branch },
                cancellationToken: cancellationToken);

            // Skip the directory itself
            items = items.Where(i => i.Path != directory.Path && i.Path != "/").ToList();

            // Add each item to the directory
            foreach (var item in items)
            {
                var itemName = GetItemName(item.Path);

                if (item.IsFolder)
                {
                    var dir = new DirectoryTreeItem
                    {
                        Name = itemName,
                        Path = item.Path,
                        Url = item.Url
                    };

                    directory.Children.Add(dir);

                    // Recursively process subdirectories
                    await BuildTreeWithDepthLimit(
                        gitClient,
                        dir,
                        repositoryId,
                        projectId,
                        branchName,
                        currentDepth + 1,
                        maxDepth,
                        pattern,
                        cancellationToken);
                }
                else
                {
                    // If pattern is specified, check if file matches
                    if (!string.IsNullOrEmpty(pattern) && !IsFileMatchingPattern(item.Path, pattern))
                    {
                        continue;
                    }

                    var file = new FileTreeItem
                    {
                        Name = itemName,
                        Path = item.Path,
                        Url = item.Url,
                        ContentType = item.ContentMetadata?.ContentType ?? string.Empty
                    };

                    directory.Children.Add(file);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving items from path {Path}", directory.Path);
            // Continue with the next directory
        }
    }

    /// <summary>
    /// Creates a directory structure for a path
    /// </summary>
    private void CreateDirectoryStructure(Dictionary<string, DirectoryTreeItem> directoryLookup, string path)
    {
        // Split the path into parts
        var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        var currentPath = "/";

        // Create each directory in the path
        foreach (var part in parts)
        {
            var parentPath = currentPath;
            currentPath = $"{(parentPath == "/" ? "" : parentPath)}/{part}";

            if (!directoryLookup.ContainsKey(currentPath))
            {
                var parentDir = directoryLookup[parentPath];
                var dir = new DirectoryTreeItem
                {
                    Name = part,
                    Path = currentPath
                };

                directoryLookup[currentPath] = dir;
                parentDir.Children.Add(dir);
            }
        }
    }

    /// <summary>
    /// Gets the parent path of a path
    /// </summary>
    private string GetParentPath(string path)
    {
        var lastSlashIndex = path.LastIndexOf('/');
        if (lastSlashIndex <= 0)
        {
            return "/";
        }

        return path.Substring(0, lastSlashIndex);
    }

    /// <summary>
    /// Gets the name of an item from its path
    /// </summary>
    private string GetItemName(string path)
    {
        var lastSlashIndex = path.LastIndexOf('/');
        if (lastSlashIndex < 0 || lastSlashIndex >= path.Length - 1)
        {
            return path;
        }

        return path.Substring(lastSlashIndex + 1);
    }

    /// <summary>
    /// Converts a wildcard pattern to a regex pattern
    /// </summary>
    private string WildcardToRegex(string pattern)
    {
        return "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".")
            + "$";
    }

    /// <summary>
    /// Checks if a file matches a pattern
    /// </summary>
    private bool IsFileMatchingPattern(string filePath, string pattern)
    {
        var fileName = Path.GetFileName(filePath);
        var regexPattern = WildcardToRegex(pattern);
        return Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase);
    }
}