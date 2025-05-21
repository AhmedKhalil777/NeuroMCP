using System;
using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetProjectDetails;

/// <summary>
/// Result model for project details
/// </summary>
public class GetProjectDetailsResult
{
    /// <summary>
    /// Project ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Project description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Project URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Project state
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Project visibility
    /// </summary>
    public string Visibility { get; set; } = string.Empty;

    /// <summary>
    /// Project capabilities
    /// </summary>
    public IDictionary<string, Dictionary<string, string>> Capabilities { get; set; } = new Dictionary<string, Dictionary<string, string>>();

    /// <summary>
    /// Default team information
    /// </summary>
    public TeamReference DefaultTeam { get; set; } = new TeamReference();

    /// <summary>
    /// Teams in the project
    /// </summary>
    public IEnumerable<TeamReference> Teams { get; set; } = Array.Empty<TeamReference>();

    /// <summary>
    /// Process information
    /// </summary>
    public ProcessReference Process { get; set; } = new ProcessReference();

    /// <summary>
    /// Work item types in the project
    /// </summary>
    public IEnumerable<WorkItemTypeReference> WorkItemTypes { get; set; } = Array.Empty<WorkItemTypeReference>();
}

/// <summary>
/// Reference to a team
/// </summary>
public class TeamReference
{
    /// <summary>
    /// Team ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Team name
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Reference to a process
/// </summary>
public class ProcessReference
{
    /// <summary>
    /// Process name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Process version
    /// </summary>
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Reference to a work item type
/// </summary>
public class WorkItemTypeReference
{
    /// <summary>
    /// Work item type name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Work item type reference name
    /// </summary>
    public string ReferenceName { get; set; } = string.Empty;

    /// <summary>
    /// Work item type description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Work item type color
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Work item type icon URL
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Whether the work item type is disabled
    /// </summary>
    public bool IsDisabled { get; set; } = false;
}