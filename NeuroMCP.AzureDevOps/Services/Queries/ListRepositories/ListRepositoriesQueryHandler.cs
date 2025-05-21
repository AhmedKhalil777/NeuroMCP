using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListRepositories;

/// <summary>
/// Handler for listing repositories in a project
/// </summary>
public class ListRepositoriesQueryHandler : AzureDevOpsRequestHandler<ListRepositoriesQuery, IEnumerable<GitRepository>>
{
    public ListRepositoriesQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<ListRepositoriesQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to list repositories
    /// </summary>
    public override async Task<IEnumerable<GitRepository>> Handle(ListRepositoriesQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        var repositories = await gitClient.GetRepositoriesAsync(
            request.ProjectId,
            includeLinks: true,
            cancellationToken: cancellationToken);

        return repositories;
    }
}