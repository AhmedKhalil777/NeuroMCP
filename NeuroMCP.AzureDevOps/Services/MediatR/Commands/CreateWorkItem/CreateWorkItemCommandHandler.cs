using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Commands.CreateWorkItem;

/// <summary>
/// Handler for creating a work item
/// </summary>
public class CreateWorkItemCommandHandler : AzureDevOpsRequestHandler<CreateWorkItemCommand, WorkItem>
{
    public CreateWorkItemCommandHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<CreateWorkItemCommandHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the create work item command
    /// </summary>
    public override async Task<WorkItem> Handle(CreateWorkItemCommand request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();
        var projectName = GetProjectId(request);

        // Create patch document with initial fields
        var patchDocument = new JsonPatchDocument();

        // Add title field
        patchDocument.Add(new JsonPatchOperation
        {
            Operation = Operation.Add,
            Path = "/fields/System.Title",
            Value = request.Title
        });

        // Add description if provided
        if (!string.IsNullOrEmpty(request.Description))
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Description",
                Value = request.Description
            });
        }

        // Add area path if provided
        if (!string.IsNullOrEmpty(request.AreaPath))
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.AreaPath",
                Value = request.AreaPath
            });
        }

        // Add iteration path if provided
        if (!string.IsNullOrEmpty(request.IterationPath))
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.IterationPath",
                Value = request.IterationPath
            });
        }

        // Add assigned to if provided
        if (!string.IsNullOrEmpty(request.AssignedTo))
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.AssignedTo",
                Value = request.AssignedTo
            });
        }

        // Add priority if provided
        if (request.Priority.HasValue)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.Common.Priority",
                Value = request.Priority.Value
            });
        }

        // Add additional fields if provided
        if (request.AdditionalFields != null)
        {
            foreach (var field in request.AdditionalFields)
            {
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{field.Key}",
                    Value = field.Value
                });
            }
        }

        try
        {
            // Create the work item
            var workItem = await witClient.CreateWorkItemAsync(
                patchDocument,
                projectName,
                request.WorkItemType,
                bypassRules: false,
                validateOnly: false,
                cancellationToken: cancellationToken);

            // If parent ID is provided, create a link
            if (request.ParentId.HasValue)
            {
                var linkPatchDocument = new JsonPatchDocument();
                linkPatchDocument.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{connection.Uri}_apis/wit/workItems/{request.ParentId}",
                        attributes = new
                        {
                            comment = "Created as child"
                        }
                    }
                });

                workItem = await witClient.UpdateWorkItemAsync(
                    linkPatchDocument,
                    workItem.Id.Value,
                    cancellationToken: cancellationToken);
            }

            return workItem;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating work item of type {WorkItemType}", request.WorkItemType);
            throw;
        }
    }
}