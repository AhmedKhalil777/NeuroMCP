using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using NeuroMCP.AzureDevOps.Models;
using NeuroMCP.AzureDevOps.Services;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.CreatePullRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Controllers
{
    /// <summary>
    /// Azure DevOps Integration Controller for NeuroMCP
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AzureDevOpsController : ControllerBase, IMcpTool
    {
        private readonly IAzureDevOpsService _azureDevOpsService;
        private readonly ILogger<AzureDevOpsController> _logger;
        private readonly IMediator _mediator;

        // IMcpTool implementation
        public string Name => "Azure DevOps";
        public string Version => GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";
        public string Description => "Azure DevOps integration for ModelContextProtocol";

        /// <summary>
        /// Constructor for Azure DevOps controller
        /// </summary>
        public AzureDevOpsController(IAzureDevOpsService azureDevOpsService, ILogger<AzureDevOpsController> logger, IMediator mediator)
        {
            _azureDevOpsService = azureDevOpsService;
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var user = await _azureDevOpsService.GetMeAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("organizations")]
        public async Task<IActionResult> ListOrganizations()
        {
            try
            {
                var orgs = await _azureDevOpsService.ListOrganizationsAsync();
                return Ok(orgs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing organizations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("projects")]
        public async Task<IActionResult> ListProjects(string? organizationId = null)
        {
            try
            {
                var projects = await _azureDevOpsService.ListProjectsAsync(organizationId);
                return Ok(projects.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    state = p.State,
                    visibility = p.Visibility,
                    lastUpdateTime = p.LastUpdateTime
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing projects");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("projects/{projectId}")]
        public async Task<IActionResult> GetProject(string projectId, string? organizationId = null)
        {
            try
            {
                var project = await _azureDevOpsService.GetProjectAsync(projectId, organizationId);
                return Ok(new
                {
                    id = project.Id,
                    name = project.Name,
                    description = project.Description,
                    state = project.State,
                    visibility = project.Visibility,
                    url = project.Url,
                    defaultTeam = project.DefaultTeam != null ? new { id = project.DefaultTeam.Id, name = project.DefaultTeam.Name } : null,
                    capabilities = project.Capabilities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project details");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("projects/{projectId}/repositories")]
        public async Task<IActionResult> ListRepositories(string projectId, string? organizationId = null)
        {
            try
            {
                var repos = await _azureDevOpsService.ListRepositoriesAsync(projectId, organizationId);
                return Ok(repos.Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    url = r.Url,
                    remoteUrl = r.RemoteUrl,
                    defaultBranch = r.DefaultBranch,
                    size = r.Size,
                    isFork = r.IsFork,
                    project = new { id = r.ProjectReference.Id, name = r.ProjectReference.Name }
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing repositories");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("projects/{projectId}/repositories/{repositoryId}")]
        public async Task<IActionResult> GetRepository(string projectId, string repositoryId, string? organizationId = null)
        {
            try
            {
                var repo = await _azureDevOpsService.GetRepositoryAsync(repositoryId, projectId, organizationId);
                return Ok(new
                {
                    id = repo.Id,
                    name = repo.Name,
                    url = repo.Url,
                    remoteUrl = repo.RemoteUrl,
                    defaultBranch = repo.DefaultBranch,
                    size = repo.Size,
                    isFork = repo.IsFork,
                    project = new { id = repo.ProjectReference.Id, name = repo.ProjectReference.Name }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository details");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("projects/{projectId}/repositories/{repositoryId}/content")]
        public async Task<IActionResult> GetFileContent(string projectId, string repositoryId, string path, string? organizationId = null)
        {
            try
            {
                var item = await _azureDevOpsService.GetFileContentAsync(repositoryId, path, projectId, organizationId);
                return Ok(new
                {
                    path = item.Path,
                    isFolder = item.IsFolder,
                    isSymbolicLink = item.IsSymbolicLink,
                    content = item.Content,
                    size = item.ContentMetadata != null ? item.ContentMetadata.ToString() : null,
                    url = item.Url,
                    commitId = item.CommitId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file content");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("projects/{projectId}/work-items")]
        public async Task<IActionResult> ListWorkItems(string projectId, string? query = null, string? organizationId = null)
        {
            try
            {
                // Default WIQL query if none provided
                query ??= $"SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.TeamProject] = '{projectId}' ORDER BY [System.Id]";

                var workItems = await _azureDevOpsService.ListWorkItemsAsync(query, projectId, organizationId);
                return Ok(workItems.Select(wi => new
                {
                    id = wi.Id,
                    fields = wi.Fields.ToDictionary(f => f.Key, f => f.Value),
                    url = wi.Url
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing work items");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("work-items/{workItemId}")]
        public async Task<IActionResult> GetWorkItem(int workItemId, string? organizationId = null)
        {
            try
            {
                var workItem = await _azureDevOpsService.GetWorkItemAsync(workItemId, organizationId);
                return Ok(new
                {
                    id = workItem.Id,
                    fields = workItem.Fields.ToDictionary(f => f.Key, f => f.Value),
                    url = workItem.Url,
                    relations = workItem.Relations?.Select(r => new { rel = r.Rel, url = r.Url, attributes = r.Attributes })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work item");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("projects/{projectId}/work-items")]
        public async Task<IActionResult> CreateWorkItem(string projectId, string workItemType, [FromBody] CreateWorkItemRequest request, string? organizationId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest(new { error = "Title is required" });
                }

                var workItem = await _azureDevOpsService.CreateWorkItemAsync(workItemType, request.Title, projectId, organizationId);
                return Ok(new
                {
                    id = workItem.Id,
                    fields = workItem.Fields.ToDictionary(f => f.Key, f => f.Value),
                    url = workItem.Url
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work item");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPatch("work-items/{workItemId}")]
        public async Task<IActionResult> UpdateWorkItem(int workItemId, [FromBody] UpdateWorkItemRequest request, string? organizationId = null)
        {
            try
            {
                var fields = new Dictionary<string, object>();

                // Add title if provided
                if (!string.IsNullOrEmpty(request.Title))
                {
                    fields["System.Title"] = request.Title;
                }

                // Add state if provided
                if (!string.IsNullOrEmpty(request.State))
                {
                    fields["System.State"] = request.State;
                }

                // Add assignedTo if provided
                if (!string.IsNullOrEmpty(request.AssignedTo))
                {
                    fields["System.AssignedTo"] = request.AssignedTo;
                }

                // Add description if provided
                if (!string.IsNullOrEmpty(request.Description))
                {
                    fields["System.Description"] = request.Description;
                }

                // Add additional fields if provided
                if (request.AdditionalFields != null)
                {
                    foreach (var field in request.AdditionalFields)
                    {
                        fields[field.Key] = field.Value;
                    }
                }

                var workItem = await _azureDevOpsService.UpdateWorkItemAsync(workItemId, fields, organizationId);
                return Ok(new
                {
                    id = workItem.Id,
                    fields = workItem.Fields.ToDictionary(f => f.Key, f => f.Value),
                    url = workItem.Url
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work item");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("projects/{projectId}/repositories/{repositoryId}/pull-requests")]
        public async Task<IActionResult> CreatePullRequest(
            string projectId,
            string repositoryId,
            [FromBody] CreatePullRequestRequest request,
            string? organizationId = null)
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(request.SourceRefName))
                    return BadRequest(new { error = "Source branch is required" });

                if (string.IsNullOrEmpty(request.TargetRefName))
                    return BadRequest(new { error = "Target branch is required" });

                if (string.IsNullOrEmpty(request.Title))
                    return BadRequest(new { error = "Title is required" });

                // Use MediatR to execute the query
                var query = new CreatePullRequestQuery
                {
                    ProjectId = projectId,
                    RepositoryId = repositoryId,
                    OrganizationId = organizationId,
                    SourceRefName = request.SourceRefName,
                    TargetRefName = request.TargetRefName,
                    Title = request.Title,
                    Description = request.Description,
                    IsDraft = request.IsDraft,
                    Reviewers = request.Reviewers,
                    WorkItemIds = request.WorkItemIds
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pull request");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Request models for API endpoints

        public class CreateWorkItemRequest
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? AssignedTo { get; set; }
            public string? State { get; set; }
            public Dictionary<string, string>? AdditionalFields { get; set; }
        }

        public class UpdateWorkItemRequest
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? AssignedTo { get; set; }
            public string? State { get; set; }
            public Dictionary<string, string>? AdditionalFields { get; set; }
        }

        public class CreatePullRequestRequest
        {
            public string SourceRefName { get; set; } = string.Empty;
            public string TargetRefName { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsDraft { get; set; }
            public IEnumerable<string>? Reviewers { get; set; }
            public IEnumerable<int>? WorkItemIds { get; set; }
        }
    }
}