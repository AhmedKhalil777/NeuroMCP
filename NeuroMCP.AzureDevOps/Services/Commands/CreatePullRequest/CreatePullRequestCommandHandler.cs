using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.Commands.CreatePullRequest;

/// <summary>
/// Handler for creating a pull request
/// </summary>
public class CreatePullRequestCommandHandler : AzureDevOpsRequestHandler<CreatePullRequestCommand, GitPullRequest>
{
    public CreatePullRequestCommandHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<CreatePullRequestCommandHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the create pull request command
    /// </summary>
    public override async Task<GitPullRequest> Handle(CreatePullRequestCommand request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();
        var projectName = GetProjectId(request);

        // Create a GitPullRequest object
        var pullRequest = new GitPullRequest
        {
            SourceRefName = request.SourceRefName,
            TargetRefName = request.TargetRefName,
            Title = request.Title,
            Description = request.Description,
            IsDraft = request.IsDraft
        };

        try
        {
            // Create the pull request
            var createdPr = await gitClient.CreatePullRequestAsync(pullRequest, request.RepositoryId, projectName, cancellationToken);

            // If reviewers are specified, add them
            if (request.Reviewers != null && request.Reviewers.Any())
            {
                // Add reviewers individually instead of as a batch
                foreach (var reviewer in request.Reviewers)
                {
                    try
                    {
                        // Try to find the user by email or display name
                        var identityRef = await ConnectionProvider.FindIdentityRefAsync(connection, reviewer);
                        if (identityRef != null)
                        {
                            var identityRefWithVote = new IdentityRefWithVote
                            {
                                Id = identityRef.Id,
                                DisplayName = identityRef.DisplayName,
                                UniqueName = identityRef.UniqueName,
                                Url = identityRef.Url,
                                Vote = 0 // No vote yet
                            };

                            await gitClient.CreatePullRequestReviewerAsync(
                                identityRefWithVote,
                                request.RepositoryId,
                                createdPr.PullRequestId,
                                identityRefWithVote.Id,
                                cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Could not add reviewer {Reviewer} to PR", reviewer);
                    }
                }
            }

            // If work items are specified, link them using the WorkItemTracking client
            if (request.WorkItemIds != null && request.WorkItemIds.Any())
            {
                var witClient = await connection.GetClientAsync<Microsoft.TeamFoundation.WorkItemTracking.WebApi.WorkItemTrackingHttpClient>();

                foreach (var workItemId in request.WorkItemIds)
                {
                    try
                    {
                        // Create a link from the work item to the pull request
                        var pullRequestArtifactUri = $"vstfs:///Git/PullRequestId/{createdPr.Repository.ProjectReference.Id}/{createdPr.PullRequestId}";

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
                            workItemId, createdPr.PullRequestId);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Could not link work item {WorkItemId} to PR {PullRequestId}",
                            workItemId, createdPr.PullRequestId);
                    }
                }
            }

            // Return the created PR
            return createdPr;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating pull request {Title}", request.Title);
            throw;
        }
    }
}