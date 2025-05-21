using Microsoft.TeamFoundation.Core.WebApi;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProject;

/// <summary>
/// Query to get project details
/// </summary>
public class GetProjectQuery : AzureDevOpsRequest<TeamProject>
{
    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
}