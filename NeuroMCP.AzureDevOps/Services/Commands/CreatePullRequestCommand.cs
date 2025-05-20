using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.Commands;

/// <summary>
/// Command to create a new pull request
/// </summary>
public class CreatePullRequestCommand : ICommand<GitPullRequest>
{
    private readonly IAzureDevOpsConnectionProvider _connectionProvider;
    private readonly ILogger<CreatePullRequestCommand> _logger;

    private readonly string _repositoryId;
    private readonly string _sourceRefName;
    private readonly string _targetRefName;
    private readonly string _title;
    private readonly string? _description;
    private readonly bool _isDraft;
    private readonly IEnumerable<string>? _reviewers;
    private readonly IEnumerable<int>? _workItemIds;
    private readonly string? _projectId;
    private readonly string? _organizationId;

    public CreatePullRequestCommand(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<CreatePullRequestCommand> logger,
        string repositoryId,
        string sourceRefName,
        string targetRefName,
        string title,
        string? description = null,
        bool isDraft = false,
        IEnumerable<string>? reviewers = null,
        IEnumerable<int>? workItemIds = null,
        string? projectId = null,
        string? organizationId = null)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
        _repositoryId = repositoryId;
        _sourceRefName = sourceRefName;
        _targetRefName = targetRefName;
        _title = title;
        _description = description;
        _isDraft = isDraft;
        _reviewers = reviewers;
        _workItemIds = workItemIds;
        _projectId = projectId;
        _organizationId = organizationId;
    }

    /// <summary>
    /// Executes the command to create a pull request
    /// </summary>
    public async Task<GitPullRequest> ExecuteAsync()
    {
        var connection = await _connectionProvider.GetConnectionAsync(_organizationId);
        var gitClient = await connection.GetClientAsync<GitHttpClient>();
        var projectName = _projectId ?? _connectionProvider.GetDefaultProject();

        // Create a GitPullRequest object
        var pullRequest = new GitPullRequest
        {
            SourceRefName = _sourceRefName,
            TargetRefName = _targetRefName,
            Title = _title,
            Description = _description,
            IsDraft = _isDraft
        };

        try
        {
            // Create the pull request
            var createdPr = await gitClient.CreatePullRequestAsync(pullRequest, _repositoryId, projectName);

            // If reviewers are specified, add them
            if (_reviewers != null && _reviewers.Any())
            {
                // Add reviewers individually instead of as a batch
                foreach (var reviewer in _reviewers)
                {
                    try
                    {
                        // Try to find the user by email or display name
                        var identityRef = await _connectionProvider.FindIdentityRefAsync(connection, reviewer);
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
                                _repositoryId,
                                createdPr.PullRequestId,
                                identityRefWithVote.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not add reviewer {Reviewer} to PR", reviewer);
                    }
                }
            }

            // If work items are specified, link them
            if (_workItemIds != null && _workItemIds.Any())
            {
                var witClient = await connection.GetClientAsync<Microsoft.TeamFoundation.WorkItemTracking.WebApi.WorkItemTrackingHttpClient>();

                foreach (var workItemId in _workItemIds)
                {
                    try
                    {
                        var artifactUri = $"vstfs:///Git/PullRequestId/{createdPr.Repository.ProjectReference.Id}/{createdPr.PullRequestId}";

                        // Add external link relationship
                        var patchDocument = new JsonPatchDocument
                        {
                            new JsonPatchOperation
                            {
                                Operation = Operation.Add,
                                Path = "/relations/-",
                                Value = new
                                {
                                    rel = "ArtifactLink",
                                    url = artifactUri,
                                    attributes = new
                                    {
                                        name = "Pull Request"
                                    }
                                }
                            }
                        };

                        await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not link work item {WorkItemId} to PR", workItemId);
                    }
                }
            }

            // Return the created PR
            return createdPr;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pull request");
            throw;
        }
    }
}