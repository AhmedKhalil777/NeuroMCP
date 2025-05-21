using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Services.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.Commands.UpdateWorkItem;

/// <summary>
/// Handler for updating an existing work item
/// </summary>
public class UpdateWorkItemCommandHandler : AzureDevOpsRequestHandler<UpdateWorkItemCommand, WorkItem>
{
    public UpdateWorkItemCommandHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<UpdateWorkItemCommandHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to update a work item
    /// </summary>
    public override async Task<WorkItem> Handle(UpdateWorkItemCommand request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var witClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        // Create a JSON patch document to update the work item
        var patchDocument = new JsonPatchDocument();

        // Add fields to the patch document
        foreach (var field in request.Fields)
        {
            // Handle multi-line text fields with HTML format
            if (field.Key == "System.Description" || field.Key == "System.History" ||
                field.Key.EndsWith("HtmlText") || field.Key.EndsWith("HtmlField"))
            {
                patchDocument.Add(
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{field.Key}",
                        Value = $"<div>{field.Value}</div>"
                    });
            }
            else
            {
                patchDocument.Add(
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = $"/fields/{field.Key}",
                        Value = field.Value
                    });
            }
        }

        // Update the work item
        var updatedWorkItem = await witClient.UpdateWorkItemAsync(
            document: patchDocument,
            id: request.WorkItemId,
            expand: WorkItemExpand.All,
            cancellationToken: cancellationToken);

        return updatedWorkItem;
    }
}