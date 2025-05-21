using Microsoft.VisualStudio.Services.WebApi;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetConnection;

/// <summary>
/// Query to get a connection to Azure DevOps
/// </summary>
public class GetConnectionQuery : AzureDevOpsRequest<VssConnection>
{
    /// <summary>
    /// The organization URL
    /// </summary>
    public string? OrganizationUrl { get; set; }
}