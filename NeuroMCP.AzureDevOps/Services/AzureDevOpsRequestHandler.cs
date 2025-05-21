using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services;

/// <summary>
/// Base class for Azure DevOps request handlers
/// </summary>
public abstract class AzureDevOpsRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : AzureDevOpsRequest<TResponse>
{
    /// <summary>
    /// Connection provider for Azure DevOps
    /// </summary>
    protected readonly IAzureDevOpsConnectionProvider ConnectionProvider;

    /// <summary>
    /// Logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Creates a new instance of the Azure DevOps request handler
    /// </summary>
    protected AzureDevOpsRequestHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger logger)
    {
        ConnectionProvider = connectionProvider;
        Logger = logger;
    }

    /// <summary>
    /// Gets a connection to Azure DevOps
    /// </summary>
    protected async Task<VssConnection> GetConnectionAsync(string? organizationId = null)
    {
        return await ConnectionProvider.GetConnectionAsync(organizationId);
    }

    /// <summary>
    /// Handles the request
    /// </summary>
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the default or requested project ID
    /// </summary>
    protected string? GetProjectId(TRequest request)
    {
        return request.ProjectId ?? ConnectionProvider.GetDefaultProject();
    }
}