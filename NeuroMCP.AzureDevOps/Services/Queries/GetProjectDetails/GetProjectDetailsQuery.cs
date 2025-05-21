namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProjectDetails;

/// <summary>
/// Query to get comprehensive project details
/// </summary>
public class GetProjectDetailsQuery : AzureDevOpsRequest<GetProjectDetailsResult>
{
    /// <summary>
    /// Whether to include associated teams in the project result
    /// </summary>
    public bool IncludeTeams { get; set; }

    /// <summary>
    /// Whether to include process information in the project result
    /// </summary>
    public bool IncludeProcess { get; set; }

    /// <summary>
    /// Whether to include work item types and their structure
    /// </summary>
    public bool IncludeWorkItemTypes { get; set; }

    /// <summary>
    /// Whether to expand identity information in the team objects
    /// </summary>
    public bool ExpandTeamIdentity { get; set; }

    /// <summary>
    /// Whether to include field information for work item types
    /// </summary>
    public bool IncludeFields { get; set; }
}