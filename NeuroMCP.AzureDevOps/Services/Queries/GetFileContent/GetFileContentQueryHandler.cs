using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetFileContent;

/// <summary>
/// Handler for getting content of a file or folder from a repository
/// </summary>
public class GetFileContentQueryHandler : AzureDevOpsRequestHandler<GetFileContentQuery, GitItem>
{
    public GetFileContentQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetFileContentQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get content of a file or folder
    /// </summary>
    public override async Task<GitItem> Handle(GetFileContentQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        var item = await gitClient.GetItemAsync(
            repositoryId: request.RepositoryId,
            path: request.Path,
            project: request.ProjectId,
            includeContent: true,
            recursionLevel: VersionControlRecursionType.Full,
            includeContentMetadata: true,
            latestProcessedChange: true,
            download: false,
            cancellationToken: cancellationToken);
            
        return item;
    }
} 