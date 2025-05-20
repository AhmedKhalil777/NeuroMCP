using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                        identities,
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
                var artifactIds = request.AddWorkItemIds.Select(id => new ResourceRef
                {
                    Id = $"vstfs:///WorkItemTracking/WorkItem/{id}"
                }).ToArray();

                await gitClient.CreatePullRequestWorkItemsAsync(
                    artifactIds,
                    request.RepositoryId,
                    request.PullRequestId,
                    projectId,
                    cancellationToken: cancellationToken);
            }

            // Remove work item references if specified
            if (request.RemoveWorkItemIds != null && request.RemoveWorkItemIds.Any())
            {
                foreach (var workItemId in request.RemoveWorkItemIds)
                {
                    try
                    {
                        var artifactId = $"vstfs:///WorkItemTracking/WorkItem/{workItemId}";
                        await gitClient.RemovePullRequestWorkItemsAsync(
                            request.RepositoryId,
                            request.PullRequestId,
                            artifactId,
                            projectId,
                            cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Error removing work item {WorkItemId} from pull request {PullRequestId}",
                            workItemId, request.PullRequestId);
                    }
                }
            }

            // Get the updated pull request with all relationships
            var finalPr = await gitClient.GetPullRequestByIdAsync(
                request.PullRequestId,
                projectId,
                cancellationToken: cancellationToken);

            // Get the work items linked to the PR
            var workItems = await gitClient.GetPullRequestWorkItemsAsync(
                request.RepositoryId,
                request.PullRequestId,
                projectId,
                cancellationToken: cancellationToken);

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
                WorkItemIds = workItems?.Select(w => int.Parse(w.Id.Split('/').Last())).ToList()
                    ?? new List<int>()
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