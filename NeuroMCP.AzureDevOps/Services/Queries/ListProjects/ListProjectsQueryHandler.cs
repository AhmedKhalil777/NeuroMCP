using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListProjects;

/// <summary>
/// Handler for listing projects in an organization
/// </summary>
public class ListProjectsQueryHandler : AzureDevOpsRequestHandler<ListProjectsQuery, IEnumerable<TeamProjectReference>>
{
    public ListProjectsQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<ListProjectsQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to list projects
    /// </summary>
    public override async Task<IEnumerable<TeamProjectReference>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        var projects = await projectClient.GetProjects();
        return projects;
    }
}
