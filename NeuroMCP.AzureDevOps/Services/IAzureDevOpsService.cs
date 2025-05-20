using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services;

/// <summary>
/// Interface for Azure DevOps operations
/// </summary>
public interface IAzureDevOpsService
{
    /// <summary>
    /// Get VssConnection to Azure DevOps
    /// </summary>
    Task<VssConnection> GetConnectionAsync(string? organizationUrl = null);

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
    /// Get comprehensive details of a project including teams, process, etc.
    /// </summary>
    Task<object> GetProjectDetailsAsync(string projectId, bool includeTeams = false, bool includeProcess = false,
        bool includeWorkItemTypes = false, string? organizationId = null);

    /// <summary>
    /// List repositories in a project
    /// </summary>
    Task<IEnumerable<GitRepository>> ListRepositoriesAsync(string projectId, string? organizationId = null);

    /// <summary>
    /// Get repository details
    /// </summary>
    Task<GitRepository> GetRepositoryAsync(string repositoryId, string projectId, string? organizationId = null);

    /// <summary>
    /// Get detailed repository information including refs and statistics
    /// </summary>
    Task<object> GetRepositoryDetailsAsync(string repositoryId, string projectId, bool includeRefs = false,
        bool includeStatistics = false, string? branchName = null, string? organizationId = null);

    /// <summary>
    /// Get content of a file or folder from a repository
    /// </summary>
    Task<GitItem> GetFileContentAsync(string repositoryId, string path, string projectId, string? organizationId = null);

    /// <summary>
    /// Get a hierarchical tree of files and directories from multiple repositories
    /// </summary>
    Task<object> GetAllRepositoriesTreeAsync(string projectId, int depth = 0, string? pattern = null,
        string? repositoryPattern = null, string? organizationId = null);

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

    /// <summary>
    /// Add or remove links between work items
    /// </summary>
    Task<WorkItem> ManageWorkItemLinkAsync(int sourceWorkItemId, int targetWorkItemId, string relationType,
        string operation, string? comment = null, string? organizationId = null);

    /// <summary>
    /// Search for code across repositories
    /// </summary>
    Task<object> SearchCodeAsync(string searchText, string? projectId = null, Dictionary<string, List<string>>? filters = null,
        int skip = 0, int top = 100, string? organizationId = null);

    /// <summary>
    /// Search for content in wikis
    /// </summary>
    Task<object> SearchWikiAsync(string searchText, string? projectId = null, Dictionary<string, List<string>>? filters = null,
        int skip = 0, int top = 100, string? organizationId = null);

    /// <summary>
    /// Search for work items
    /// </summary>
    Task<object> SearchWorkItemsAsync(string searchText, string? projectId = null, Dictionary<string, List<string>>? filters = null,
        int skip = 0, int top = 100, string? organizationId = null);

    /// <summary>
    /// Create a new pull request
    /// </summary>
    Task<object> CreatePullRequestAsync(string repositoryId, string sourceRefName, string targetRefName, string title,
        string? description = null, bool isDraft = false, IEnumerable<string>? reviewers = null,
        IEnumerable<int>? workItemIds = null, string? projectId = null, string? organizationId = null);

    /// <summary>
    /// List pull requests in a repository
    /// </summary>
    Task<IEnumerable<object>> ListPullRequestsAsync(string repositoryId, string? status = null, string? creatorId = null,
        string? reviewerId = null, string? sourceRefName = null, string? targetRefName = null,
        int skip = 0, int top = 10, string? projectId = null, string? organizationId = null);

    /// <summary>
    /// Get comments from a pull request
    /// </summary>
    Task<object> GetPullRequestCommentsAsync(string repositoryId, int pullRequestId, int? threadId = null,
        bool includeDeleted = false, int? top = null, string? projectId = null, string? organizationId = null);

    /// <summary>
    /// Add a comment to a pull request
    /// </summary>
    Task<object> AddPullRequestCommentAsync(string repositoryId, int pullRequestId, string content,
        int? threadId = null, int? parentCommentId = null, string? filePath = null, int? lineNumber = null,
        string? status = null, string? projectId = null, string? organizationId = null);

    /// <summary>
    /// Update an existing pull request
    /// </summary>
    Task<object> UpdatePullRequestAsync(string repositoryId, int pullRequestId, string? title = null,
        string? description = null, string? status = null, bool? isDraft = null,
        IEnumerable<string>? addReviewers = null, IEnumerable<string>? removeReviewers = null,
        IEnumerable<int>? addWorkItemIds = null, IEnumerable<int>? removeWorkItemIds = null,
        string? projectId = null, string? organizationId = null);

    /// <summary>
    /// Gets the authenticated user's details
    /// </summary>
    Task<AccountModel> GetMeAsync(string? organizationId = null);
}