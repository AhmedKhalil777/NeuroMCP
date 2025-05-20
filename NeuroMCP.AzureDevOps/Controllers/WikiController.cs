using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWiki;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WikiController : ControllerBase
{
    private readonly IMediator _mediator;

    public WikiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search wiki content
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<SearchWikiResult>> SearchWikiAsync(
        [FromQuery] string searchText,
        [FromQuery] string? projectId = null,
        [FromQuery] int skip = 0,
        [FromQuery] int top = 100,
        [FromQuery] string? organizationId = null)
    {
        var query = new SearchWikiQuery
        {
            SearchText = searchText,
            ProjectId = projectId,
            Skip = skip,
            Top = top,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
} 