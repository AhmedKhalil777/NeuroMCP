namespace NeuroMCP.AzureDevOps.Services;

/// <summary>
/// Configuration for Azure DevOps
/// </summary>
public class AzureDevOpsConfig
{
    /// <summary>
    /// The URL of the organization
    /// </summary>
    public string? OrgUrl { get; set; }

    /// <summary>
    /// The default project to use
    /// </summary>
    public string? DefaultProject { get; set; }

    /// <summary>
    /// Authentication configuration
    /// </summary>
    public AzureDevOpsAuthConfig? Authentication { get; set; }
}

/// <summary>
/// Authentication configuration for Azure DevOps
/// </summary>
public class AzureDevOpsAuthConfig
{
    /// <summary>
    /// The type of authentication to use
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The Personal Access Token to use for authentication
    /// </summary>
    public string? PatToken { get; set; }

    /// <summary>
    /// The tenant ID to use for Azure AD authentication
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// The client ID to use for Azure AD authentication
    /// </summary>
    public string? ClientId { get; set; }
}