using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetRepository;

/// <summary>
/// Handler for getting repository details
/// </summary>
public class GetRepositoryQueryHandler : AzureDevOpsRequestHandler<GetRepositoryQuery, GitRepository>
{
    public GetRepositoryQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetRepositoryQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get repository details
    /// </summary>
    public override async Task<GitRepository> Handle(GetRepositoryQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        var repository = await gitClient.GetRepositoryAsync(
            request.RepositoryId,
            request.ProjectId,
            cancellationToken: cancellationToken);

        return repository;
    }
}