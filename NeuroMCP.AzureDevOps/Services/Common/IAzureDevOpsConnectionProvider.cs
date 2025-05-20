using Microsoft.VisualStudio.Services.WebApi;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.Common;

/// <summary>
/// Interface for Azure DevOps connection provider
/// </summary>
public interface IAzureDevOpsConnectionProvider
{
    /// <summary>
    /// Get a VssConnection to Azure DevOps
    /// </summary>
    Task<VssConnection> GetConnectionAsync(string? organizationUrl = null);

    /// <summary>
    /// Gets the default project name if configured
    /// </summary>
    string? GetDefaultProject();

    /// <summary>
    /// Find an identity reference by email or display name
    /// </summary>
    Task<IdentityRef?> FindIdentityRefAsync(VssConnection connection, string identifier);
}