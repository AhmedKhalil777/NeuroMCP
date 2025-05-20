using NeuroMCP.AzureDevOps.Services.MediatR.Models.Queries;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchCode;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWiki;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWorkItems;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Models.Mapping;

/// <summary>
/// Extension methods for mapping between search models and queries
/// </summary>
public static class SearchMappingExtensions
{
    /// <summary>
    /// Maps a SearchCodeModel to a SearchCodeQuery
    /// </summary>
    public static SearchCodeQuery ToQuery(this SearchCodeModel model)
    {
        return new SearchCodeQuery
        {
            SearchText = model.SearchText,
            ProjectId = model.ProjectId,
            Filters = model.Filters,
            Skip = model.Skip,
            Top = model.Top,
            OrganizationId = model.OrganizationId
        };
    }

    /// <summary>
    /// Maps a SearchWikiModel to a SearchWikiQuery
    /// </summary>
    public static SearchWikiQuery ToQuery(this SearchWikiModel model)
    {
        return new SearchWikiQuery
        {
            SearchText = model.SearchText,
            ProjectId = model.ProjectId,
            Filters = model.Filters,
            Skip = model.Skip,
            Top = model.Top,
            IncludeFacets = model.IncludeFacets,
            OrganizationId = model.OrganizationId
        };
    }

    /// <summary>
    /// Maps a SearchWorkItemsModel to a SearchWorkItemsQuery
    /// </summary>
    public static SearchWorkItemsQuery ToQuery(this SearchWorkItemsModel model)
    {
        return new SearchWorkItemsQuery
        {
            SearchText = model.SearchText,
            ProjectId = model.ProjectId,
            Filters = model.Filters,
            Skip = model.Skip,
            Top = model.Top,
            IncludeFacets = model.IncludeFacets,
            OrderBy = model.OrderBy?.Select(o => new SearchWorkItems.SortOption
            {
                Field = o.Field,
                SortOrder = o.SortOrder
            }).ToList(),
            OrganizationId = model.OrganizationId
        };
    }
}