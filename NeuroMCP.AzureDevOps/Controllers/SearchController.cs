using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeuroMCP.AzureDevOps.Services.Common.Models.Queries;
using NeuroMCP.AzureDevOps.Services.Queries.SearchWorkItems;

namespace NeuroMCP.AzureDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search for code across repositories
    /// </summary>
    [HttpGet("code")]
    public async Task<ActionResult> SearchCodeAsync(
        [FromQuery] string searchText,
        [FromQuery] string? projectId = null,
        [FromQuery] int? skip = null,
        [FromQuery] int? top = null,
        [FromQuery] string? organizationId = null)
    {
        // Temporarily comment out until we have the SearchCode query implemented
        /*
        var query = new SearchCodeQuery
        {
            Model = new SearchCodeModel
            {
                SearchText = searchText,
                Skip = skip,
                Top = top
            },
            ProjectId = projectId,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
        */
        return NotFound("Search code functionality is not implemented yet");
    }



    /// <summary>
    /// Search for work items
    /// </summary>
    [HttpGet("workitems")]
    public async Task<ActionResult<SearchWorkItemsResult>> SearchWorkItemsAsync(
        [FromQuery] string searchText,
        [FromQuery] string? projectId = null,
        [FromQuery] int? skip = null,
        [FromQuery] int? top = null,
        [FromQuery] bool? includeFacets = null,
        [FromQuery] string? organizationId = null)
    {
        var query = new SearchWorkItemsQuery
        {
            Model = new SearchWorkItemsModel
            {
                SearchText = searchText,
                Skip = skip,
                Top = top,
                IncludeFacets = includeFacets ?? false
            },
            ProjectId = projectId,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }


}