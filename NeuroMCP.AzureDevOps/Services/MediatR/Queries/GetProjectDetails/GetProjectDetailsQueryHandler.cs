using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.ProcessConfiguration.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProjectDetails;

/// <summary>
/// Handler for getting comprehensive project details
/// </summary>
public class GetProjectDetailsQueryHandler : AzureDevOpsRequestHandler<GetProjectDetailsQuery, GetProjectDetailsResult>
{
    public GetProjectDetailsQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetProjectDetailsQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get project details
    /// </summary>
    public override async Task<GetProjectDetailsResult> Handle(GetProjectDetailsQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        // Determine project ID
        var projectId = request.ProjectId ?? ConnectionProvider.GetDefaultProject();
        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Project ID is required");
        }

        try
        {
            // Get basic project details - this is always needed
            var project = await projectClient.GetProject(
                projectId,
                includeCapabilities: true,
                cancellationToken: cancellationToken);

            // Create the result with basic information
            var result = new GetProjectDetailsResult
            {
                Id = project.Id.ToString(),
                Name = project.Name,
                Description = project.Description,
                Url = project.Url,
                State = project.State.ToString(),
                Visibility = project.Visibility.ToString(),
                Capabilities = project.Capabilities ?? new Dictionary<string, object>()
            };

            if (project.DefaultTeam != null)
            {
                result.DefaultTeam = new TeamReference
                {
                    Id = project.DefaultTeam.Id.ToString(),
                    Name = project.DefaultTeam.Name
                };
            }

            // Get teams if requested
            if (request.IncludeTeams)
            {
                var teamClient = await connection.GetClientAsync<TeamHttpClient>();
                var teams = await teamClient.GetTeamsAsync(
                    project.Id,
                    expandIdentity: request.ExpandTeamIdentity,
                    cancellationToken: cancellationToken);

                result.Teams = teams.Select(t => new TeamReference
                {
                    Id = t.Id.ToString(),
                    Name = t.Name
                }).ToList();
            }

            // Get process information if requested
            if (request.IncludeProcess)
            {
                try
                {
                    var processClient = await connection.GetClientAsync<ProcessHttpClient>();
                    var processes = await processClient.GetProcessesAsync(cancellationToken: cancellationToken);

                    // Find the process ID from project capabilities
                    if (project.Capabilities != null &&
                        project.Capabilities.TryGetValue("processTemplate", out var processTemplateObj) &&
                        processTemplateObj is Dictionary<string, object> processTemplate &&
                        processTemplate.TryGetValue("templateTypeId", out var processIdObj) &&
                        processIdObj is string processIdStr)
                    {
                        var process = processes.FirstOrDefault(p => p.Id.ToString() == processIdStr);
                        if (process != null)
                        {
                            result.Process = new ProcessReference
                            {
                                Name = process.Name,
                                Version = process.Description // Process doesn't have a version field directly
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error getting process information for project {ProjectId}", projectId);
                    // Continue execution, this is not critical
                }
            }

            // Get work item types if requested
            if (request.IncludeWorkItemTypes)
            {
                try
                {
                    var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();
                    var workItemTypes = await witClient.GetWorkItemTypesAsync(
                        projectId,
                        cancellationToken: cancellationToken);

                    var mappedWorkItemTypes = new List<WorkItemTypeReference>();

                    foreach (var wit in workItemTypes)
                    {
                        var witRef = new WorkItemTypeReference
                        {
                            Name = wit.Name,
                            ReferenceName = wit.ReferenceName,
                            Description = wit.Description,
                            Color = wit.Color,
                            Icon = wit.Icon?.Url ?? string.Empty,
                            IsDisabled = wit.IsDisabled
                        };

                        if (request.IncludeFields)
                        {
                            // Get fields for the work item type if requested
                            var fields = await witClient.GetWorkItemTypeFieldsAsync(
                                projectId,
                                wit.Name,
                                cancellationToken: cancellationToken);

                            // This information could be added to the result if needed
                            // Currently we don't store the fields in the result
                        }

                        mappedWorkItemTypes.Add(witRef);
                    }

                    result.WorkItemTypes = mappedWorkItemTypes;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error getting work item types for project {ProjectId}", projectId);
                    // Continue execution, this is not critical
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting project details for {ProjectId}", projectId);
            throw;
        }
    }
}