using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListPullRequests;

/// <summary>
/// Handler for listing pull requests
/// </summary>
public class ListPullRequestsQueryHandler : AzureDevOpsRequestHandler<ListPullRequestsQuery, ListPullRequestsResult>
{
    public ListPullRequestsQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<ListPullRequestsQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to list pull requests
    /// </summary>
    public override async Task<ListPullRequestsResult> Handle(ListPullRequestsQuery request, CancellationToken cancellationToken)
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
            // Map status string to enum
            var statusFilter = !string.IsNullOrEmpty(request.Status)
                ? MapStatusToEnum(request.Status)
                : PullRequestStatus.Active;

            // Get pull requests
            var pullRequests = await gitClient.GetPullRequestsAsync(
                projectId,
                request.RepositoryId,
                new GitPullRequestSearchCriteria
                {
                    Status = statusFilter,
                    CreatorId = !string.IsNullOrEmpty(request.CreatorId) ? new Guid(request.CreatorId) : (Guid?)null,
                    ReviewerId = !string.IsNullOrEmpty(request.ReviewerId) ? new Guid(request.ReviewerId) : (Guid?)null,
                    SourceRefName = request.SourceRefName,
                    TargetRefName = request.TargetRefName
                },
                skip: request.Skip,
                top: request.Top,
                cancellationToken: cancellationToken);

            // Map the results
            var result = new ListPullRequestsResult
            {
                PullRequests = pullRequests.Select(pr => new PullRequestInfo
                {
                    PullRequestId = pr.PullRequestId,
                    Title = pr.Title,
                    Description = pr.Description,
                    SourceRefName = pr.SourceRefName,
                    TargetRefName = pr.TargetRefName,
                    Status = pr.Status.ToString(),
                    IsDraft = pr.IsDraft,
                    CreationDate = pr.CreationDate,
                    WebUrl = pr.Url,
                    CreatedBy = pr.CreatedBy != null ? new IdentityInfo
                    {
                        Id = pr.CreatedBy.Id.ToString(),
                        DisplayName = pr.CreatedBy.DisplayName,
                        EmailAddress = pr.CreatedBy.UniqueName,
                        ImageUrl = pr.CreatedBy.ImageUrl
                    } : new IdentityInfo(),
                    Repository = pr.Repository != null ? new RepositoryInfo
                    {
                        Id = pr.Repository.Id.ToString(),
                        Name = pr.Repository.Name,
                        Url = pr.Repository.Url,
                        Project = pr.Repository.ProjectReference != null ? new ProjectInfo
                        {
                            Id = pr.Repository.ProjectReference.Id.ToString(),
                            Name = pr.Repository.ProjectReference.Name
                        } : new ProjectInfo()
                    } : new RepositoryInfo(),
                    Reviewers = pr.Reviewers?.Select(r => new ReviewerInfo
                    {
                        Id = r.Id.ToString(),
                        DisplayName = r.DisplayName,
                        Vote = r.Vote,
                        IsRequired = r.IsRequired
                    }).ToList() ?? new List<ReviewerInfo>()
                }).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error listing pull requests for repository {RepositoryId}", request.RepositoryId);
            throw;
        }
    }

    /// <summary>
    /// Maps status string to PullRequestStatus enum
    /// </summary>
    private PullRequestStatus MapStatusToEnum(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "active" => PullRequestStatus.Active,
            "abandoned" => PullRequestStatus.Abandoned,
            "completed" => PullRequestStatus.Completed,
            "all" => PullRequestStatus.All,
            _ => PullRequestStatus.Active
        };
    }
}