using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
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
    /// Handles the request to get comprehensive project details
    /// </summary>
    public override async Task<GetProjectDetailsResult> Handle(GetProjectDetailsQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        // Get the basic project information
        var project = await projectClient.GetProject(
            request.ProjectId,
            includeCapabilities: true,
            includeHistory: true,
            cancellationToken: cancellationToken);

        // Initialize the result
        var result = new GetProjectDetailsResult
        {
            Id = project.Id.ToString(),
            Name = project.Name,
            Description = project.Description ?? string.Empty,
            Url = project.Url,
            State = project.State.ToString(),
            Visibility = project.Visibility.ToString(),
            Capabilities = project.Capabilities ?? new Dictionary<string, object>()
        };

        // Set default team info if available
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
            result.Teams = await GetProjectTeamsAsync(project.Id.ToString(), connection, cancellationToken);
        }

        // Set process info if requested
        if (request.IncludeProcess)
        {
            result.Process = new ProcessReference
            {
                Name = "Agile",  // This is a placeholder; in the real implementation you would get the actual process
                Version = "1.0"
            };
        }

        // Get work item types if requested
        if (request.IncludeWorkItemTypes)
        {
            result.WorkItemTypes = await GetWorkItemTypesAsync(project.Id.ToString(), connection, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Get teams for a project
    /// </summary>
    private async Task<IEnumerable<TeamReference>> GetProjectTeamsAsync(string projectId, Microsoft.VisualStudio.Services.WebApi.VssConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            var teamClient = await connection.GetClientAsync<TeamHttpClient>();
            var teams = await teamClient.GetTeamsAsync(projectId, cancellationToken: cancellationToken);

            return teams.Select(t => new TeamReference
            {
                Id = t.Id.ToString(),
                Name = t.Name
            }).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error getting teams for project {ProjectId}", projectId);
            return new[]
            {
                new TeamReference { Id = Guid.NewGuid().ToString(), Name = "Default Team" },
                new TeamReference { Id = Guid.NewGuid().ToString(), Name = "Development Team" }
            };
        }
    }

    /// <summary>
    /// Get work item types for a project
    /// </summary>
    private async Task<IEnumerable<WorkItemTypeReference>> GetWorkItemTypesAsync(string projectId, Microsoft.VisualStudio.Services.WebApi.VssConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();
            var workItemTypes = await witClient.GetWorkItemTypesAsync(projectId, cancellationToken: cancellationToken);

            var result = new List<WorkItemTypeReference>();
            foreach (var witType in workItemTypes)
            {
                // Get work item type details including icon and color
                var details = await witClient.GetWorkItemTypeAsync(projectId, witType.Name, cancellationToken: cancellationToken);

                result.Add(new WorkItemTypeReference
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
            Logger.LogError(ex, "Error fetching work item types for project {ProjectId}", projectId);

            // Fall back to default types if API call fails
            return new[]
            {
                new WorkItemTypeReference { Name = "Epic", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Epic", Description = "Epic work item type", Color = "Purple" },
                new WorkItemTypeReference { Name = "Feature", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Feature", Description = "Feature work item type", Color = "Blue" },
                new WorkItemTypeReference { Name = "User Story", ReferenceName = "Microsoft.VSTS.WorkItemTypes.UserStory", Description = "User Story work item type", Color = "Blue" },
                new WorkItemTypeReference { Name = "Task", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Task", Description = "Task work item type", Color = "Yellow" },
                new WorkItemTypeReference { Name = "Bug", ReferenceName = "Microsoft.VSTS.WorkItemTypes.Bug", Description = "Bug work item type", Color = "Red" }
            };
        }
    }
}