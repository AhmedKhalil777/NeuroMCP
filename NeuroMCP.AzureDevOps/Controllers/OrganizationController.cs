using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeuroMCP.AzureDevOps.Models;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetMe;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListOrganizations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get details of the authenticated user
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<AccountModel>> GetMeAsync(
        [FromQuery] string? organizationId = null)
    {
        var query = new GetMeQuery
        {
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// List all accessible organizations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountModel>>> ListOrganizationsAsync()
    {
        var query = new ListOrganizationsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}