using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.UpdatePullRequest;

/// <summary>
/// Handler for updating a pull request
/// </summary>
public class UpdatePullRequestQueryHandler : AzureDevOpsRequestHandler<UpdatePullRequestQuery, UpdatePullRequestResult>
{
    public UpdatePullRequestQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<UpdatePullRequestQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to update a pull request
    /// </summary>
    public override async Task<UpdatePullRequestResult> Handle(UpdatePullRequestQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        // Determine project ID
        var projectId = request.ProjectId ?? ConnectionProvider.GetDefaultProject();
        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Project ID is required");
        }

        try
        {
            // First get the current pull request
            var pullRequest = await gitClient.GetPullRequestByIdAsync(
                request.PullRequestId,
                projectId,
                cancellationToken: cancellationToken);

            if (pullRequest == null)
            {
                throw new InvalidOperationException($"Pull request with ID {request.PullRequestId} not found");
            }

            // Create an update object
            var updates = new GitPullRequest
            {
                Title = request.Title ?? pullRequest.Title,
                Description = request.Description ?? pullRequest.Description,
                IsDraft = request.IsDraft ?? pullRequest.IsDraft,
            };

            // Set status if provided
            if (!string.IsNullOrEmpty(request.Status))
            {
                updates.Status = MapStatusStringToEnum(request.Status);
            }

            // If additional properties are provided, add them
            if (request.AdditionalProperties != null)
            {
                foreach (var prop in request.AdditionalProperties)
                {
                    // Use reflection to set properties dynamically
                    var property = typeof(GitPullRequest).GetProperty(prop.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(updates, prop.Value, null);
                    }
                }
            }

            // Update the pull request
            var updatedPr = await gitClient.UpdatePullRequestAsync(
                updates,
                request.RepositoryId,
                request.PullRequestId,
                projectId,
                cancellationToken: cancellationToken);

            // Add reviewers if specified
            if (request.AddReviewers != null && request.AddReviewers.Any())
            {
                var identities = new List<IdentityRef>();

                foreach (var reviewer in request.AddReviewers)
                {
                    var identity = await ConnectionProvider.FindIdentityRefAsync(connection, reviewer);
                    if (identity != null)
                    {
                        identities.Add(identity);
                    }
                    else
                    {
                        Logger.LogWarning("Could not find identity for reviewer: {Reviewer}", reviewer);
                    }
                }

                if (identities.Any())
                {
                    await gitClient.CreatePullRequestReviewersAsync(
                        identities.ToArray(),
                        request.RepositoryId,
                        request.PullRequestId,
                        projectId,
                        cancellationToken: cancellationToken);
                }
            }

            // Remove reviewers if specified
            if (request.RemoveReviewers != null && request.RemoveReviewers.Any())
            {
                foreach (var reviewer in request.RemoveReviewers)
                {
                    var identity = await ConnectionProvider.FindIdentityRefAsync(connection, reviewer);
                    if (identity != null)
                    {
                        try
                        {
                            await gitClient.DeletePullRequestReviewerAsync(
                                request.RepositoryId,
                                request.PullRequestId,
                                identity.Id,
                                projectId,
                                cancellationToken: cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Error removing reviewer {ReviewerId} from pull request {PullRequestId}",
                                identity.Id, request.PullRequestId);
                        }
                    }
                }
            }

            // Add work item references if specified
            if (request.AddWorkItemIds != null && request.AddWorkItemIds.Any())
            {
                var witClient = await connection.GetClientAsync<Microsoft.TeamFoundation.WorkItemTracking.WebApi.WorkItemTrackingHttpClient>();

                foreach (var workItemId in request.AddWorkItemIds)
                {
                    try
                    {
                        // Create a link from the work item to the pull request
                        var pullRequestArtifactUri = $"vstfs:///Git/PullRequestId/{updatedPr.Repository.ProjectReference.Id}/{updatedPr.PullRequestId}";

                        // Create a patch document to add the relation
                        var patchDocument = new JsonPatchDocument
                        {
                            new JsonPatchOperation
                            {
                                Operation = Operation.Add,
                                Path = "/relations/-",
                                Value = new
                                {
                                    rel = "ArtifactLink",
                                    url = pullRequestArtifactUri,
                                    attributes = new
                                    {
                                        name = "Pull Request"
                                    }
                                }
                            }
                        };

                        // Update the work item with the link to the PR
                        await witClient.UpdateWorkItemAsync(patchDocument, workItemId, cancellationToken: cancellationToken);
                        Logger.LogInformation("Linked work item {WorkItemId} to pull request {PullRequestId}",
                            workItemId, updatedPr.PullRequestId);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Could not link work item {WorkItemId} to PR {PullRequestId}",
                            workItemId, updatedPr.PullRequestId);
                    }
                }
            }

