using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ModelContextProtocol.Server;
using NeuroMCP.AzureDevOps.Services;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Controllers
{
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly ILogger<McpController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAzureDevOpsConnectionProvider _connectionProvider;

        public McpController(
            IServiceProvider serviceProvider,
            ILogger<McpController> logger,
            IAzureDevOpsConnectionProvider connectionProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _connectionProvider = connectionProvider;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessMcpRequest([FromBody] JsonDocument requestBody)
        {
            try
            {
                _logger.LogInformation("Received MCP request: {Request}", requestBody.RootElement.ToString());

                // Parse the request to extract the required information
                string id = "0";
                string method = "";
                JsonElement paramsElement = new JsonElement();

                if (requestBody.RootElement.TryGetProperty("id", out var idElement))
                {
                    id = idElement.ToString();
                }

                if (requestBody.RootElement.TryGetProperty("method", out var methodElement))
                {
                    method = methodElement.GetString() ?? "";
                }

                if (requestBody.RootElement.TryGetProperty("params", out var foundParams))
                {
                    paramsElement = foundParams;
                }

                // Build the response based on the method
                if (string.IsNullOrEmpty(method))
                {
                    return BadRequest(new
                    {
                        id,
                        error = new
                        {
                            code = -32600,
                            message = "Invalid Request: no method specified"
                        },
                        jsonrpc = "2.0"
                    });
                }

                // Process method based on the name
                switch (method)
                {
                    case "initialize":
                        return HandleInitialize(id, paramsElement);

                    case "getTools":
                        return Ok(new
                        {
                            id,
                            result = new
                            {
                                tools = GetAvailableTools()
                            },
                            jsonrpc = "2.0"
                        });

                    // Add cases for other Azure DevOps methods here
                    // For example:
                    // case "azureDevOps_getProjects":
                    //     return await HandleGetProjects(id, paramsElement);

                    default:
                        return BadRequest(new
                        {
                            id,
                            error = new
                            {
                                code = -32601,
                                message = $"Method '{method}' not found"
                            },
                            jsonrpc = "2.0"
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MCP request");
                return StatusCode(500, new
                {
                    id = "0",
                    error = new
                    {
                        code = -32603,
                        message = "Internal error: " + ex.Message
                    },
                    jsonrpc = "2.0"
                });
            }
        }

        private object[] GetAvailableTools()
        {
            // Define the tools that this MCP endpoint offers
            return new object[]
            {
                // User tools
                new
                {
                    name = "azureDevOps_getMe",
                    description = "Get details of the authenticated user (id, displayName, email)",
                    parameters = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                
                // Organization tools
                new
                {
                    name = "azureDevOps_listOrganizations",
                    description = "List all Azure DevOps organizations accessible to the current authentication",
                    parameters = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                
                // Project tools
                new
                {
                    name = "azureDevOps_listProjects",
                    description = "List all projects in an organization",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            stateFilter = new {
                                type = "number",
                                description = "Filter on team project state (0: all, 1: well-formed, 2: creating, 3: deleting, 4: new)"
                            },
                            top = new {
                                type = "number",
                                description = "Maximum number of projects to return"
                            },
                            skip = new {
                                type = "number",
                                description = "Number of projects to skip"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getProject",
                    description = "Get details of a specific project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            }
                        }
                    }
                },
                
                // Repository tools
                new
                {
                    name = "azureDevOps_listRepositories",
                    description = "List repositories in a project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            includeLinks = new {
                                type = "boolean",
                                description = "Whether to include reference links"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getRepository",
                    description = "Get details of a specific repository",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getRepositoryDetails",
                    description = "Get detailed information about a repository including statistics and refs",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            },
                            includeStatistics = new {
                                type = "boolean",
                                description = "Whether to include branch statistics"
                            },
                            includeRefs = new {
                                type = "boolean",
                                description = "Whether to include repository refs"
                            },
                            refFilter = new {
                                type = "string",
                                description = "Optional filter for refs (e.g., \"heads/\" or \"tags/\")"
                            },
                            branchName = new {
                                type = "string",
                                description = "Name of specific branch to get statistics for (if includeStatistics is true)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getFileContent",
                    description = "Get content of a file or directory from a repository",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            },
                            path = new {
                                type = "string",
                                description = "Path to the file or folder"
                            },
                            version = new {
                                type = "string",
                                description = "The version (branch, tag, or commit) to get content from"
                            },
                            versionType = new {
                                type = "string",
                                description = "Type of version specified (branch, commit, or tag)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getRepositoriesTree",
                    description = "Displays a hierarchical tree view of files and directories across multiple repositories",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            depth = new {
                                type = "integer",
                                description = "Maximum depth to traverse within each repository (0 = unlimited)"
                            },
                            pattern = new {
                                type = "string",
                                description = "File pattern (wildcard characters allowed) to filter files by within each repository"
                            },
                            repositoryPattern = new {
                                type = "string",
                                description = "Repository name pattern (wildcard characters allowed) to filter which repositories are included"
                            }
                        }
                    }
                },
                
                // Work Item tools
                new
                {
                    name = "azureDevOps_listWorkItems",
                    description = "List work items in a project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            queryId = new {
                                type = "string",
                                description = "ID of a saved work item query"
                            },
                            wiql = new {
                                type = "string",
                                description = "Work Item Query Language (WIQL) query"
                            },
                            top = new {
                                type = "number",
                                description = "Maximum number of work items to return"
                            },
                            skip = new {
                                type = "number",
                                description = "Number of work items to skip"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getWorkItem",
                    description = "Get details of a specific work item",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            workItemId = new {
                                type = "number",
                                description = "The ID of the work item"
                            },
                            expand = new {
                                type = "number",
                                description = "The level of detail to include in the response"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_createWorkItem",
                    description = "Create a new work item",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            workItemType = new {
                                type = "string",
                                description = "The type of work item to create (e.g., \"Task\", \"Bug\", \"User Story\")"
                            },
                            title = new {
                                type = "string",
                                description = "The title of the work item"
                            },
                            description = new {
                                type = "string",
                                description = "Work item description in HTML format"
                            },
                            additionalFields = new {
                                type = "object",
                                description = "Additional fields to set on the work item"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_updateWorkItem",
                    description = "Update an existing work item",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            workItemId = new {
                                type = "number",
                                description = "The ID of the work item to update"
                            },
                            title = new {
                                type = "string",
                                description = "The updated title of the work item"
                            },
                            description = new {
                                type = "string",
                                description = "Work item description in HTML format"
                            },
                            state = new {
                                type = "string",
                                description = "The updated state of the work item"
                            },
                            areaPath = new {
                                type = "string",
                                description = "The updated area path for the work item"
                            },
                            iterationPath = new {
                                type = "string",
                                description = "The updated iteration path for the work item"
                            },
                            assignedTo = new {
                                type = "string",
                                description = "The email or name of the user to assign the work item to"
                            },
                            priority = new {
                                type = "number",
                                description = "The updated priority of the work item"
                            },
                            additionalFields = new {
                                type = "object",
                                description = "Additional fields to update on the work item"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_manageWorkItemLink",
                    description = "Add or remove links between work items",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            sourceWorkItemId = new {
                                type = "number",
                                description = "The ID of the source work item"
                            },
                            targetWorkItemId = new {
                                type = "number",
                                description = "The ID of the target work item"
                            },
                            operation = new {
                                type = "string",
                                description = "The operation to perform on the link"
                            },
                            relationType = new {
                                type = "string",
                                description = "The reference name of the relation type"
                            },
                            comment = new {
                                type = "string",
                                description = "Optional comment explaining the link"
                            },
                            newRelationType = new {
                                type = "string",
                                description = "The new relation type to use when updating a link"
                            }
                        }
                    }
                },
                
                // Pull Request tools
                new
                {
                    name = "azureDevOps_createPullRequest",
                    description = "Create a new pull request",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            },
                            title = new {
                                type = "string",
                                description = "The title of the pull request"
                            },
                            sourceRefName = new {
                                type = "string",
                                description = "The source branch name (e.g., refs/heads/feature-branch)"
                            },
                            targetRefName = new {
                                type = "string",
                                description = "The target branch name (e.g., refs/heads/main)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_listPullRequests",
                    description = "List pull requests in a repository",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            },
                            status = new {
                                type = "string",
                                description = "Filter by pull request status"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getPullRequestComments",
                    description = "Get comments from a specific pull request",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            },
                            pullRequestId = new {
                                type = "number",
                                description = "The ID of the pull request"
                            },
                            threadId = new {
                                type = "number",
                                description = "The ID of the specific thread to get comments from"
                            },
                            includeDeleted = new {
                                type = "boolean",
                                description = "Whether to include deleted comments"
                            },
                            top = new {
                                type = "number",
                                description = "Maximum number of threads/comments to return"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_addPullRequestComment",
                    description = "Add a comment to a pull request (reply to existing comments or create new threads)",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            },
                            pullRequestId = new {
                                type = "number",
                                description = "The ID of the pull request"
                            },
                            content = new {
                                type = "string",
                                description = "The content of the comment in markdown"
                            },
                            threadId = new {
                                type = "number",
                                description = "The ID of the thread to add the comment to"
                            },
                            parentCommentId = new {
                                type = "number",
                                description = "ID of the parent comment when replying to an existing comment"
                            },
                            status = new {
                                type = "string",
                                description = "The status to set for a new thread"
                            },
                            filePath = new {
                                type = "string",
                                description = "The path of the file to comment on (for new thread on file)"
                            },
                            lineNumber = new {
                                type = "number",
                                description = "The line number to comment on (for new thread on file)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_updatePullRequest",
                    description = "Update an existing pull request with new properties, link work items, and manage reviewers",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID or name of the repository"
                            },
                            pullRequestId = new {
                                type = "number",
                                description = "The ID of the pull request to update"
                            },
                            title = new {
                                type = "string",
                                description = "The updated title of the pull request"
                            },
                            description = new {
                                type = "string",
                                description = "The updated description of the pull request"
                            },
                            status = new {
                                type = "string",
                                description = "The updated status of the pull request"
                            },
                            isDraft = new {
                                type = "boolean",
                                description = "Whether the pull request should be marked as a draft (true) or unmarked (false)"
                            },
                            addReviewers = new {
                                type = "array",
                                description = "List of reviewer email addresses or IDs to add"
                            },
                            removeReviewers = new {
                                type = "array",
                                description = "List of reviewer email addresses or IDs to remove"
                            },
                            addWorkItemIds = new {
                                type = "array",
                                description = "List of work item IDs to link to the pull request"
                            },
                            removeWorkItemIds = new {
                                type = "array",
                                description = "List of work item IDs to unlink from the pull request"
                            },
                            additionalProperties = new {
                                type = "object",
                                description = "Additional properties to update on the pull request"
                            }
                        }
                    }
                },
                
                // Pipeline tools
                new
                {
                    name = "azureDevOps_listPipelines",
                    description = "List pipelines in a project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            orderBy = new {
                                type = "string",
                                description = "Order by field and direction (e.g., \"createdDate desc\")"
                            },
                            top = new {
                                type = "number",
                                description = "Maximum number of pipelines to return"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getPipeline",
                    description = "Get details of a specific pipeline",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            pipelineId = new {
                                type = "integer",
                                description = "The numeric ID of the pipeline to retrieve"
                            },
                            pipelineVersion = new {
                                type = "integer",
                                description = "The version of the pipeline to retrieve (latest if not specified)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_triggerPipeline",
                    description = "Trigger a pipeline run",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            pipelineId = new {
                                type = "integer",
                                description = "The numeric ID of the pipeline to trigger"
                            },
                            branch = new {
                                type = "string",
                                description = "The branch to run the pipeline on (e.g., \"main\", \"feature/my-branch\")"
                            },
                            variables = new {
                                type = "object",
                                description = "Variables to pass to the pipeline run"
                            },
                            templateParameters = new {
                                type = "object",
                                description = "Parameters for template-based pipelines"
                            },
                            stagesToSkip = new {
                                type = "array",
                                description = "Stages to skip in the pipeline run"
                            }
                        }
                    }
                },
                
                // Wiki tools
                new
                {
                    name = "azureDevOps_getWikis",
                    description = "Get details of wikis in a project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_getWikiPage",
                    description = "Get the content of a wiki page",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            wikiId = new {
                                type = "string",
                                description = "The ID or name of the wiki"
                            },
                            pagePath = new {
                                type = "string",
                                description = "The path of the page within the wiki"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_createWiki",
                    description = "Create a new wiki in the project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            name = new {
                                type = "string",
                                description = "The name of the new wiki"
                            },
                            type = new {
                                type = "string",
                                description = "Type of wiki to create (projectWiki or codeWiki)"
                            },
                            repositoryId = new {
                                type = "string",
                                description = "The ID of the repository to associate with the wiki (required for codeWiki)"
                            },
                            mappedPath = new {
                                type = "string",
                                description = "Folder path inside repository which is shown as Wiki (only for codeWiki)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_updateWikiPage",
                    description = "Update content of a wiki page",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project (Default: IPTS Taxes)"
                            },
                            wikiId = new {
                                type = "string",
                                description = "The ID or name of the wiki"
                            },
                            pagePath = new {
                                type = "string",
                                description = "Path of the wiki page to update"
                            },
                            content = new {
                                type = "string",
                                description = "The new content for the wiki page in markdown format"
                            },
                            comment = new {
                                type = "string",
                                description = "Optional comment for the update"
                            }
                        }
                    }
                },
                
                // Search tools
                new
                {
                    name = "azureDevOps_searchCode",
                    description = "Search for code across repositories in a project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project to search in (Default: IPTS Taxes)"
                            },
                            searchText = new {
                                type = "string",
                                description = "The text to search for"
                            },
                            filters = new {
                                type = "object",
                                description = "Optional filters to narrow search results"
                            },
                            includeContent = new {
                                type = "boolean",
                                description = "Whether to include full file content in results (default: true)"
                            },
                            includeSnippet = new {
                                type = "boolean",
                                description = "Whether to include code snippets in results (default: true)"
                            },
                            top = new {
                                type = "integer",
                                description = "Number of results to return (default: 100, max: 1000)"
                            },
                            skip = new {
                                type = "integer",
                                description = "Number of results to skip for pagination (default: 0)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_searchWiki",
                    description = "Search for content across wiki pages in a project",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project to search in (Default: IPTS Taxes)"
                            },
                            searchText = new {
                                type = "string",
                                description = "The text to search for in wikis"
                            },
                            filters = new {
                                type = "object",
                                description = "Optional filters to narrow search results"
                            },
                            includeFacets = new {
                                type = "boolean",
                                description = "Whether to include faceting in results (default: true)"
                            },
                            top = new {
                                type = "integer",
                                description = "Number of results to return (default: 100, max: 1000)"
                            },
                            skip = new {
                                type = "integer",
                                description = "Number of results to skip for pagination (default: 0)"
                            }
                        }
                    }
                },
                new
                {
                    name = "azureDevOps_searchWorkItems",
                    description = "Search for work items across projects in Azure DevOps",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            organizationId = new {
                                type = "string",
                                description = "The ID or name of the organization (Default: IPTS-Perspecta)"
                            },
                            projectId = new {
                                type = "string",
                                description = "The ID or name of the project to search in (Default: IPTS Taxes)"
                            },
                            searchText = new {
                                type = "string",
                                description = "The text to search for in work items"
                            },
                            filters = new {
                                type = "object",
                                description = "Optional filters to narrow search results"
                            },
                            includeFacets = new {
                                type = "boolean",
                                description = "Whether to include faceting in results (default: true)"
                            },
                            orderBy = new {
                                type = "array",
                                description = "Options for sorting search results"
                            },
                            top = new {
                                type = "integer",
                                description = "Number of results to return (default: 100, max: 1000)"
                            },
                            skip = new {
                                type = "integer",
                                description = "Number of results to skip for pagination (default: 0)"
                            }
                        }
                    }
                }
            };
        }

        private IActionResult HandleInitialize(string id, JsonElement paramsElement)
        {
            return Ok(new
            {
                id,
                result = new
                {
                    name = "NeuroMCP.AzureDevOps",
                    version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                    vendor = new
                    {
                        name = "NeuroMCP",
                        url = "https://github.com/AhmedKhalil777/NeuroMCP"
                    }
                },
                jsonrpc = "2.0"
            });
        }

        // Add handlers for other Azure DevOps methods
    }
}