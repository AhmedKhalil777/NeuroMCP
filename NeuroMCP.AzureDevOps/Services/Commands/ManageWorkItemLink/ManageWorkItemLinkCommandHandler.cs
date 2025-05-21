using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.Commands.ManageWorkItemLink;

/// <summary>
/// Handler for adding, removing, or updating links between work items
/// </summary>
public class ManageWorkItemLinkCommandHandler : AzureDevOpsRequestHandler<ManageWorkItemLinkCommand, WorkItem>
{
    public ManageWorkItemLinkCommandHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<ManageWorkItemLinkCommandHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to manage work item links
    /// </summary>
    public override async Task<WorkItem> Handle(ManageWorkItemLinkCommand request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        // Create a JSON patch document for the operation
        var patchDocument = new JsonPatchDocument();

        // Get the proper operation based on the request
        var operation = request.Operation.ToLowerInvariant() switch
        {
            "add" => Operation.Add,
            "remove" => Operation.Remove,
            "update" => Operation.Replace,
            _ => throw new ArgumentException($"Invalid operation: {request.Operation}. Expected 'add', 'remove', or 'update'.")
        };

        // Add the appropriate patch operation for the link
        if (operation == Operation.Add || operation == Operation.Replace)
        {
            patchDocument.Add(
                new JsonPatchOperation
                {
                    Operation = operation,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = request.RelationType,
                        url = $"{connection.Uri}_apis/wit/workItems/{request.TargetWorkItemId}",
                        attributes = new
                        {
                            comment = request.Comment
                        }
                    }
                });
        }
        else if (operation == Operation.Remove)
        {
            // For remove, we need to find the index of the relation to remove
            var sourceWorkItem = await witClient.GetWorkItemAsync(
                request.SourceWorkItemId,
                expand: WorkItemExpand.Relations,
                cancellationToken: cancellationToken);

            int relationIndex = -1;
            if (sourceWorkItem.Relations != null)
            {
                for (int i = 0; i < sourceWorkItem.Relations.Count; i++)
                {
                    var relation = sourceWorkItem.Relations[i];
                    if (relation.Rel == request.RelationType &&
                        relation.Url.EndsWith($"/{request.TargetWorkItemId}"))
                    {
                        relationIndex = i;
                        break;
                    }
                }
            }

            if (relationIndex >= 0)
            {
                patchDocument.Add(
                    new JsonPatchOperation
                    {
                        Operation = Operation.Remove,
                        Path = $"/relations/{relationIndex}"
                    });
            }
            else
            {
                throw new InvalidOperationException($"Relation of type '{request.RelationType}' to work item {request.TargetWorkItemId} not found.");
            }
        }

        // Update the work item with the new relation
        var updatedWorkItem = await witClient.UpdateWorkItemAsync(
            document: patchDocument,
            id: request.SourceWorkItemId,
            expand: WorkItemExpand.Relations,
            cancellationToken: cancellationToken);

        return updatedWorkItem;
    }
}