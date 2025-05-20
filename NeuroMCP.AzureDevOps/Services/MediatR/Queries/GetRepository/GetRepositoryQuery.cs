using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetRepository;

/// <summary>
/// Query to get repository details
/// </summary>
public class GetRepositoryQuery : AzureDevOpsRequest<GitRepository>
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
}