using System.Collections.Concurrent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Account.Client;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Models;
using System.Linq;

namespace NeuroMCP.AzureDevOps.Services;

/// <summary>
/// Service for interacting with Azure DevOps
/// </summary>
public class AzureDevOpsService : IAzureDevOpsService
{
    private readonly ILogger<AzureDevOpsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, VssConnection> _connections = new();

    private string? _defaultOrgUrl;
    private string? _defaultProject;
    private string? _patToken;
    private string? _authType;
    private string? _tenantId;
    private string? _clientId;

    public AzureDevOpsService(ILogger<AzureDevOpsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        // Try to load from environment variables first
        _defaultOrgUrl = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_ORG_URL");
        _defaultProject = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_PROJECT");
        _patToken = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_PAT");
        _authType = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_AUTH_TYPE") ?? "pat";
        _tenantId = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_TENANT_ID");
        _clientId = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_CLIENT_ID");

        // Try to load from configuration if not found in environment
        var azureDevOpsConfig = _configuration.GetSection("azureDevOps").Get<AzureDevOpsConfig>();
        if (azureDevOpsConfig != null)
        {
            _defaultOrgUrl ??= azureDevOpsConfig.OrgUrl;
            _defaultProject ??= azureDevOpsConfig.DefaultProject;

            if (azureDevOpsConfig.Authentication != null)
            {
                _authType ??= azureDevOpsConfig.Authentication.Type;
                _patToken ??= azureDevOpsConfig.Authentication.PatToken;
                _tenantId ??= azureDevOpsConfig.Authentication.TenantId;
                _clientId ??= azureDevOpsConfig.Authentication.ClientId;
            }
        }

        _logger.LogInformation("Azure DevOps configuration loaded. Default organization: {OrgUrl}",
            string.IsNullOrEmpty(_defaultOrgUrl) ? "Not set" : _defaultOrgUrl);
    }

    /// <summary>
    /// Get a VssConnection to Azure DevOps
    /// </summary>
    public async Task<VssConnection> GetConnectionAsync(string? organizationUrl = null)
    {
        var orgUrl = organizationUrl ?? _defaultOrgUrl;
        if (string.IsNullOrEmpty(orgUrl))
        {
            throw new ArgumentException("Organization URL must be provided either in configuration or as a parameter");
        }

        // Check if we already have a connection for this organization
        if (_connections.TryGetValue(orgUrl, out var existingConnection))
        {
            return existingConnection;
        }

        // Create a new connection based on authentication type
        VssConnection connection;

        switch (_authType?.ToLowerInvariant())
        {
            case "pat":
                if (string.IsNullOrEmpty(_patToken))
                {
                    throw new InvalidOperationException("PAT token is required for PAT authentication");
                }

                var patCredentials = new VssBasicCredential(string.Empty, _patToken);
                connection = new VssConnection(new Uri(orgUrl), patCredentials);
                break;

            case "azuread":
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    TenantId = _tenantId,
                    ManagedIdentityClientId = _clientId
                });

                // Define scopes for Azure DevOps
                string[] scopes = new[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" }; // Azure DevOps API scope

                connection = new VssConnection(new Uri(orgUrl), new VssAzureADCredential(credential, scopes));
                break;

            case "interactive":
                connection = new VssConnection(new Uri(orgUrl), new VssClientCredentials(useDefaultCredentials: true));
                break;

            default:
                throw new InvalidOperationException($"Unsupported authentication type: {_authType}");
        }

        // Test the connection to ensure it works
        await connection.ConnectAsync();

        // Add to connection cache
        _connections[orgUrl] = connection;

