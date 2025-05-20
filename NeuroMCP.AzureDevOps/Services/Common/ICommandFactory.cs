using Microsoft.TeamFoundation.SourceControl.WebApi;
using NeuroMCP.AzureDevOps.Services.Commands;

namespace NeuroMCP.AzureDevOps.Services.Common;

/// <summary>
/// Factory interface for creating command objects
/// </summary>
public interface ICommandFactory
{
    /// <summary>
    /// Creates a new pull request
    /// </summary>
    CreatePullRequestCommand CreatePullRequest(
        string repositoryId,
        string sourceRefName,
        string targetRefName,
        string title,
        string? description = null,
        bool isDraft = false,
        IEnumerable<string>? reviewers = null,
        IEnumerable<int>? workItemIds = null,
        string? projectId = null,
        string? organizationId = null);

    /// <summary>
    /// Updates an existing pull request
    /// </summary>
    UpdatePullRequestCommand UpdatePullRequest(
        string repositoryId,
        int pullRequestId,
        string? title = null,
        string? description = null,
        string? status = null,
        bool? isDraft = null,
        IEnumerable<string>? addReviewers = null,
        IEnumerable<string>? removeReviewers = null,
        IEnumerable<int>? addWorkItemIds = null,
        IEnumerable<int>? removeWorkItemIds = null,
        string? projectId = null,
        string? organizationId = null);

    /// <summary>
    /// Adds a comment to a pull request
    /// </summary>
    AddPullRequestCommentCommand AddPullRequestComment(
        string repositoryId,
        int pullRequestId,
        string content,
        int? threadId = null,
        int? parentCommentId = null,
        string? filePath = null,
        int? lineNumber = null,
        string? status = null,
        string? projectId = null,
        string? organizationId = null);

    /// <summary>
    /// Creates a new work item
    /// </summary>
    CreateWorkItemCommand CreateWorkItem(
        string workItemType,
        string title,
        string? description = null,
        string? areaPath = null,
        string? iterationPath = null,
        string? assignedTo = null,
        int? priority = null,
        IDictionary<string, object>? additionalFields = null,
        int? parentId = null,
        string? projectId = null,
        string? organizationId = null);

    /// <summary>
    /// Updates an existing work item
    /// </summary>
    UpdateWorkItemCommand UpdateWorkItem(
        int workItemId,
        string? title = null,
        string? description = null,
        string? state = null,
        string? areaPath = null,
        string? iterationPath = null,
        string? assignedTo = null,
        int? priority = null,
        IDictionary<string, object>? additionalFields = null,
        string? organizationId = null);
}