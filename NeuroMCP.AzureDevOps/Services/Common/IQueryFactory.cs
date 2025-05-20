using NeuroMCP.AzureDevOps.Services.Queries;

namespace NeuroMCP.AzureDevOps.Services.Common;

/// <summary>
/// Factory interface for creating query objects
/// </summary>
public interface IQueryFactory
{
    /// <summary>
    /// Gets the current authenticated user
    /// </summary>
    GetMeQuery GetMe(string? organizationId = null);

    /// <summary>
    /// Lists all accessible organizations
    /// </summary>
    ListOrganizationsQuery ListOrganizations();

    /// <summary>
    /// Lists all projects in an organization
    /// </summary>
    ListProjectsQuery ListProjects(string? organizationId = null);

    /// <summary>
    /// Gets a specific project
    /// </summary>
    GetProjectQuery GetProject(string projectId, string? organizationId = null);

    /// <summary>
    /// Gets detailed information about a project
    /// </summary>
    GetProjectDetailsQuery GetProjectDetails(
        string projectId,
        bool includeTeams = false,
        bool includeProcess = false,
        bool includeWorkItemTypes = false,
        string? organizationId = null);

    /// <summary>
    /// Lists repositories in a project
    /// </summary>
    ListRepositoriesQuery ListRepositories(string projectId, string? organizationId = null);

    /// <summary>
    /// Gets a specific repository
    /// </summary>
    GetRepositoryQuery GetRepository(string repositoryId, string projectId, string? organizationId = null);

    /// <summary>
    /// Gets content of a file or folder from a repository
    /// </summary>
    GetFileContentQuery GetFileContent(
        string repositoryId,
        string path,
        string projectId,
        string? organizationId = null);

    /// <summary>
    /// Lists work items in a project based on a query
    /// </summary>
    ListWorkItemsQuery ListWorkItems(
        string query,
        string projectId,
        string? organizationId = null);

    /// <summary>
    /// Gets a specific work item
    /// </summary>
    GetWorkItemQuery GetWorkItem(int workItemId, string? organizationId = null);

    /// <summary>
    /// Gets detailed information about a repository
    /// </summary>
    GetRepositoryDetailsQuery GetRepositoryDetails(
        string repositoryId,
        string projectId,
        bool includeRefs = false,
        bool includeStatistics = false,
        string? branchName = null,
        string? organizationId = null);

    /// <summary>
    /// Gets a hierarchical tree of files and directories from multiple repositories
    /// </summary>
    GetAllRepositoriesTreeQuery GetAllRepositoriesTree(
        string projectId,
        int depth = 0,
        string? pattern = null,
        string? repositoryPattern = null,
        string? organizationId = null);

    /// <summary>
    /// Searches for code across repositories
    /// </summary>
    SearchCodeQuery SearchCode(
        string searchText,
        string? projectId = null,
        Dictionary<string, List<string>>? filters = null,
        int skip = 0,
        int top = 100,
        string? organizationId = null);

    /// <summary>
    /// Searches for content in wikis
    /// </summary>
    SearchWikiQuery SearchWiki(
        string searchText,
        string? projectId = null,
        Dictionary<string, List<string>>? filters = null,
        int skip = 0,
        int top = 100,
        string? organizationId = null);

    /// <summary>
    /// Searches for work items
    /// </summary>
    SearchWorkItemsQuery SearchWorkItems(
        string searchText,
        string? projectId = null,
        Dictionary<string, List<string>>? filters = null,
        int skip = 0,
        int top = 100,
        string? organizationId = null);

    /// <summary>
    /// Lists pull requests in a repository
    /// </summary>
    ListPullRequestsQuery ListPullRequests(
        string repositoryId,
        string? status = null,
        string? creatorId = null,
        string? reviewerId = null,
        string? sourceRefName = null,
        string? targetRefName = null,
        int skip = 0,
        int top = 10,
        string? projectId = null,
        string? organizationId = null);

    /// <summary>
    /// Gets comments from a pull request
    /// </summary>
    GetPullRequestCommentsQuery GetPullRequestComments(
        string repositoryId,
        int pullRequestId,
        int? threadId = null,
        bool includeDeleted = false,
        int? top = null,
        string? projectId = null,
        string? organizationId = null);
}