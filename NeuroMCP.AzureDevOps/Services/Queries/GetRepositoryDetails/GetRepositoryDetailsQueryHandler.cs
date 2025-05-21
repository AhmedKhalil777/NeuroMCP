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
            // Initialize the result with basic stats
            var result = new BranchStatistics
            {
                BranchName = branchName,
                CommitCount = 0,
                FileCount = 0,
                Size = 0,
            };

            try
            {
                // Get the repository info first to get default branch
                var repoInfo = await gitClient.GetRepositoryAsync(
                    repositoryId,
                    projectId.ToString(),
                    cancellationToken: cancellationToken);

                // Skip ahead/behind calculation if the branch is the default branch
                if (branchName == repoInfo.DefaultBranch)
                {
                    result.AheadBehind = new AheadBehindStatistics
                    {
                        Ahead = 0,
                        Behind = 0
                    };
                    return result;
                }

                // Get the default branch to compare with
                string defaultBranch = repoInfo.DefaultBranch ?? "refs/heads/main";

                // Get the commits for the branch
                var branchCommits = await gitClient.GetCommitsAsync(
                    repositoryId,
                    new GitQueryCommitsCriteria
                    {
                        ItemVersion = new GitVersionDescriptor
                        {
                            Version = branchName,
                            VersionType = GitVersionType.Branch
                        }
                    },
                    cancellationToken: cancellationToken);

                // Update commit count
                result.CommitCount = branchCommits.Count;

                // Get a count of files in the branch by looking at the root directory
                try
                {
                    var items = await gitClient.GetItemsAsync(
                        projectId,
                        repositoryId,
                        null, // null to get root items
                        recursionLevel: VersionControlRecursionType.OneLevel,
                        versionDescriptor: new GitVersionDescriptor
                        {
                            Version = branchName,
                            VersionType = GitVersionType.Branch
                        },
                        cancellationToken: cancellationToken);

                    result.FileCount = items.Count;

                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error getting files for branch {BranchName} in repository {RepositoryId}", branchName, repositoryId);
                }

                // Calculate ahead/behind using commit dates as a simple approximation
                try
                {
                    // Get the default branch commits
                    var defaultBranchCommits = await gitClient.GetCommitsAsync(
                        repositoryId,
                        new GitQueryCommitsCriteria
                        {
                            ItemVersion = new GitVersionDescriptor
                            {
                                Version = defaultBranch,
                                VersionType = GitVersionType.Branch
                            }
                        },
                        cancellationToken: cancellationToken);

                    if (branchCommits.Count > 0 && defaultBranchCommits.Count > 0)
                    {
                        // Find common ancestor (simple approximation using dates)
                        var defaultBranchLatestCommit = defaultBranchCommits.OrderByDescending(c => c.Author.Date).FirstOrDefault();
                        var branchLatestCommit = branchCommits.OrderByDescending(c => c.Author.Date).FirstOrDefault();

                        if (defaultBranchLatestCommit != null && branchLatestCommit != null)
                        {
                            // This is a simplistic approach - in reality, we need actual git graph traversal logic
                            int ahead = branchCommits.Count(c => c.Author.Date > defaultBranchLatestCommit.Author.Date);
                            int behind = defaultBranchCommits.Count(c => c.Author.Date > branchLatestCommit.Author.Date);

                            result.AheadBehind = new AheadBehindStatistics
                            {
                                Ahead = ahead,
                                Behind = behind
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error calculating ahead/behind for branch {BranchName} in repository {RepositoryId}", branchName, repositoryId);
                    result.AheadBehind = new AheadBehindStatistics { Ahead = 0, Behind = 0 };
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error getting detailed statistics for branch {BranchName} in repository {RepositoryId}", branchName, repositoryId);
                result.AheadBehind = new AheadBehindStatistics { Ahead = 0, Behind = 0 };
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