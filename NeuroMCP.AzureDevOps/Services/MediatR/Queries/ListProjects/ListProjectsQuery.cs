using Microsoft.TeamFoundation.Core.WebApi;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListProjects;

/// <summary>
/// Query to list projects in an organization
/// </summary>
public class ListProjectsQuery : AzureDevOpsRequest<IEnumerable<TeamProjectReference>>
{
    // OrganizationId is already defined in the base class
}