using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using NeuroMCP.AzureDevOps.Services.Commands.CreateWorkItem;
using NeuroMCP.AzureDevOps.Services.Commands.ManageWorkItemLink;
using NeuroMCP.AzureDevOps.Services.Commands.UpdateWorkItem;
using NeuroMCP.AzureDevOps.Services.Common.Models.Commands;
using NeuroMCP.AzureDevOps.Services.Common.Models.Queries;
using NeuroMCP.AzureDevOps.Services.MediatR.Commands.CreateWorkItem;
using NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetWorkItem;
using NeuroMCP.AzureDevOps.Services.Queries.SearchWorkItems;
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
        [FromQuery] int? skip = null,
        [FromQuery] int? top = null,
        [FromQuery] string? organizationId = null,
        [FromQuery] string? projectId = null)
    {
        var query = new SearchWorkItemsQuery
        {
            Model = new SearchWorkItemsModel
            {
                SearchText = searchText,
                Skip = skip,
                Top = top
            },
            OrganizationId = organizationId,
            ProjectId = projectId
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

        return NotFound("Get work item functionality is not implemented yet");
    }

    /// <summary>
    /// Create a new work item
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkItem>> CreateWorkItemAsync(
        [FromBody] CreateWorkItemRequest request,
        [FromQuery] string? organizationId = null,
        [FromQuery] string? projectId = null)
    {
        var command = new CreateWorkItemCommand
        {
            WorkItemType = request.WorkItemType,
            Title = request.Title,
            Description = request.Description,
            AreaPath = request.AreaPath,
            IterationPath = request.IterationPath,
            AssignedTo = request.AssignedTo,
            Priority = request.Priority,
            ParentId = request.ParentId,
            AdditionalFields = request.AdditionalFields,
            OrganizationId = organizationId,
            ProjectId = projectId
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetWorkItemAsync), new { workItemId = result.Id }, result);
    }

    /// <summary>
    /// Update an existing work item
    /// </summary>
    [HttpPut("{workItemId}")]
    public async Task<ActionResult> UpdateWorkItemAsync(
        [FromRoute] int workItemId,
        [FromBody] Dictionary<string, object> fields,
        [FromQuery] string? organizationId = null)
    {

        var command = new UpdateWorkItemCommand
        {
            WorkItemId = workItemId,
            Fields = fields,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(command);
        return Ok(result);

        // return NotFound("Update work item functionality is not implemented yet");
    }

    /// <summary>
    /// Manage links between work items
    /// </summary>
    [HttpPost("{sourceWorkItemId}/links")]
    public async Task<ActionResult> ManageWorkItemLinkAsync(
        [FromRoute] int sourceWorkItemId,
        [FromQuery] int targetWorkItemId,
        [FromQuery] string relationType,
        [FromQuery] string operation,
        [FromQuery] string? comment = null,
        [FromQuery] string? organizationId = null)
    {

        var command = new ManageWorkItemLinkCommand
        {
            SourceWorkItemId = sourceWorkItemId,
            TargetWorkItemId = targetWorkItemId,
            RelationType = relationType,
            Operation = operation,
            Comment = comment,
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
        //.return NotFound("Manage work item links functionality is not implemented yet");
    }
}

/// <summary>
/// Request model for creating a work item
/// </summary>
public class CreateWorkItemRequest
{
    /// <summary>
    /// The type of work item to create (e.g., "Task", "Bug", "User Story")
    /// </summary>
    public string WorkItemType { get; set; } = string.Empty;

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