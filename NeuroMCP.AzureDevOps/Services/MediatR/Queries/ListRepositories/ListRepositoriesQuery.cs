using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListRepositories;

/// <summary>
/// Query to list repositories in a project
/// </summary>
public class ListRepositoriesQuery : AzureDevOpsRequest<IEnumerable<GitRepository>>
{
    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
}