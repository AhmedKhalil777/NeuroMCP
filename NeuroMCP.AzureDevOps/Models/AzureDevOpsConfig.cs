namespace NeuroMCP.AzureDevOps.Models;

/// <summary>
/// Configuration for Azure DevOps connection
/// </summary>
public class AzureDevOpsConfig
{
    /// <summary>
    /// The Azure DevOps organization URL (e.g., https://dev.azure.com/yourorg)
    /// </summary>
    public string? OrgUrl { get; set; }

    /// <summary>
    /// The default project name to use
    /// </summary>
    public string? DefaultProject { get; set; }

    /// <summary>
    /// Authentication configuration
    /// </summary>
    public AuthConfig? Authentication { get; set; }
}

/// <summary>
/// Authentication configuration for Azure DevOps
/// </summary>
public class AuthConfig
{
    /// <summary>
    /// The authentication type: "pat", "interactive", "azureAD"
    /// </summary>
    public string Type { get; set; } = "pat";

    /// <summary>
    /// Personal Access Token (used when Type = "pat")
    /// </summary>
    public string? PatToken { get; set; }

    /// <summary>
    /// Azure AD tenant ID (used when Type = "azureAD")
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Azure AD client ID (used when Type = "azureAD")
    /// </summary>
    public string? ClientId { get; set; }
}