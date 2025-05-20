namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetAllRepositoriesTree;

/// <summary>
/// Query to get a hierarchical tree of files and directories from multiple repositories
/// </summary>
public class GetAllRepositoriesTreeQuery : AzureDevOpsRequest<GetAllRepositoriesTreeResult>
{
    /// <summary>
    /// Maximum depth to traverse within each repository (0 = unlimited)
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// File pattern (wildcard characters allowed) to filter files by within each repository
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Repository name pattern (wildcard characters allowed) to filter which repositories are included
    /// </summary>
    public string? RepositoryPattern { get; set; }
}