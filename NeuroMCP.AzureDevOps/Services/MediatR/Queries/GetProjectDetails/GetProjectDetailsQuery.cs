namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProjectDetails;

/// <summary>
/// Query to get comprehensive project details including teams, process, etc.
/// </summary>
public class GetProjectDetailsQuery : AzureDevOpsRequest<GetProjectDetailsResult>
{
    /// <summary>
    /// The ID or name of the project
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include team information
    /// </summary>
    public bool IncludeTeams { get; set; } = false;

    /// <summary>
    /// Whether to include process information
    /// </summary>
    public bool IncludeProcess { get; set; } = false;

    /// <summary>
    /// Whether to include work item type information
    /// </summary>
    public bool IncludeWorkItemTypes { get; set; } = false;
}