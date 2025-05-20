using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using NeuroMCP.AzureDevOps.Services.MediatR.Commands.CreateWorkItem;
using NeuroMCP.AzureDevOps.Services.MediatR.Commands.ManageWorkItemLink;
using NeuroMCP.AzureDevOps.Services.MediatR.Commands.UpdateWorkItem;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetWorkItem;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.SearchWorkItems;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search for work items
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<SearchWorkItemsResult>> SearchWorkItemsAsync(
        [FromQuery] string searchText,
        [FromQuery] int skip = 0,
        [FromQuery] int top = 100,
        [FromQuery] string? organizationId = null)
    {
        var query = new SearchWorkItemsQuery
        {
            SearchText = searchText,
            Skip = skip,
            Top = top,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get work item details
    /// </summary>
    [HttpGet("{workItemId}")]
    public async Task<ActionResult<WorkItem>> GetWorkItemAsync(
        [FromRoute] int workItemId,
        [FromQuery] string? organizationId = null)
    {
        var query = new GetWorkItemQuery
        {
            WorkItemId = workItemId,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new work item
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkItem>> CreateWorkItemAsync(
        [FromBody] CreateWorkItemCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetWorkItemAsync), new { workItemId = result.Id }, result);
    }

    /// <summary>    /// Update an existing work item    /// </summary>    [HttpPut("{workItemId}")]    public async Task<ActionResult<WorkItem>> UpdateWorkItemAsync(        [FromRoute] int workItemId,        [FromBody] Dictionary<string, object> fields,        [FromQuery] string? organizationId = null)    {        var command = new UpdateWorkItemCommand        {            WorkItemId = workItemId,            Fields = fields,            OrganizationId = organizationId        };        var result = await _mediator.Send(command);        return Ok(result);    }        /// <summary>    /// Manage links between work items    /// </summary>    [HttpPost("{sourceWorkItemId}/links")]    public async Task<ActionResult<WorkItem>> ManageWorkItemLinkAsync(        [FromRoute] int sourceWorkItemId,        [FromQuery] int targetWorkItemId,        [FromQuery] string relationType,        [FromQuery] string operation,        [FromQuery] string? comment = null,        [FromQuery] string? organizationId = null)    {        var command = new ManageWorkItemLinkCommand        {            SourceWorkItemId = sourceWorkItemId,            TargetWorkItemId = targetWorkItemId,            RelationType = relationType,            Operation = operation,            Comment = comment,            OrganizationId = organizationId        };        var result = await _mediator.Send(command);        return Ok(result);    }
}

/// <summary>
/// Request model for creating a work item
/// </summary>
public class CreateWorkItemRequest
{
    /// <summary>
    /// The title of the work item
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description of the work item in HTML format (optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The area path for the work item (optional)
    /// </summary>
    public string? AreaPath { get; set; }

    /// <summary>
    /// The iteration path for the work item (optional)
    /// </summary>
    public string? IterationPath { get; set; }

    /// <summary>
    /// The email or name of the user to assign the work item to (optional)
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// The priority of the work item (optional)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Additional fields to set on the work item (optional)
    /// </summary>
    public Dictionary<string, object>? AdditionalFields { get; set; }

    /// <summary>
    /// The ID of the parent work item to create a relationship with (optional)
    /// </summary>
    public int? ParentId { get; set; }
}