            // Remove work item references if specified
            if (request.RemoveWorkItemIds != null && request.RemoveWorkItemIds.Any())
            {
                var witClient = await connection.GetClientAsync<Microsoft.TeamFoundation.WorkItemTracking.WebApi.WorkItemTrackingHttpClient>();

                foreach (var workItemId in request.RemoveWorkItemIds)
                {
                    try
                    {
                        // Get the work item to find its relations
                        var workItem = await witClient.GetWorkItemAsync(
                            workItemId,
                            expand: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemExpand.Relations,
                            cancellationToken: cancellationToken);

                        // Find PR relationship to remove
                        var pullRequestArtifactUri = $"vstfs:///Git/PullRequestId/{updatedPr.Repository.ProjectReference.Id}/{updatedPr.PullRequestId}";

                        // Find the relation index
                        int relationIndex = -1;
                        for (int i = 0; i < workItem.Relations.Count; i++)
                        {
                            if (workItem.Relations[i].Url.Equals(pullRequestArtifactUri, StringComparison.OrdinalIgnoreCase))
                            {
                                relationIndex = i;
                                break;
                            }
                        }

                        if (relationIndex >= 0)
                        {
                            // Create a patch document to remove the relation
                            var patchDocument = new JsonPatchDocument
                            {
                                new JsonPatchOperation
                                {
                                    Operation = Operation.Remove,
                                    Path = $"/relations/{relationIndex}"
                                }
                            };

                            // Update the work item to remove the link
                            await witClient.UpdateWorkItemAsync(patchDocument, workItemId,cancellationToken: cancellationToken);
                            Logger.LogInformation("Removed link between work item {WorkItemId} and pull request {PullRequestId}",
                                workItemId, updatedPr.PullRequestId);
                        }
                        else
                        {
                            Logger.LogWarning("No link found between work item {WorkItemId} and pull request {PullRequestId}",
                                workItemId, updatedPr.PullRequestId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Error removing link between work item {WorkItemId} and pull request {PullRequestId}",
                            workItemId, updatedPr.PullRequestId);
                    }
                }
            }

            // Get the updated pull request with all relationships
            var finalPr = await gitClient.GetPullRequestByIdAsync(
                request.PullRequestId,
                projectId,
                cancellationToken: cancellationToken);

            // Get the work items linked to the PR by querying work items
            var workItemIds = new List<int>();
            try
            {
                // We need to query the work items separately since we don't have direct API access
                var witClient = await connection.GetClientAsync<Microsoft.TeamFoundation.WorkItemTracking.WebApi.WorkItemTrackingHttpClient>();
                var artifactUri = $"vstfs:///Git/PullRequestId/{finalPr.Repository.ProjectReference.Id}/{finalPr.PullRequestId}";

                // Build a WIQL query to find work items linked to this PR
                var queryString = $@"
                    SELECT [System.Id]
                    FROM WorkItems
                    WHERE [System.Links.LinkType] = 'ArtifactLink'
                    AND [System.Links.LinkedArtifact] = '{artifactUri}'";

                var wiqlQuery = new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.Wiql { Query = queryString };
                var queryResult = await witClient.QueryByWiqlAsync(wiqlQuery, projectId, cancellationToken: cancellationToken);

                if (queryResult.WorkItems != null && queryResult.WorkItems.Any())
                {
                    workItemIds = queryResult.WorkItems.Select(wi => wi.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error querying work items linked to pull request {PullRequestId}", finalPr.PullRequestId);
            }

            // Map the result
            var result = new UpdatePullRequestResult
            {
                PullRequestId = finalPr.PullRequestId,
                Title = finalPr.Title,
                Description = finalPr.Description,
                SourceRefName = finalPr.SourceRefName,
                TargetRefName = finalPr.TargetRefName,
                Status = finalPr.Status.ToString(),
                IsDraft = finalPr.IsDraft,
                WebUrl = finalPr.Url,
                WorkItemIds = workItemIds
            };

            // Add reviewer information
            if (finalPr.Reviewers != null)
            {
                result.Reviewers = finalPr.Reviewers.Select(r => new ReviewerInfo
                {
                    Id = r.Id.ToString(),
                    DisplayName = r.DisplayName,
                    Vote = r.Vote,
                    IsRequired = r.IsRequired
                }).ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating pull request {PullRequestId} in repository {RepositoryId}",
                request.PullRequestId, request.RepositoryId);
            throw;
        }
    }

    /// <summary>
    /// Maps a status string to a PullRequestStatus enum
    /// </summary>
    private PullRequestStatus MapStatusStringToEnum(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "active" => PullRequestStatus.Active,
            "abandoned" => PullRequestStatus.Abandoned,
            "completed" => PullRequestStatus.Completed,
            _ => throw new ArgumentException($"Invalid status: {status}. Valid values are: active, abandoned, completed.")
        };
    }
}