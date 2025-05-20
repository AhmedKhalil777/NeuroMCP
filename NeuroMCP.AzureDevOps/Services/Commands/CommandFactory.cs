using Microsoft.Extensions.Logging;
using NeuroMCP.AzureDevOps.Services.Common;

namespace NeuroMCP.AzureDevOps.Services.Commands;

/// <summary>
/// Factory for creating command objects
/// </summary>
public class CommandFactory : ICommandFactory
{
    private readonly IAzureDevOpsConnectionProvider _connectionProvider;
    private readonly ILoggerFactory _loggerFactory;

    public CommandFactory(IAzureDevOpsConnectionProvider connectionProvider, ILoggerFactory loggerFactory)
    {
        _connectionProvider = connectionProvider;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates a new pull request
    /// </summary>
    public CreatePullRequestCommand CreatePullRequest(
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
        return new CreatePullRequestCommand(
            _connectionProvider,
            _loggerFactory.CreateLogger<CreatePullRequestCommand>(),
            repositoryId,
            sourceRefName,
            targetRefName,
            title,
            description,
            isDraft,
            reviewers,
            workItemIds,
            projectId,
            organizationId);
    }

    /// <summary>
    /// Updates an existing pull request
    /// </summary>
    public UpdatePullRequestCommand UpdatePullRequest(
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
        string? organizationId = null)
    {
        return new UpdatePullRequestCommand(
            _connectionProvider,
            _loggerFactory.CreateLogger<UpdatePullRequestCommand>(),
            repositoryId,
            pullRequestId,
            title,
            description,
            status,
            isDraft,
            addReviewers,
            removeReviewers,
            addWorkItemIds,
            removeWorkItemIds,
            projectId,
            organizationId);
    }

    /// <summary>
    /// Adds a comment to a pull request
    /// </summary>
    public AddPullRequestCommentCommand AddPullRequestComment(
        string repositoryId,
        int pullRequestId,
        string content,
        int? threadId = null,
        int? parentCommentId = null,
        string? filePath = null,
        int? lineNumber = null,
        string? status = null,
        string? projectId = null,
        string? organizationId = null)
    {
        return new AddPullRequestCommentCommand(
            _connectionProvider,
            _loggerFactory.CreateLogger<AddPullRequestCommentCommand>(),
            repositoryId,
            pullRequestId,
            content,
            threadId,
            parentCommentId,
            filePath,
            lineNumber,
            status,
            projectId,
            organizationId);
    }

    /// <summary>
    /// Creates a new work item
    /// </summary>
    public CreateWorkItemCommand CreateWorkItem(
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
        string? organizationId = null)
    {
        return new CreateWorkItemCommand(
            _connectionProvider,
            _loggerFactory.CreateLogger<CreateWorkItemCommand>(),
            workItemType,
            title,
            description,
            areaPath,
            iterationPath,
            assignedTo,
            priority,
            additionalFields,
            parentId,
            projectId,
            organizationId);
    }

    /// <summary>
    /// Updates an existing work item
    /// </summary>
    public UpdateWorkItemCommand UpdateWorkItem(
        int workItemId,
        string? title = null,
        string? description = null,
        string? state = null,
        string? areaPath = null,
        string? iterationPath = null,
        string? assignedTo = null,
        int? priority = null,
        IDictionary<string, object>? additionalFields = null,
        string? organizationId = null)
    {
        return new UpdateWorkItemCommand(
            _connectionProvider,
            _loggerFactory.CreateLogger<UpdateWorkItemCommand>(),
            workItemId,
            title,
            description,
            state,
            areaPath,
            iterationPath,
            assignedTo,
            priority,
            additionalFields,
            organizationId);
    }
}