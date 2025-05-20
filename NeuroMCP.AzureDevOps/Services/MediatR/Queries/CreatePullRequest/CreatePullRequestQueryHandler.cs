using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.CreatePullRequest;

/// <summary>
/// Handler for creating a new pull request
/// </summary>
public class CreatePullRequestQueryHandler : AzureDevOpsRequestHandler<CreatePullRequestQuery, CreatePullRequestResult>
{
    public CreatePullRequestQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<CreatePullRequestQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to create a pull request
    /// </summary>
    public override async Task<CreatePullRequestResult> Handle(CreatePullRequestQuery request, CancellationToken cancellationToken)
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
            // Create the pull request definition
            var pullRequestToCreate = new GitPullRequest
            {
                SourceRefName = request.SourceRefName,
                TargetRefName = request.TargetRefName,
                Title = request.Title,
                Description = request.Description,
                IsDraft = request.IsDraft
            };

            // If additional properties are provided, add them
            if (request.AdditionalProperties != null)
            {
                foreach (var prop in request.AdditionalProperties)
                {
                    // Use reflection to set properties dynamically
                    var property = typeof(GitPullRequest).GetProperty(prop.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(pullRequestToCreate, prop.Value, null);
                    }
                }
            }

            // Create the pull request
            var createdPr = await gitClient.CreatePullRequestAsync(
                pullRequestToCreate,
                request.RepositoryId,
                projectId,
                cancellationToken: cancellationToken);

            // Add reviewers if specified
            if (request.Reviewers != null && request.Reviewers.Any())
            {
                var identities = new List<IdentityRef>();

                foreach (var reviewer in request.Reviewers)
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
                        createdPr.PullRequestId,
                        projectId,
                        cancellationToken: cancellationToken);
                }
            }

            // Add work item references if specified
            if (request.WorkItemIds != null && request.WorkItemIds.Any())
            {
                foreach (var workItemId in request.WorkItemIds)
                {
                    try
                    {
                        var artifactId = $"vstfs:///WorkItemTracking/WorkItem/{workItemId}";
                        await gitClient.CreatePullRequestWorkItemsAsync(
                            new[] { new ResourceRef { Id = artifactId } },
                            request.RepositoryId,
                            createdPr.PullRequestId,
                            projectId,
                            cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Failed to link work item {WorkItemId} to pull request {PullRequestId}",
                            workItemId, createdPr.PullRequestId);
                    }
                }
            }

            // Map the result
            var result = new CreatePullRequestResult
            {
                PullRequestId = createdPr.PullRequestId,
                Title = createdPr.Title,
                Description = createdPr.Description,
                SourceRefName = createdPr.SourceRefName,
                TargetRefName = createdPr.TargetRefName,
                Status = createdPr.Status.ToString(),
                IsDraft = createdPr.IsDraft,
                CreatedBy = createdPr.CreatedBy?.DisplayName ?? string.Empty,
                CreationDate = createdPr.CreationDate.ToString("o"),
                WebUrl = createdPr.Url
            };

            // Add repository information
            if (createdPr.Repository != null)
            {
                result.Repository = new RepositoryInfo
                {
                    Id = createdPr.Repository.Id.ToString(),
                    Name = createdPr.Repository.Name,
                    Url = createdPr.Repository.Url
                };

                if (createdPr.Repository.ProjectReference != null)
                {
                    result.Repository.Project = new ProjectInfo
                    {
                        Id = createdPr.Repository.ProjectReference.Id.ToString(),
                        Name = createdPr.Repository.ProjectReference.Name
                    };
                }
            }

            // Add reviewer information
            if (createdPr.Reviewers != null)
            {
                result.Reviewers = createdPr.Reviewers.Select(r => new ReviewerInfo
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
            Logger.LogError(ex, "Error creating pull request in repository {RepositoryId}", request.RepositoryId);
            throw;
        }
    }
}