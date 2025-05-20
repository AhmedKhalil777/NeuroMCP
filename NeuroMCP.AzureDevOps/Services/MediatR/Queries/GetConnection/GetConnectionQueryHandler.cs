using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetConnection;

/// <summary>
/// Handler for getting a connection to Azure DevOps
/// </summary>
public class GetConnectionQueryHandler : AzureDevOpsRequestHandler<GetConnectionQuery, VssConnection>
{
    public GetConnectionQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetConnectionQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get a connection
    /// </summary>
    public override async Task<VssConnection> Handle(GetConnectionQuery request, CancellationToken cancellationToken)
    {
        return await ConnectionProvider.GetConnectionAsync(request.OrganizationUrl);
    }
}