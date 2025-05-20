using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ManageWorkItemLink;

/// <summary>
/// Handler for managing links between work items
/// </summary>
public class ManageWorkItemLinkQueryHandler : AzureDevOpsRequestHandler<ManageWorkItemLinkQuery, WorkItem>
{
    public ManageWorkItemLinkQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<ManageWorkItemLinkQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to manage work item links
    /// </summary>
    public override async Task<WorkItem> Handle(ManageWorkItemLinkQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        try
        {
            // Create a patch document for the source work item
            var patchDocument = new JsonPatchDocument();

            // Check the operation type
            switch (request.Operation.ToLowerInvariant())
            {
                case "add":
                    // Add a relation
                    patchDocument.Add(
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/relations/-",
                            Value = new
                            {
                                rel = request.RelationType,
                                url = $"{connection.Uri}_apis/wit/workItems/{request.TargetWorkItemId}",
                                attributes = new Dictionary<string, object>
                                {
                                    { "comment", request.Comment ?? string.Empty }
                                }
                            }
                        });
                    break;

                case "remove":
                    // First, get the source work item to find the relation to remove
                    var sourceWorkItem = await witClient.GetWorkItemAsync(
                        request.SourceWorkItemId,
                        expand: WorkItemExpand.Relations,
                        cancellationToken: cancellationToken);

                    // Find the relation to the target work item
                    var relationToRemove = sourceWorkItem.Relations?
                        .FirstOrDefault(r => r.Rel == request.RelationType &&
                                             r.Url.EndsWith($"/{request.TargetWorkItemId}"));

                    if (relationToRemove == null)
                    {
                        throw new InvalidOperationException($"Relation of type '{request.RelationType}' from work item {request.SourceWorkItemId} to {request.TargetWorkItemId} not found");
                    }

                    // Find the index of the relation in the array
                    var relationIndex = sourceWorkItem.Relations.ToList().IndexOf(relationToRemove);

                    // Remove the relation
                    patchDocument.Add(
                        new JsonPatchOperation
                        {
                            Operation = Operation.Remove,
                            Path = $"/relations/{relationIndex}"
                        });
                    break;

                case "update":
                    if (string.IsNullOrEmpty(request.NewRelationType))
                    {
                        throw new InvalidOperationException("NewRelationType is required for update operation");
                    }

                    // First, get the source work item to find the relation to update
                    var sourceWorkItemForUpdate = await witClient.GetWorkItemAsync(
                        request.SourceWorkItemId,
                        expand: WorkItemExpand.Relations,
                        cancellationToken: cancellationToken);

                    // Find the relation to the target work item
                    var relationToUpdate = sourceWorkItemForUpdate.Relations?
                        .FirstOrDefault(r => r.Rel == request.RelationType &&
                                             r.Url.EndsWith($"/{request.TargetWorkItemId}"));

                    if (relationToUpdate == null)
                    {
                        throw new InvalidOperationException($"Relation of type '{request.RelationType}' from work item {request.SourceWorkItemId} to {request.TargetWorkItemId} not found");
                    }

                    // Find the index of the relation in the array
                    var updateIndex = sourceWorkItemForUpdate.Relations.ToList().IndexOf(relationToUpdate);

                    // Update the relation type
                    patchDocument.Add(
                        new JsonPatchOperation
                        {
                            Operation = Operation.Replace,
                            Path = $"/relations/{updateIndex}/rel",
                            Value = request.NewRelationType
                        });

                    // If comment provided, update it
                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        patchDocument.Add(
                            new JsonPatchOperation
                            {
                                Operation = Operation.Replace,
                                Path = $"/relations/{updateIndex}/attributes/comment",
                                Value = request.Comment
                            });
                    }
                    break;

                default:
                    throw new ArgumentException($"Unknown operation type: {request.Operation}. Must be 'add', 'remove', or 'update'");
            }

            // Apply the patch
            return await witClient.UpdateWorkItemAsync(
                patchDocument,
                request.SourceWorkItemId,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error managing work item link between {SourceId} and {TargetId} with operation {Operation}",
                request.SourceWorkItemId, request.TargetWorkItemId, request.Operation);
            throw;
        }
    }
} 