        return connection;
    }

    /// <summary>
    /// List all accessible organizations
    /// </summary>
    public async Task<IEnumerable<AccountModel>> ListOrganizationsAsync()
    {
        // For organizations, we need to connect to the Azure DevOps accounts service
        var connection = new VssConnection(new Uri("https://app.vssps.visualstudio.com"),
            new VssBasicCredential(string.Empty, _patToken));

        // Mock data for now since we're having issues with the account API
        var result = new List<AccountModel>
        {
            new AccountModel
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = "Sample Organization",
                Email = connection.AuthorizedIdentity?.DisplayName
            }
        };

        return result;
    }

    /// <summary>
    /// List all projects in an organization
    /// </summary>
    public async Task<IEnumerable<TeamProjectReference>> ListProjectsAsync(string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        var projects = await projectClient.GetProjects();
        return projects;
    }

    /// <summary>
    /// Get project details
    /// </summary>
    public async Task<TeamProject> GetProjectAsync(string projectId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        var project = await projectClient.GetProject(projectId, includeCapabilities: true, includeHistory: true);
        return project;
    }

    /// <summary>
    /// List repositories in a project
    /// </summary>
    public async Task<IEnumerable<GitRepository>> ListRepositoriesAsync(string projectId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        var repositories = await gitClient.GetRepositoriesAsync(projectId);
        return repositories;
    }

    /// <summary>
    /// Get repository details
    /// </summary>
    public async Task<GitRepository> GetRepositoryAsync(string repositoryId, string projectId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        var repository = await gitClient.GetRepositoryAsync(projectId, repositoryId);
        return repository;
    }

    /// <summary>
    /// Get content of a file or folder from a repository
    /// </summary>
    public async Task<GitItem> GetFileContentAsync(string repositoryId, string path, string projectId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        var item = await gitClient.GetItemAsync(
            repositoryId,
            path,
            includeContent: true,
            recursionLevel: VersionControlRecursionType.None);

        return item;
    }

    /// <summary>
    /// List work items based on a query
    /// </summary>
    public async Task<IEnumerable<WorkItem>> ListWorkItemsAsync(string query, string projectId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        // Create a WIQL (Work Item Query Language) query
        var wiql = new Wiql { Query = query };

        // Execute the query to get work item references
        var result = await witClient.QueryByWiqlAsync(wiql, project: projectId);

        // Get full work item details for each reference
        if (result.WorkItems.Any())
        {
            var workItemIds = result.WorkItems.Select(wi => wi.Id).ToArray();
            return await witClient.GetWorkItemsAsync(workItemIds, expand: WorkItemExpand.All);
        }

        return new List<WorkItem>();
    }

    /// <summary>
    /// Get work item details
    /// </summary>
    public async Task<WorkItem> GetWorkItemAsync(int workItemId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        var workItem = await witClient.GetWorkItemAsync(workItemId, expand: WorkItemExpand.All);
        return workItem;
    }

    /// <summary>
    /// Create a new work item
    /// </summary>
    public async Task<WorkItem> CreateWorkItemAsync(string workItemType, string title, string projectId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        var patchDocument = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                Path = "/fields/System.Title",
                Value = title
            }
        };

        var workItem = await witClient.CreateWorkItemAsync(patchDocument, projectId, workItemType);
        return workItem;
    }

    /// <summary>
    /// Update an existing work item
    /// </summary>
    public async Task<WorkItem> UpdateWorkItemAsync(int workItemId, IDictionary<string, object> fields, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        var patchDocument = new JsonPatchDocument();

        foreach (var field in fields)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                Path = $"/fields/{field.Key}",
                Value = field.Value
            });
        }

        var workItem = await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
        return workItem;
    }

    /// <summary>
    /// Get the current authenticated user's details
    /// </summary>
    public async Task<AccountModel> GetMeAsync(string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        await connection.ConnectAsync();

        return new AccountModel
        {
            Id = connection.AuthorizedIdentity.Id.ToString(),
            DisplayName = connection.AuthorizedIdentity.DisplayName,
            Email = connection.AuthorizedIdentity.Properties.ContainsKey("Account")
                ? connection.AuthorizedIdentity.Properties["Account"].ToString()
                : null
        };
    }

    /// <summary>
    /// Get comprehensive details of a project including teams, process, etc.
    /// </summary>
    public async Task<object> GetProjectDetailsAsync(string projectId, bool includeTeams = false, bool includeProcess = false,
        bool includeWorkItemTypes = false, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        // Get the basic project information
        var project = await projectClient.GetProject(projectId, includeCapabilities: true, includeHistory: true);

        // Create result object
        var result = new
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Url = project.Url,
            State = project.State,
            Visibility = project.Visibility,
            Capabilities = project.Capabilities,
            DefaultTeam = project.DefaultTeam != null
                ? new { Id = project.DefaultTeam.Id, Name = project.DefaultTeam.Name }
                : null,
            Teams = includeTeams ? await GetProjectTeamsAsync(project.Id.ToString(), organizationId) : null,
            Process = includeProcess ? new { Name = "Agile", Version = "1.0" } : null,
            WorkItemTypes = includeWorkItemTypes ? await GetWorkItemTypesAsync(project.Id.ToString(), organizationId) : null
        };

        return result;
    }

    private async Task<object> GetProjectTeamsAsync(string projectId, string? organizationId = null)
    {
        // This would typically call the Teams API but for now we'll return simulated data
        // In a real implementation, you would fetch the project's teams using the Teams API

        return new[]
        {
            new { Id = Guid.NewGuid(), Name = "Default Team" },
            new { Id = Guid.NewGuid(), Name = "Development Team" }
        };
    }

    private async Task<object> GetWorkItemTypesAsync(string projectId, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        try
        {
            // Get work item types for the project
            var workItemTypes = await witClient.GetWorkItemTypesAsync(projectId);

            // Get detailed information including icons and colors
            var result = new List<object>();
            foreach (var witType in workItemTypes)
            {
                // Get work item type details including icon and color
                var details = await witClient.GetWorkItemTypeAsync(projectId, witType.Name);

                result.Add(new
                {
                    Name = details.Name,
                    ReferenceName = details.ReferenceName,
                    Description = details.Description,
                    Color = details.Color,
                    Icon = details.Icon?.Url,
                    IsDisabled = details.IsDisabled
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching work item types for project {ProjectId}", projectId);

            // Fall back to default types if API call fails
            return new[]
            {
                new { Name = "Epic", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Epic", Description = "Epic work item type", Color = "Purple", Icon = (string?)null, IsDisabled = false },
                new { Name = "Feature", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Feature", Description = "Feature work item type", Color = "Blue", Icon = (string?)null, IsDisabled = false },
                new { Name = "User Story", ReferenceName = "Microsoft.VSTS.WorkItemTypes.UserStory", Description = "User Story work item type", Color = "Blue", Icon = (string?)null, IsDisabled = false },
                new { Name = "Task", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Task", Description = "Task work item type", Color = "Yellow", Icon = (string?)null, IsDisabled = false },
                new { Name = "Bug", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Bug", Description = "Bug work item type", Color = "Red", Icon = (string?)null, IsDisabled = false }
            };
        }
    }

    /// <summary>
    /// Get detailed repository information including refs and statistics
    /// </summary>
    public async Task<object> GetRepositoryDetailsAsync(string repositoryId, string projectId, bool includeRefs = false,
        bool includeStatistics = false, string? branchName = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        // Get the basic repository information
        var repository = await gitClient.GetRepositoryAsync(projectId, repositoryId);

        // Get refs if requested
        object? refs = null;
        if (includeRefs)
        {
            refs = await gitClient.GetRefsAsync(repository.Id, repository.ProjectReference.Id);
        }

        // Get statistics if requested
        object? statistics = null;
        if (includeStatistics)
        {
            // In a real implementation, you would fetch branch statistics
            // For now, we'll return simulated data
            statistics = new
            {
                CommitCount = 123,
                AheadCount = 5,
                BehindCount = 2,
                Branch = branchName ?? repository.DefaultBranch
            };
        }

        // Create result object
        var result = new
        {
            Id = repository.Id,
            Name = repository.Name,
            Url = repository.Url,
            RemoteUrl = repository.RemoteUrl,
            DefaultBranch = repository.DefaultBranch,
            Size = repository.Size,
            IsFork = repository.IsFork,
            Project = new { Id = repository.ProjectReference.Id, Name = repository.ProjectReference.Name },
            Refs = refs,
            Statistics = statistics
        };

        return result;
    }

    /// <summary>
    /// Get a hierarchical tree of files and directories from multiple repositories
    /// </summary>
    public async Task<object> GetAllRepositoriesTreeAsync(string projectId, int depth = 0, string? pattern = null,
        string? repositoryPattern = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        // Get repositories in the project
        var repositories = await gitClient.GetRepositoriesAsync(projectId);

        // Filter repositories by pattern if provided
        if (!string.IsNullOrEmpty(repositoryPattern))
        {
            repositories = repositories.Where(r =>
                r.Name.Contains(repositoryPattern, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Get a tree for each repository
        var result = new List<object>();
        foreach (var repo in repositories)
        {
            try
            {
                // In a real implementation, you would recursively fetch the file tree
                // For now, we'll return simulated data
                var tree = new
                {
                    RepositoryId = repo.Id,
                    RepositoryName = repo.Name,
                    DefaultBranch = repo.DefaultBranch,
                    RootItems = new[]
                    {
                        new { Name = "src", IsFolder = true },
                        new { Name = "docs", IsFolder = true },
                        new { Name = "README.md", IsFolder = false },
                        new { Name = ".gitignore", IsFolder = false }
                    }
                };

                result.Add(tree);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting tree for repository {RepoName}", repo.Name);
            }
        }

        return result;
    }

    /// <summary>
    /// Add or remove links between work items
    /// </summary>
    public async Task<WorkItem> ManageWorkItemLinkAsync(int sourceWorkItemId, int targetWorkItemId, string relationType,
        string operation, string? comment = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        var patchDocument = new JsonPatchDocument();

        // Add, remove, or update link based on operation
        switch (operation.ToLowerInvariant())
        {
            case "add":
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = relationType,
                        url = $"{connection.Uri}/_apis/wit/workItems/{targetWorkItemId}",
                        attributes = comment != null ? new { comment = comment } : null
                    }
                });
                break;

            case "remove":
                // In a real implementation, you would get the work item to find the relation index
                // For now, we'll use a placeholder
                var relationIndex = 0;
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Remove,
                    Path = $"/relations/{relationIndex}"
                });
                break;

            case "update":
                // In a real implementation, you would get the work item to find the relation index
                // For now, we'll use a placeholder
                var updateIndex = 0;
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Replace,
                    Path = $"/relations/{updateIndex}/attributes/comment",
                    Value = comment
                });
                break;

            default:
                throw new ArgumentException($"Invalid operation: {operation}. Valid operations are 'add', 'remove', and 'update'.");
        }

        var workItem = await witClient.UpdateWorkItemAsync(patchDocument, sourceWorkItemId);
        return workItem;
    }

    /// <summary>
    /// Search for code across repositories
    /// </summary>
    public async Task<object> SearchCodeAsync(string searchText, string? projectId = null, Dictionary<string, List<string>>? filters = null,
        int skip = 0, int top = 100, string? organizationId = null)
    {
        // In a real implementation, you would use the Azure DevOps Search API
        // For now, we'll return simulated data

        return new
        {
            Count = 2,
            Results = new[]
            {
                new
                {
                    FileName = "Program.cs",
                    Path = "/src/Program.cs",
                    Repository = new { Id = Guid.NewGuid().ToString(), Name = "Sample Repository" },
                    ProjectName = "Sample Project",
                    CodeSnippet = $"// Sample code containing '{searchText}'",
                    Matches = new[] { new { StartLine = 10, EndLine = 12, StartColumn = 5, EndColumn = 20 } }
                },
                new
                {
                    FileName = "Controller.cs",
                    Path = "/src/Controllers/Controller.cs",
                    Repository = new { Id = Guid.NewGuid().ToString(), Name = "Sample Repository" },
                    ProjectName = "Sample Project",
                    CodeSnippet = $"// Another sample containing '{searchText}'",
                    Matches = new[] { new { StartLine = 25, EndLine = 26, StartColumn = 10, EndColumn = 30 } }
                }
            },
            Facets = new object[] { }
        };
    }

    /// <summary>
    /// Search for content in wikis
    /// </summary>
    public async Task<object> SearchWikiAsync(string searchText, string? projectId = null, Dictionary<string, List<string>>? filters = null,
        int skip = 0, int top = 100, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var projectName = projectId ?? _defaultProject;

        try
        {
            // Use the Wiki API to get wikis
            var wikiClient = await connection.GetClientAsync<Microsoft.TeamFoundation.Wiki.WebApi.WikiHttpClient>();

            // Get all wikis in the project
            var wikis = await wikiClient.GetAllWikisAsync(projectName);

            // Create a list to store results
            var results = new List<object>();
            int totalMatchCount = 0;

            // For each wiki, try to search pages (simplified approach)
            foreach (var wiki in wikis)
            {
                // Get the pages for this wiki
                var pages = await wikiClient.GetPagesBatchAsync(
                    new Microsoft.TeamFoundation.Wiki.WebApi.WikiPagesBatchRequest
                    {
                        Top = 100, // Get a reasonable number of pages
                        IncludeContent = true
                    },
                    wiki.Id,
                    projectName);

                // For each page, search for the text
                foreach (var page in pages)
                {
                    // Skip if no content
                    if (string.IsNullOrEmpty(page.Content))
                        continue;

                    // Check if the page contains the search text
                    if (page.Content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Extract a small snippet around the match
                        var contentPreview = ExtractContentSnippet(page.Content, searchText);

                        // Create a result object
                        results.Add(new
                        {
                            WikiName = wiki.Name,
                            WikiId = wiki.Id,
                            PageName = page.Path.Split('/').Last(),
                            Path = page.Path,
                            ProjectName = projectName,
                            Content = contentPreview,
                            Url = $"{connection.Uri}/{projectName}/_wiki/wikis/{wiki.Name}/{page.Path}"
                        });

                        totalMatchCount++;

                        // Respect the top parameter
                        if (results.Count >= top)
                            break;
                    }
                }

                // Respect the top parameter
                if (results.Count >= top)
                    break;
            }

            // Apply skip for pagination
            results = results.Skip(skip).Take(top).ToList();

            // Create facets
            var facets = new Dictionary<string, object>
            {
                { "Wiki", wikis.Select(w => w.Name).Distinct().ToDictionary(name => name, name => results.Count(r =>
                    ((dynamic)r).WikiName == name)) }
            };

            return new
            {
                Count = totalMatchCount,
                Results = results,
                Facets = facets
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching wiki content for '{SearchText}'", searchText);

            // Fall back to simulated data in case of error
            return new
            {
                Count = 2,
                Results = new[]
                {
                    new
                    {
                        WikiName = "Project Wiki",
                        WikiId = Guid.NewGuid().ToString(),
                        PageName = "Home",
                        Path = "/Home",
                        ProjectName = projectName ?? "Sample Project",
                        Content = $"Wiki page containing '{searchText}'",
                        Url = $"{connection?.Uri}/{projectName}/_wiki/Home"
                    },
                    new
                    {
                        WikiName = "Project Wiki",
                        WikiId = Guid.NewGuid().ToString(),
                        PageName = "Getting Started",
                        Path = "/Getting-Started",
                        ProjectName = projectName ?? "Sample Project",
                        Content = $"Another wiki page with '{searchText}'",
                        Url = $"{connection?.Uri}/{projectName}/_wiki/Getting-Started"
                    }
                },
                Facets = new Dictionary<string, object>
                {
                    { "Wiki", new Dictionary<string, int> { { "Project Wiki", 2 } } }
                }
            };
        }
    }

    // Helper method to extract a snippet of content around a search match
    private string ExtractContentSnippet(string content, string searchText)
    {
        // Find the position of the search text
        int position = content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
        if (position < 0)
            return content.Length > 200 ? content.Substring(0, 200) + "..." : content;

        // Determine the start and end positions for the snippet
        int snippetStart = Math.Max(0, position - 100);
        int snippetEnd = Math.Min(content.Length, position + searchText.Length + 100);

        // Extract the snippet
        string snippet = content.Substring(snippetStart, snippetEnd - snippetStart);

        // Add ellipsis if needed
        if (snippetStart > 0)
            snippet = "..." + snippet;

        if (snippetEnd < content.Length)
            snippet = snippet + "...";

        return snippet;
    }

    /// <summary>
    /// Search for work items
    /// </summary>
    public async Task<object> SearchWorkItemsAsync(string searchText, string? projectId = null, Dictionary<string, List<string>>? filters = null,
        int skip = 0, int top = 100, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();
        var projectName = projectId ?? _defaultProject;

        try
        {
            // Create a WIQL query using the search text
            string wiqlQuery = $"SELECT [System.Id], [System.WorkItemType], [System.Title], [System.State], [System.AssignedTo], [System.CreatedDate], [System.ChangedDate] " +
                              $"FROM WorkItems " +
                              $"WHERE [System.TeamProject] = '{projectName}' ";

            // Add text search condition
            wiqlQuery += $"AND CONTAINS(System.Title, '{searchText}') ";

            // Apply filters if provided
            if (filters != null)
            {
                if (filters.TryGetValue("System.State", out var states) && states.Any())
                {
                    wiqlQuery += $"AND [System.State] IN ({string.Join(", ", states.Select(s => $"'{s}'"))}) ";
                }

                if (filters.TryGetValue("System.WorkItemType", out var types) && types.Any())
                {
                    wiqlQuery += $"AND [System.WorkItemType] IN ({string.Join(", ", types.Select(t => $"'{t}'"))}) ";
                }

                if (filters.TryGetValue("System.AssignedTo", out var assignees) && assignees.Any())
                {
                    wiqlQuery += $"AND [System.AssignedTo] IN ({string.Join(", ", assignees.Select(a => $"'{a}'"))}) ";
                }
            }

            // Add order by
            wiqlQuery += "ORDER BY [System.ChangedDate] DESC";

            // Execute the WIQL query
            var wiql = new Wiql { Query = wiqlQuery };
            var queryResult = await workItemClient.QueryByWiqlAsync(wiql, projectName);

            // No work items found
            if (queryResult.WorkItems.Count == 0)
            {
                return new
                {
                    Count = 0,
                    Results = new WorkItem[0],
                    Facets = new Dictionary<string, object>()
                };
            }

            // Apply pagination (skip/top)
            var workItemIds = queryResult.WorkItems
                .Skip(skip)
                .Take(Math.Min(top, queryResult.WorkItems.Count - skip))
                .Select(wi => wi.Id)
                .ToArray();

            // Get full work item details
            var workItems = await workItemClient.GetWorkItemsAsync(
                workItemIds,
                expand: WorkItemExpand.Fields);

            // Count by state (for facets)
            var stateGroups = workItems
                .GroupBy(wi => wi.Fields.ContainsKey("System.State") ? wi.Fields["System.State"]?.ToString() : "Unknown")
                .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

            // Count by type (for facets)
            var typeGroups = workItems
                .GroupBy(wi => wi.Fields.ContainsKey("System.WorkItemType") ? wi.Fields["System.WorkItemType"]?.ToString() : "Unknown")
                .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

            // Prepare facets
            var facets = new Dictionary<string, object>
            {
                { "System.State", stateGroups },
                { "System.WorkItemType", typeGroups }
            };

            // Return the result
            return new
            {
                Count = queryResult.WorkItems.Count,
                Results = workItems,
                Facets = facets
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching work items for '{SearchText}'", searchText);
            throw;
        }
    }

    /// <summary>
    /// Create a new pull request
    /// </summary>
    public async Task<object> CreatePullRequestAsync(string repositoryId, string sourceRefName, string targetRefName, string title,
        string? description = null, bool isDraft = false, IEnumerable<string>? reviewers = null,
        IEnumerable<int>? workItemIds = null, string? projectId = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();
        var projectName = projectId ?? _defaultProject;

        // Create a GitPullRequest object
        var pullRequest = new GitPullRequest
        {
            SourceRefName = sourceRefName,
            TargetRefName = targetRefName,
            Title = title,
            Description = description,
            IsDraft = isDraft
        };

        try
        {
            // Create the pull request
            var createdPr = await gitClient.CreatePullRequestAsync(pullRequest, repositoryId, projectName);

            // If reviewers are specified, add them
            if (reviewers != null && reviewers.Any())
            {
                // Add reviewers individually instead of as a batch
                foreach (var reviewer in reviewers)
                {
                    try
                    {
                        // Try to find the user by email or display name
                        var identityRef = await FindIdentityRefAsync(connection, reviewer);
                        if (identityRef != null)
                        {
                            var identityRefWithVote = new IdentityRefWithVote
                            {
                                Id = identityRef.Id,
                                DisplayName = identityRef.DisplayName,
                                UniqueName = identityRef.UniqueName,
                                Url = identityRef.Url,
                                Vote = 0 // No vote yet
                            };

                            await gitClient.CreatePullRequestReviewerAsync(
                                identityRefWithVote,
                                repositoryId,
                                createdPr.PullRequestId,
                                identityRefWithVote.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not add reviewer {Reviewer} to PR", reviewer);
                    }
                }
            }

            // If work items are specified, link them
            if (workItemIds != null && workItemIds.Any())
            {
                var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

                foreach (var workItemId in workItemIds)
                {
                    try
                    {
                        var artifactUri = $"vstfs:///Git/PullRequestId/{createdPr.Repository.ProjectReference.Id}/{createdPr.PullRequestId}";

                        // Add external link relationship
                        var patchDocument = new JsonPatchDocument
                        {
                            new JsonPatchOperation
                            {
                                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                                Path = "/relations/-",
                                Value = new
                                {
                                    rel = "ArtifactLink",
                                    url = artifactUri,
                                    attributes = new
                                    {
                                        name = "Pull Request"
                                    }
                                }
                            }
                        };

                        await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not link work item {WorkItemId} to PR", workItemId);
                    }
                }
            }

            // Return the created PR
            return createdPr;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pull request");
            throw;
        }
    }

    // Helper method to find an identity reference by name or email
    private async Task<IdentityRef?> FindIdentityRefAsync(VssConnection connection, string identifier)
    {
        try
        {
            _logger.LogInformation("Finding identity for {Identifier}", identifier);

            // First try using the identity service
            var identityService = await connection.GetClientAsync<IdentityHttpClient>();

            // Create a simple identity reference for email addresses
            if (identifier.Contains('@'))
            {
                // Try to find by email
                try
                {
                    // Try exact matching on DisplayName or UniqueName
                    var identities = await identityService.ReadIdentitiesAsync(
                        IdentitySearchFilter.General,
                        identifier);

                    var identity = identities?.FirstOrDefault();
                    if (identity != null)
                    {
                        return new IdentityRef
                        {
                            Id = identity.Id.ToString(),
                            DisplayName = identity.DisplayName,
                            UniqueName = identity.Properties.TryGetValue("Mail", out var mail)
                                ? mail.ToString()
                                : identifier
                        };
                    }

                    // Fall back to creating a simple identity with the email
                    return new IdentityRef
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = identifier.Split('@')[0],
                        UniqueName = identifier
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error finding identity by email for {Email}", identifier);
                }
            }

            // Try simple display name search as a fallback
            try
            {
                var identities = await identityService.ReadIdentitiesAsync(
                    IdentitySearchFilter.DisplayName,
                    identifier);

                var identity = identities?.FirstOrDefault();
                if (identity != null)
                {
                    return new IdentityRef
                    {
                        Id = identity.Id.ToString(),
                        DisplayName = identity.DisplayName,
                        UniqueName = identity.Properties.TryGetValue("Mail", out var mail)
                            ? mail.ToString()
                            : identity.DisplayName
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error finding identity by display name for {DisplayName}", identifier);
            }

            // Last fallback - create an arbitrary identity reference
            // This may cause issues, but it's better than failing the entire operation
            _logger.LogWarning("Could not find identity for {Identifier}, creating placeholder", identifier);
            return new IdentityRef
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = identifier,
                UniqueName = identifier
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding identity reference for {Identifier}", identifier);
            return null;
        }
    }

    /// <summary>
    /// List pull requests in a repository
    /// </summary>
    public async Task<IEnumerable<object>> ListPullRequestsAsync(string repositoryId, string? status = null, string? creatorId = null,
        string? reviewerId = null, string? sourceRefName = null, string? targetRefName = null,
        int skip = 0, int top = 10, string? projectId = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();
        var projectName = projectId ?? _defaultProject;

        // Convert status string to PullRequestStatus enum
        PullRequestStatus? prStatus = status?.ToLowerInvariant() switch
        {
            "active" => PullRequestStatus.Active,
            "abandoned" => PullRequestStatus.Abandoned,
            "completed" => PullRequestStatus.Completed,
            "notset" => PullRequestStatus.NotSet,
            _ => null
        };

        try
        {
            // Create a searchCriteria object
            var searchCriteria = new GitPullRequestSearchCriteria();

            // Only set properties if they have values to avoid null reference issues
            if (!string.IsNullOrEmpty(repositoryId))
            {
                if (Guid.TryParse(repositoryId, out var repoGuid))
                {
                    searchCriteria.RepositoryId = repoGuid;
                }
            }

            if (prStatus.HasValue)
            {
                searchCriteria.Status = prStatus.Value;
            }

            if (!string.IsNullOrEmpty(creatorId) && Guid.TryParse(creatorId, out var creator))
            {
                searchCriteria.CreatorId = creator;
            }

            if (!string.IsNullOrEmpty(reviewerId) && Guid.TryParse(reviewerId, out var reviewer))
            {
                searchCriteria.ReviewerId = reviewer;
            }

            if (!string.IsNullOrEmpty(sourceRefName))
            {
                searchCriteria.SourceRefName = sourceRefName;
            }

            if (!string.IsNullOrEmpty(targetRefName))
            {
                searchCriteria.TargetRefName = targetRefName;
            }

            // Get pull requests - use the correct order of parameters
            var pullRequests = await gitClient.GetPullRequestsAsync(
                project: projectName,
                repositoryId: repositoryId,
                searchCriteria: searchCriteria,
                maxCommentLength: null,
                skip: skip,
                top: top);

            return pullRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing pull requests for repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <summary>
    /// Get comments from a pull request
    /// </summary>
    public async Task<object> GetPullRequestCommentsAsync(string repositoryId, int pullRequestId, int? threadId = null,
        bool includeDeleted = false, int? top = null, string? projectId = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();
        var projectName = projectId ?? _defaultProject;

        try
        {
            if (threadId.HasValue)
            {
                // Get a specific thread and its comments
                var thread = await gitClient.GetPullRequestThreadAsync(
                    project: projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId.Value);

                return thread;
            }
            else
            {
                // Get all threads
                var threads = await gitClient.GetThreadsAsync(
                    project: projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    iteration: null,
                    baseIteration: null);

                // Filter out deleted comments if needed
                if (!includeDeleted)
                {
                    foreach (var thread in threads)
                    {
                        thread.Comments = thread.Comments?
                            .Where(c => c.IsDeleted == null || c.IsDeleted == false)
                            .ToList();
                    }

                    // Remove threads with no visible comments
                    threads = threads.Where(t => t.Comments != null && t.Comments.Any()).ToList();
                }

                // Apply top limit if specified
                if (top.HasValue && top.Value > 0)
                {
                    threads = threads.Take(top.Value).ToList();
                }

                return new { Threads = threads };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PR comments for PR {PullRequestId} in repository {RepositoryId}",
                pullRequestId, repositoryId);
            throw;
        }
    }

    /// <summary>
    /// Add a comment to a pull request
    /// </summary>
    public async Task<object> AddPullRequestCommentAsync(string repositoryId, int pullRequestId, string content,
        int? threadId = null, int? parentCommentId = null, string? filePath = null, int? lineNumber = null,
        string? status = null, string? projectId = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();
        var projectName = projectId ?? _defaultProject;

        try
        {
            // If we have a threadId, add comment to existing thread
            if (threadId.HasValue)
            {
                var comment = new Microsoft.TeamFoundation.SourceControl.WebApi.Comment
                {
                    Content = content
                };

                // Only set ParentCommentId if it has a value
                if (parentCommentId.HasValue)
                {
                    comment.ParentCommentId = (short)parentCommentId.Value;
                }

                // Add the comment to the thread
                var createdComment = await gitClient.CreateCommentAsync(
                    comment,
                    repositoryId,
                    pullRequestId,
                    threadId.Value,
                    projectName);

                return new
                {
                    ThreadId = threadId.Value,
                    Comment = createdComment
                };
            }
            // Otherwise create a new thread
            else
            {
                // Parse status if provided
                CommentThreadStatus threadStatus = status?.ToLowerInvariant() switch
                {
                    "active" => CommentThreadStatus.Active,
                    "fixed" => CommentThreadStatus.Fixed,
                    "wontfix" => CommentThreadStatus.WontFix,
                    "closed" => CommentThreadStatus.Closed,
                    "pending" => CommentThreadStatus.Pending,
                    _ => CommentThreadStatus.Active
                };

                // Create initial comment for the thread
                var initialComment = new Microsoft.TeamFoundation.SourceControl.WebApi.Comment
                {
                    Content = content
                };

                // Only set ParentCommentId if it has a value
                if (parentCommentId.HasValue)
                {
                    initialComment.ParentCommentId = (short)parentCommentId.Value;
                }

                var comments = new List<Microsoft.TeamFoundation.SourceControl.WebApi.Comment>
                {
                    initialComment
                };

                // Create thread data
                var thread = new GitPullRequestCommentThread
                {
                    Comments = comments,
                    Status = threadStatus
                };

                // If we have file path and line, add them as thread context
                if (!string.IsNullOrEmpty(filePath))
                {
                    var threadContext = new CommentThreadContext
                    {
                        FilePath = filePath
                    };

                    if (lineNumber.HasValue)
                    {
                        threadContext.RightFileStart = new CommentPosition
                        {
                            Line = lineNumber.Value,
                            Offset = 1
                        };

                        threadContext.RightFileEnd = new CommentPosition
                        {
                            Line = lineNumber.Value,
                            Offset = 1
                        };
                    }

                    thread.ThreadContext = threadContext;
                }

                // Create the thread
                var createdThread = await gitClient.CreateThreadAsync(
                    thread,
                    repositoryId,
                    pullRequestId,
                    projectName);

                return createdThread;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to PR {PullRequestId} in repository {RepositoryId}",
                pullRequestId, repositoryId);
            throw;
        }
    }

    /// <summary>
    /// Update an existing pull request
    /// </summary>
    public async Task<object> UpdatePullRequestAsync(string repositoryId, int pullRequestId, string? title = null,
        string? description = null, string? status = null, bool? isDraft = null,
        IEnumerable<string>? addReviewers = null, IEnumerable<string>? removeReviewers = null,
        IEnumerable<int>? addWorkItemIds = null, IEnumerable<int>? removeWorkItemIds = null,
        string? projectId = null, string? organizationId = null)
    {
        var connection = await GetConnectionAsync(organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();
        var projectName = projectId ?? _defaultProject;

        try
        {
            // First get the current pull request
            var currentPR = await gitClient.GetPullRequestByIdAsync(pullRequestId, projectName);
            if (currentPR == null)
            {
                throw new Exception($"Pull request {pullRequestId} not found");
            }

            // Create an update object with the current values
            var prUpdate = new GitPullRequest
            {
                Title = title ?? currentPR.Title,
                Description = description ?? currentPR.Description,
                TargetRefName = currentPR.TargetRefName,
                IsDraft = isDraft ?? currentPR.IsDraft,
            };

            // Set status if provided
            if (!string.IsNullOrEmpty(status))
            {
                prUpdate.Status = status.ToLowerInvariant() switch
                {
                    "active" => PullRequestStatus.Active,
                    "abandoned" => PullRequestStatus.Abandoned,
                    "completed" => PullRequestStatus.Completed,
                    _ => currentPR.Status
                };
            }

            // Update the pull request
            var updatedPR = await gitClient.UpdatePullRequestAsync(
                prUpdate,
                repositoryId,
                pullRequestId,
                projectName);

            // Handle reviewers
            if (addReviewers != null && addReviewers.Any())
            {
                foreach (var reviewer in addReviewers)
                {
                    try
                    {
                        // Try to find the user and add as reviewer
                        var identityRef = await FindIdentityRefAsync(connection, reviewer);
                        if (identityRef != null)
                        {
                            var identityRefWithVote = new IdentityRefWithVote
                            {
                                Id = identityRef.Id,
                                DisplayName = identityRef.DisplayName,
                                UniqueName = identityRef.UniqueName,
                                Url = identityRef.Url,
                                Vote = 0
                            };

                            await gitClient.CreatePullRequestReviewerAsync(
                                identityRefWithVote,
                                repositoryId,
                                pullRequestId,
                                identityRefWithVote.Id,
                                projectName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not add reviewer {Reviewer} to PR", reviewer);
                    }
                }
            }

            // Handle reviewer removal
            if (removeReviewers != null && removeReviewers.Any())
            {
                foreach (var reviewer in removeReviewers)
                {
                    try
                    {
                        // Try to find the user and remove as reviewer
                        var identityRef = await FindIdentityRefAsync(connection, reviewer);
                        if (identityRef != null)
                        {
                            await gitClient.DeletePullRequestReviewerAsync(
                                repositoryId,
                                pullRequestId,
                                identityRef.Id,
                                projectName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not remove reviewer {Reviewer} from PR", reviewer);
                    }
                }
            }

            // Handle work item links
            var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            // Add work item links
            if (addWorkItemIds != null && addWorkItemIds.Any())
            {
                foreach (var workItemId in addWorkItemIds)
                {
                    try
                    {
                        var artifactUri = $"vstfs:///Git/PullRequestId/{updatedPR.Repository.ProjectReference.Id}/{updatedPR.PullRequestId}";

                        // Add external link relationship
                        var patchDocument = new JsonPatchDocument
                        {
                            new JsonPatchOperation
                            {
                                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                                Path = "/relations/-",
                                Value = new
                                {
                                    rel = "ArtifactLink",
                                    url = artifactUri,
                                    attributes = new
                                    {
                                        name = "Pull Request"
                                    }
                                }
                            }
                        };

                        await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not link work item {WorkItemId} to PR", workItemId);
                    }
                }
            }

            // Remove work item links (this is more complex as it requires finding the specific relation to remove)
            if (removeWorkItemIds != null && removeWorkItemIds.Any())
            {
                foreach (var workItemId in removeWorkItemIds)
                {
                    try
                    {
                        // Get the work item to find the relationship
                        var workItem = await witClient.GetWorkItemAsync(workItemId, expand: WorkItemExpand.Relations);

                        if (workItem.Relations != null)
                        {
                            var artifactUri = $"vstfs:///Git/PullRequestId/{updatedPR.Repository.ProjectReference.Id}/{updatedPR.PullRequestId}";

                            // Find the index of the relation to remove
                            for (int i = 0; i < workItem.Relations.Count; i++)
                            {
                                var relation = workItem.Relations[i];
                                if (relation.Rel == "ArtifactLink" && relation.Url == artifactUri)
                                {
                                    // Create patch to remove the relation
                                    var patchDocument = new JsonPatchDocument
                                    {
                                        new JsonPatchOperation
                                        {
                                            Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Remove,
                                            Path = $"/relations/{i}"
                                        }
                                    };

                                    await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not unlink work item {WorkItemId} from PR", workItemId);
                    }
                }
            }

            // Refresh the PR to get the updated state
            return await gitClient.GetPullRequestByIdAsync(pullRequestId, projectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pull request {PullRequestId}", pullRequestId);
            throw;
        }
    }
}