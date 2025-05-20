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
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Models;

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

                connection = new VssConnection(new Uri(orgUrl), new VssAzureADCredential(credential));
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
    /// Get current authenticated user details
    /// </summary>
    public async Task<Microsoft.VisualStudio.Services.Identity.Identity> GetCurrentUserAsync()
    {
        var connection = await GetConnectionAsync();
        var identityClient = connection.GetClient<GraphHttpClient>();

        var user = await identityClient.GetUserAsync();
        return user;
    }

    /// <summary>
    /// List all accessible organizations
    /// </summary>
    public async Task<IEnumerable<AccountModel>> ListOrganizationsAsync()
    {
        // For organizations, we need to connect to the Azure DevOps accounts service
        var connection = new VssConnection(new Uri("https://app.vssps.visualstudio.com"),
            new VssBasicCredential(string.Empty, _patToken));

        var accountClient = await connection.GetClientAsync<AccountHttpClient>();
        var accounts = await accountClient.GetAccountsByMemberAsync(connection.AuthorizedIdentity.Id);

        return accounts;
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
                Operation = Operation.Add,
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
                Operation = Operation.Add,
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
        var client = await GetConnectionAsync(organizationId);
        var identity = await client.GetConnectedAsync();

        return new AccountModel
        {
            Id = identity.Id.ToString(),
            DisplayName = identity.DisplayName,
            Email = identity.Properties.ContainsKey("Account") ? identity.Properties["Account"].ToString() : null
        };
    }
}