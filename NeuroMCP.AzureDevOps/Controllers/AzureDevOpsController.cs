using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Models;
using NeuroMCP.AzureDevOps.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace NeuroMCP.AzureDevOps.Controllers;

/// <summary>
/// Azure DevOps Integration Controller for NeuroMCP
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AzureDevOpsController : ControllerBase, IMcpTool
{
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly ILogger<AzureDevOpsController> _logger;

    // IMcpTool implementation
    public string Name => "Azure DevOps";
    public string Version => GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";
    public string Description => "Azure DevOps integration for ModelContextProtocol";

    /// <summary>
    /// Constructor for Azure DevOps controller
    /// </summary>
    public AzureDevOpsController(IAzureDevOpsService azureDevOpsService, ILogger<AzureDevOpsController> logger)
    {
        _azureDevOpsService = azureDevOpsService;
        _logger = logger;
    }

    private void RegisterMcpFunctions()
    {
        // Register user related functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_me", GetMe, "Get details of the authenticated user (id, displayName, email)");

        // Register organization functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_list_organizations", ListOrganizations, "List all Azure DevOps organizations accessible to the current authentication");

        // Register project functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_list_projects", ListProjects, "List all projects in an organization");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_project", GetProject, "Get details of a specific project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_project_details", GetProjectDetails, "Get comprehensive details of a project including process, work item types, and teams");

        // Register repository functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_list_repositories", ListRepositories, "List repositories in a project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_repository", GetRepository, "Get details of a specific repository");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_repository_details", GetRepositoryDetails, "Get detailed information about a repository including statistics and refs");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_file_content", GetFileContent, "Get content of a file or directory from a repository");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_all_repositories_tree", GetAllRepositoriesTree, "Displays a hierarchical tree view of files and directories across multiple Azure DevOps repositories within a project, based on their default branches");

        // Register work item functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_list_work_items", ListWorkItems, "List work items in a project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_work_item", GetWorkItem, "Get details of a specific work item");
        _mcpTool.RegisterFunction("mcp_azureDevOps_create_work_item", CreateWorkItem, "Create a new work item");
        _mcpTool.RegisterFunction("mcp_azureDevOps_update_work_item", UpdateWorkItem, "Update an existing work item");
        _mcpTool.RegisterFunction("mcp_azureDevOps_manage_work_item_link", ManageWorkItemLink, "Add or remove links between work items");

        // Register search functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_search_code", SearchCode, "Search for code across repositories in a project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_search_wiki", SearchWiki, "Search for content across wiki pages in a project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_search_work_items", SearchWorkItems, "Search for work items across projects in Azure DevOps");

        // Register pull request functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_create_pull_request", CreatePullRequest, "Create a new pull request");
        _mcpTool.RegisterFunction("mcp_azureDevOps_list_pull_requests", ListPullRequests, "List pull requests in a repository");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_pull_request_comments", GetPullRequestComments, "Get comments from a specific pull request");
        _mcpTool.RegisterFunction("mcp_azureDevOps_add_pull_request_comment", AddPullRequestComment, "Add a comment to a pull request (reply to existing comments or create new threads)");
        _mcpTool.RegisterFunction("mcp_azureDevOps_update_pull_request", UpdatePullRequest, "Update an existing pull request with new properties, link work items, and manage reviewers");

        // Register pipeline functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_list_pipelines", ListPipelines, "List pipelines in a project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_pipeline", GetPipeline, "Get details of a specific pipeline");
        _mcpTool.RegisterFunction("mcp_azureDevOps_trigger_pipeline", TriggerPipeline, "Trigger a pipeline run");

        // Register wiki functions
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_wikis", GetWikis, "Get details of wikis in a project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_get_wiki_page", GetWikiPage, "Get the content of a wiki page");
        _mcpTool.RegisterFunction("mcp_azureDevOps_create_wiki", CreateWiki, "Create a new wiki in the project");
        _mcpTool.RegisterFunction("mcp_azureDevOps_update_wiki_page", UpdateWikiPage, "Update content of a wiki page");
    }

    #region User Functions

    private async Task<object> GetMe(JsonElement parameters)
    {
        try
        {
            var user = await _azureDevOpsService.GetCurrentUserAsync();
            return new
            {
                id = user.Id,
                displayName = user.DisplayName,
                descriptor = user.Descriptor.ToString(),
                directoryAlias = user.DirectoryAlias
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            throw;
        }
    }

    #endregion

    #region Organization Functions

    private async Task<object> ListOrganizations(JsonElement parameters)
    {
        try
        {
            var orgs = await _azureDevOpsService.ListOrganizationsAsync();
            return orgs.Select(o => new
            {
                id = o.AccountId,
                name = o.AccountName,
                type = o.AccountType,
                accountUri = o.AccountUri,
                namespaceId = o.AccountNamespaceId,
                status = o.AccountStatus
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing organizations");
            throw;
        }
    }

    #endregion

    #region Project Functions

    private async Task<object> ListProjects(JsonElement parameters)
    {
        try
        {
            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            var projects = await _azureDevOpsService.ListProjectsAsync(organizationId);
            return projects.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                description = p.Description,
                state = p.State,
                visibility = p.Visibility,
                lastUpdateTime = p.LastUpdateTime
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing projects");
            throw;
        }
    }

    private async Task<object> GetProject(JsonElement parameters)
    {
        try
        {
            var projectId = parameters.GetProperty("projectId").GetString() ?? throw new ArgumentNullException("projectId");

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            var project = await _azureDevOpsService.GetProjectAsync(projectId, organizationId);
            return new
            {
                id = project.Id,
                name = project.Name,
                description = project.Description,
                state = project.State,
                visibility = project.Visibility,
                url = project.Url,
                defaultTeam = project.DefaultTeam != null ? new { id = project.DefaultTeam.Id, name = project.DefaultTeam.Name } : null,
                capabilities = project.Capabilities
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project details");
            throw;
        }
    }

    private async Task<object> GetProjectDetails(JsonElement parameters)
    {
        // Implementation omitted for brevity - would be similar to GetProject but with additional details
        // This would involve getting teams, process, work item types, etc.
        return new { message = "Not implemented yet" };
    }

    #endregion

    #region Repository Functions

    private async Task<object> ListRepositories(JsonElement parameters)
    {
        try
        {
            var projectId = parameters.GetProperty("projectId").GetString() ?? throw new ArgumentNullException("projectId");

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            var repos = await _azureDevOpsService.ListRepositoriesAsync(projectId, organizationId);
            return repos.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                url = r.Url,
                remoteUrl = r.RemoteUrl,
                defaultBranch = r.DefaultBranch,
                size = r.Size,
                isFork = r.IsFork,
                project = new { id = r.ProjectReference.Id, name = r.ProjectReference.Name }
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing repositories");
            throw;
        }
    }

    private async Task<object> GetRepository(JsonElement parameters)
    {
        try
        {
            var repositoryId = parameters.GetProperty("repositoryId").GetString() ?? throw new ArgumentNullException("repositoryId");
            var projectId = parameters.GetProperty("projectId").GetString() ?? throw new ArgumentNullException("projectId");

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            var repo = await _azureDevOpsService.GetRepositoryAsync(repositoryId, projectId, organizationId);
            return new
            {
                id = repo.Id,
                name = repo.Name,
                url = repo.Url,
                remoteUrl = repo.RemoteUrl,
                defaultBranch = repo.DefaultBranch,
                size = repo.Size,
                isFork = repo.IsFork,
                project = new { id = repo.ProjectReference.Id, name = repo.ProjectReference.Name }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository details");
            throw;
        }
    }

    private async Task<object> GetRepositoryDetails(JsonElement parameters)
    {
        // This would be similar to GetRepository but with additional details
        return new { message = "Not implemented yet" };
    }

    private async Task<object> GetFileContent(JsonElement parameters)
    {
        try
        {
            var repositoryId = parameters.GetProperty("repositoryId").GetString() ?? throw new ArgumentNullException("repositoryId");
            var path = parameters.GetProperty("path").GetString() ?? "/";
            var projectId = parameters.GetProperty("projectId").GetString() ?? throw new ArgumentNullException("projectId");

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            var item = await _azureDevOpsService.GetFileContentAsync(repositoryId, path, projectId, organizationId);
            return new
            {
                path = item.Path,
                isFolder = item.IsFolder,
                isSymbolicLink = item.IsSymbolicLink,
                content = item.Content,
                size = item.ContentMetadata?.ContentLength,
                url = item.Url,
                commitId = item.CommitId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file content");
            throw;
        }
    }

    private async Task<object> GetAllRepositoriesTree(JsonElement parameters)
    {
        // This would involve getting all repositories in a project and listing their folder structures
        return new { message = "Not implemented yet" };
    }

    #endregion

    #region Work Item Functions

    private async Task<object> ListWorkItems(JsonElement parameters)
    {
        try
        {
            // This method requires a WIQL query
            var projectId = parameters.GetProperty("projectId").GetString() ?? throw new ArgumentNullException("projectId");

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            // Get WIQL query from parameters
            string query;
            if (parameters.TryGetProperty("wiql", out var wiqlElement) && wiqlElement.ValueKind != JsonValueKind.Null)
            {
                query = wiqlElement.GetString() ??
                    "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.TeamProject] = @project ORDER BY [System.Id]";
            }
            else
            {
                query = "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.TeamProject] = @project ORDER BY [System.Id]";
            }

            var workItems = await _azureDevOpsService.ListWorkItemsAsync(query, projectId, organizationId);

            return workItems.Select(wi => new
            {
                id = wi.Id,
                fields = wi.Fields.ToDictionary(
                    f => f.Key,
                    f => f.Value
                ),
                url = wi.Url
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing work items");
            throw;
        }
    }

    private async Task<object> GetWorkItem(JsonElement parameters)
    {
        try
        {
            var workItemId = parameters.GetProperty("workItemId").GetInt32();

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            var workItem = await _azureDevOpsService.GetWorkItemAsync(workItemId, organizationId);

            return new
            {
                id = workItem.Id,
                fields = workItem.Fields.ToDictionary(
                    f => f.Key,
                    f => f.Value
                ),
                url = workItem.Url,
                relations = workItem.Relations?.Select(r => new
                {
                    rel = r.Rel,
                    url = r.Url,
                    attributes = r.Attributes
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work item");
            throw;
        }
    }

    private async Task<object> CreateWorkItem(JsonElement parameters)
    {
        try
        {
            var workItemType = parameters.GetProperty("workItemType").GetString() ?? throw new ArgumentNullException("workItemType");
            var title = parameters.GetProperty("title").GetString() ?? throw new ArgumentNullException("title");
            var projectId = parameters.GetProperty("projectId").GetString() ?? throw new ArgumentNullException("projectId");

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            var workItem = await _azureDevOpsService.CreateWorkItemAsync(workItemType, title, projectId, organizationId);

            return new
            {
                id = workItem.Id,
                fields = workItem.Fields.ToDictionary(
                    f => f.Key,
                    f => f.Value
                ),
                url = workItem.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work item");
            throw;
        }
    }

    private async Task<object> UpdateWorkItem(JsonElement parameters)
    {
        try
        {
            var workItemId = parameters.GetProperty("workItemId").GetInt32();

            string? organizationId = null;
            if (parameters.TryGetProperty("organizationId", out var orgIdElement) && orgIdElement.ValueKind != JsonValueKind.Null)
            {
                organizationId = orgIdElement.GetString();
            }

            // Create a dictionary of fields to update
            var fields = new Dictionary<string, object>();

            // Add title if provided
            if (parameters.TryGetProperty("title", out var titleElement) && titleElement.ValueKind != JsonValueKind.Null)
            {
                fields["System.Title"] = titleElement.GetString()!;
            }

            // Add state if provided
            if (parameters.TryGetProperty("state", out var stateElement) && stateElement.ValueKind != JsonValueKind.Null)
            {
                fields["System.State"] = stateElement.GetString()!;
            }

            // Add assignedTo if provided
            if (parameters.TryGetProperty("assignedTo", out var assignedToElement) && assignedToElement.ValueKind != JsonValueKind.Null)
            {
                fields["System.AssignedTo"] = assignedToElement.GetString()!;
            }

            // Add description if provided
            if (parameters.TryGetProperty("description", out var descElement) && descElement.ValueKind != JsonValueKind.Null)
            {
                fields["System.Description"] = descElement.GetString()!;
            }

            // Add additional fields if provided
            if (parameters.TryGetProperty("additionalFields", out var additionalFieldsElement) &&
                additionalFieldsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in additionalFieldsElement.EnumerateObject())
                {
                    fields[property.Name] = property.Value.GetString()!;
                }
            }

            var workItem = await _azureDevOpsService.UpdateWorkItemAsync(workItemId, fields, organizationId);

            return new
            {
                id = workItem.Id,
                fields = workItem.Fields.ToDictionary(
                    f => f.Key,
                    f => f.Value
                ),
                url = workItem.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work item");
            throw;
        }
    }

    private async Task<object> ManageWorkItemLink(JsonElement parameters)
    {
        // Implementation for adding or removing links between work items
        return new { message = "Not implemented yet" };
    }

    #endregion

    #region Search Functions

    private async Task<object> SearchCode(JsonElement parameters)
    {
        // Implementation for searching code across repositories
        return new { message = "Not implemented yet" };
    }

    private async Task<object> SearchWiki(JsonElement parameters)
    {
        // Implementation for searching wiki content
        return new { message = "Not implemented yet" };
    }

    private async Task<object> SearchWorkItems(JsonElement parameters)
    {
        // Implementation for searching work items
        return new { message = "Not implemented yet" };
    }

    #endregion

    #region Pull Request Functions

    private async Task<object> CreatePullRequest(JsonElement parameters)
    {
        // Implementation for creating a pull request
        return new { message = "Not implemented yet" };
    }

    private async Task<object> ListPullRequests(JsonElement parameters)
    {
        // Implementation for listing pull requests
        return new { message = "Not implemented yet" };
    }

    private async Task<object> GetPullRequestComments(JsonElement parameters)
    {
        // Implementation for getting pull request comments
        return new { message = "Not implemented yet" };
    }

    private async Task<object> AddPullRequestComment(JsonElement parameters)
    {
        // Implementation for adding a comment to a pull request
        return new { message = "Not implemented yet" };
    }

    private async Task<object> UpdatePullRequest(JsonElement parameters)
    {
        // Implementation for updating a pull request
        return new { message = "Not implemented yet" };
    }

    #endregion

    #region Pipeline Functions

    private async Task<object> ListPipelines(JsonElement parameters)
    {
        // Implementation for listing pipelines
        return new { message = "Not implemented yet" };
    }

    private async Task<object> GetPipeline(JsonElement parameters)
    {
        // Implementation for getting pipeline details
        return new { message = "Not implemented yet" };
    }

    private async Task<object> TriggerPipeline(JsonElement parameters)
    {
        // Implementation for triggering a pipeline run
        return new { message = "Not implemented yet" };
    }

    #endregion

    #region Wiki Functions

    private async Task<object> GetWikis(JsonElement parameters)
    {
        // Implementation for getting wikis
        return new { message = "Not implemented yet" };
    }

    private async Task<object> GetWikiPage(JsonElement parameters)
    {
        // Implementation for getting wiki page content
        return new { message = "Not implemented yet" };
    }

    private async Task<object> CreateWiki(JsonElement parameters)
    {
        // Implementation for creating a wiki
        return new { message = "Not implemented yet" };
    }

    private async Task<object> UpdateWikiPage(JsonElement parameters)
    {
        // Implementation for updating wiki page content
        return new { message = "Not implemented yet" };
    }

    #endregion
}