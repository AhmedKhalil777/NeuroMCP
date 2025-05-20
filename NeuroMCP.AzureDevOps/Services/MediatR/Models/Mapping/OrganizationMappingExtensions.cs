using NeuroMCP.AzureDevOps.Services.MediatR.Models.Queries;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetMe;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListOrganizations;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Models.Mapping;

/// <summary>
/// Extension methods for mapping between organization models and queries
/// </summary>
public static class OrganizationMappingExtensions
{
    /// <summary>
    /// Maps a GetMeModel to a GetMeQuery
    /// </summary>
    public static GetMeQuery ToQuery(this GetMeModel model)
    {
        return new GetMeQuery
        {
            OrganizationId = model.OrganizationId
        };
    }

    /// <summary>
    /// Maps a ListOrganizationsModel to a ListOrganizationsQuery
    /// </summary>
    public static ListOrganizationsQuery ToQuery(this ListOrganizationsModel model)
    {
        return new ListOrganizationsQuery();
    }
}