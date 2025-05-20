using MediatR;

namespace NeuroMCP.AzureDevOps.Services.MediatR;

/// <summary>
/// Base class for all Azure DevOps requests
/// </summary>
public abstract class AzureDevOpsRequest<TResponse> : IRequest<TResponse>
{
    /// <summary>
    /// The organization ID or URL
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// The project ID or name
    /// </summary>
    public string? ProjectId { get; set; }
}