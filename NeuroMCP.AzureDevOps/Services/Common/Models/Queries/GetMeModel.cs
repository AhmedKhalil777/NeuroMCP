namespace NeuroMCP.AzureDevOps.Services.Common.Models.Queries;

/// <summary>
/// Model for getting the authenticated user
/// </summary>
public class GetMeModel
{
    /// <summary>
    /// The organization ID or URL (optional)
    /// </summary>
    public string? OrganizationId { get; set; }
}