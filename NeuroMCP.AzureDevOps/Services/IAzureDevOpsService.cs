using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace NeuroMCP.AzureDevOps.Services;

/// <summary>
/// Interface for Azure DevOps operations
/// </summary>
public interface IAzureDevOpsService
{
    /// <summary>
    /// Get current authenticated user details
    /// </summary>
    Task<VssConnection> GetConnectionAsync(string? organizationUrl = null);

    /// <summary>
    /// Get current authenticated user details
    /// </summary>
    Task<Microsoft.VisualStudio.Services.Identity.Identity> GetCurrentUserAsync();

    /// <summary>
    /// List all accessible organizations
    /// </summary>
    Task<IEnumerable<AccountModel>> ListOrganizationsAsync();

    /// <summary>
    /// List all projects in an organization
    /// </summary>
    Task<IEnumerable<TeamProjectReference>> ListProjectsAsync(string? organizationId = null);

    /// <summary>
    /// Get project details
    /// </summary>
    Task<TeamProject> GetProjectAsync(string projectId, string? organizationId = null);

    /// <summary>
    /// List repositories in a project
    /// </summary>
    Task<IEnumerable<GitRepository>> ListRepositoriesAsync(string projectId, string? organizationId = null);

    /// <summary>
    /// Get repository details
    /// </summary>
    Task<GitRepository> GetRepositoryAsync(string repositoryId, string projectId, string? organizationId = null);

    /// <summary>
    /// Get content of a file or folder from a repository
    /// </summary>
    Task<GitItem> GetFileContentAsync(string repositoryId, string path, string projectId, string? organizationId = null);

    /// <summary>
    /// List work items based on a query
    /// </summary>
    Task<IEnumerable<WorkItem>> ListWorkItemsAsync(string query, string projectId, string? organizationId = null);

    /// <summary>
    /// Get work item details
    /// </summary>
    Task<WorkItem> GetWorkItemAsync(int workItemId, string? organizationId = null);

    /// <summary>
    /// Create a new work item
    /// </summary>
    Task<WorkItem> CreateWorkItemAsync(string workItemType, string title, string projectId, string? organizationId = null);

    /// <summary>
    /// Update an existing work item
    /// </summary>
    Task<WorkItem> UpdateWorkItemAsync(int workItemId, IDictionary<string, object> fields, string? organizationId = null);
}