using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.MediatR;

/// <summary>
/// Base class for all Azure DevOps request handlers
/// </summary>
public abstract class AzureDevOpsRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : AzureDevOpsRequest<TResponse>
{
    protected readonly IAzureDevOpsConnectionProvider ConnectionProvider;
    protected readonly ILogger Logger;

    protected AzureDevOpsRequestHandler(IAzureDevOpsConnectionProvider connectionProvider, ILogger logger)
    {
        ConnectionProvider = connectionProvider;
        Logger = logger;
    }

    /// <summary>
    /// Handles the request
    /// </summary>
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a connection to Azure DevOps
    /// </summary>
    protected async Task<VssConnection> GetConnectionAsync(string? organizationId = null)
    {
        return await ConnectionProvider.GetConnectionAsync(organizationId);
    }

    /// <summary>
    /// Gets the default or requested project ID
    /// </summary>
    protected string? GetProjectId(TRequest request)
    {
        return request.ProjectId ?? ConnectionProvider.GetDefaultProject();
    }
}