using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeuroMCP.AzureDevOps.Services.MediatR.Models.Mapping;
using NeuroMCP.AzureDevOps.Services.MediatR.Models.Queries;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchCode;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWiki;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWorkItems;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public async Task<ActionResult<SearchCodeResult>> SearchCodeAsync(
        [FromQuery] string searchText,
        [FromQuery] string? projectId = null,
        [FromQuery] int skip = 0,
        [FromQuery] int top = 100,
        [FromQuery] string? organizationId = null)
    {
        var model = new SearchCodeModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            Skip = skip,
            Top = top,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(model.ToQuery());
        return Ok(result);
    }

    /// <summary>
    /// Search for code with advanced filtering
    /// </summary>
    [HttpPost("code")]
    public async Task<ActionResult<SearchCodeResult>> SearchCodeWithFiltersAsync(
        [FromBody] SearchCodeModel model)
    {
        var result = await _mediator.Send(model.ToQuery());
        return Ok(result);
    }

    /// <summary>
    /// Search for content across wiki pages
    /// </summary>
    [HttpGet("wiki")]
    public async Task<ActionResult<SearchWikiResult>> SearchWikiAsync(
        [FromQuery] string searchText,
        [FromQuery] string? projectId = null,
        [FromQuery] int skip = 0,
        [FromQuery] int top = 100,
        [FromQuery] bool includeFacets = true,
        [FromQuery] string? organizationId = null)
    {
        var model = new SearchWikiModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            Skip = skip,
            Top = top,
            IncludeFacets = includeFacets,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(model.ToQuery());
        return Ok(result);
    }

    /// <summary>
    /// Search for wiki content with advanced filtering
    /// </summary>
    [HttpPost("wiki")]
    public async Task<ActionResult<SearchWikiResult>> SearchWikiWithFiltersAsync(
        [FromBody] SearchWikiModel model)
    {
        var result = await _mediator.Send(model.ToQuery());
        return Ok(result);
    }

    /// <summary>
    /// Search for work items
    /// </summary>
    [HttpGet("workitems")]
    public async Task<ActionResult<SearchWorkItemsResult>> SearchWorkItemsAsync(
        [FromQuery] string searchText,
        [FromQuery] string? projectId = null,
        [FromQuery] int skip = 0,
        [FromQuery] int top = 100,
        [FromQuery] bool includeFacets = true,
        [FromQuery] string? organizationId = null)
    {
        var model = new SearchWorkItemsModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            Skip = skip,
            Top = top,
            IncludeFacets = includeFacets,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(model.ToQuery());
        return Ok(result);
    }

    /// <summary>
    /// Search for work items with advanced filtering
    /// </summary>
    [HttpPost("workitems")]
    public async Task<ActionResult<SearchWorkItemsResult>> SearchWorkItemsWithFiltersAsync(
        [FromBody] SearchWorkItemsModel model)
    {
        var result = await _mediator.Send(model.ToQuery());
        return Ok(result);
    }
}