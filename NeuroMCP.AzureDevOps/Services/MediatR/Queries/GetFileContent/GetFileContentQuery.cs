using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetFileContent;

/// <summary>
/// Query to get content of a file or folder from a repository
/// </summary>
public class GetFileContentQuery : AzureDevOpsRequest<GitItem>
{
    /// <summary>
    /// The ID or name of the repository
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// Path to the file or folder
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
}