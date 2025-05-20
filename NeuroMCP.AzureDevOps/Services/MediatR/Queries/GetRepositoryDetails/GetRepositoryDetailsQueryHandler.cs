using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetRepositoryDetails;

/// <summary>
/// Handler for getting detailed repository information
/// </summary>
public class GetRepositoryDetailsQueryHandler : AzureDevOpsRequestHandler<GetRepositoryDetailsQuery, GetRepositoryDetailsResult>
{
    public GetRepositoryDetailsQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetRepositoryDetailsQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get detailed repository information
    /// </summary>
    public override async Task<GetRepositoryDetailsResult> Handle(GetRepositoryDetailsQuery request, CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(request.OrganizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();

        // Get the repository details
        var repository = await gitClient.GetRepositoryAsync(
            request.RepositoryId,
            request.ProjectId,
            cancellationToken: cancellationToken);

        // Create the result
        var result = new GetRepositoryDetailsResult
        {
            Id = repository.Id.ToString(),
            Name = repository.Name,
            Url = repository.Url,
            Project = repository.ProjectReference?.Name ?? string.Empty,
            DefaultBranch = repository.DefaultBranch ?? "refs/heads/main",
            Size = repository.Size,
            RemoteUrl = repository.RemoteUrl,
            WebUrl = repository.WebUrl,
            IsFork = repository.IsFork
        };

        // Get the refs if requested
        if (request.IncludeRefs)
        {
            result.Refs = await GetRefsAsync(gitClient, repository.Id, repository.ProjectReference.Id, cancellationToken);
        }

        // Get the statistics if requested
        if (request.IncludeStatistics)
        {
            result.Statistics = await GetStatisticsAsync(gitClient, repository.Id, repository.ProjectReference.Id,
                request.BranchName ?? repository.DefaultBranch, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Get references for a repository
    /// </summary>
    private async Task<IEnumerable<GitReferenceInfo>> GetRefsAsync(GitHttpClient gitClient, Guid repositoryId, Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var refs = await gitClient.GetRefsAsync(
                repositoryId.ToString(),
                projectId.ToString(),
                filter: null, // No filter, get all refs
                includeLinks: true,
                includeStatuses: false,
                includeMyBranches: false,
                latestStatusesOnly: false,
                cancellationToken: cancellationToken);

            return refs.Select(r => new GitReferenceInfo
            {
                Name = r.Name,
                ObjectId = r.ObjectId,
                Url = r.Url
            }).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error getting refs for repository {RepositoryId}", repositoryId);
            return new List<GitReferenceInfo>();
        }
    }

    /// <summary>
    /// Get statistics for a branch
    /// </summary>
    private async Task<BranchStatistics> GetStatisticsAsync(GitHttpClient gitClient, Guid repositoryId, Guid projectId, string branchName, CancellationToken cancellationToken)
    {
        try
        {
            var stats = await gitClient.GetBranchStatisticsAsync(
                repositoryId,
                branchName,
                cancellationToken: cancellationToken);

            var aheadBehind = await gitClient.GetBranchStatsBatchAsync(
                new List<GitPullRequestQuery>
                {
                    new GitPullRequestQuery
                    {
                        Items = new List<GitVersionDescriptor>
                        {
                            new GitVersionDescriptor
                            {
                                Version = branchName,
                                VersionType = GitVersionType.Branch
                            }
                        },
                        ComparisonVersionCommit = string.Empty // Will compare to default branch
                    }
                },
                repositoryId,
                projectId.ToString(),
                cancellationToken: cancellationToken);

            var result = new BranchStatistics
            {
                BranchName = branchName,
                CommitCount = stats.CommitCount,
                FileCount = stats.FileCount,
                Size = stats.FileSize,
            };

            // If we have ahead/behind stats
            if (aheadBehind != null && aheadBehind.Count > 0 && aheadBehind[0].CommitCounts != null)
            {
                result.AheadBehind = new AheadBehindStatistics
                {
                    Ahead = aheadBehind[0].CommitCounts.Count > 0 ? aheadBehind[0].CommitCounts[0].Ahead : 0,
                    Behind = aheadBehind[0].CommitCounts.Count > 0 ? aheadBehind[0].CommitCounts[0].Behind : 0
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error getting statistics for branch {BranchName} in repository {RepositoryId}", branchName, repositoryId);
            return new BranchStatistics
            {
                BranchName = branchName,
                CommitCount = 0,
                FileCount = 0,
                Size = 0
            };
        }
    }
}