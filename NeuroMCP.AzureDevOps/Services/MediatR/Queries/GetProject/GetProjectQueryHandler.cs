using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProject;

/// <summary>
/// Handler for getting project details
/// </summary>
public class GetProjectQueryHandler : AzureDevOpsRequestHandler<GetProjectQuery, TeamProject>
{
    public GetProjectQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetProjectQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get project details
    /// </summary>
    public override async Task<TeamProject> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        var project = await projectClient.GetProject(
            request.ProjectId,
            includeCapabilities: true,
            includeHistory: true,
            cancellationToken: cancellationToken);

        return project;
    }
}