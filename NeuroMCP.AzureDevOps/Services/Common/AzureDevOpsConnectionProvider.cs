using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using NeuroMCP.AzureDevOps.Models;
using System.Collections.Concurrent;

namespace NeuroMCP.AzureDevOps.Services.Common;

/// <summary>
/// Provides connections to Azure DevOps
/// </summary>
public class AzureDevOpsConnectionProvider : IAzureDevOpsConnectionProvider
{
    private readonly ILogger<AzureDevOpsConnectionProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, VssConnection> _connections = new();

    private string? _defaultOrgUrl;
    private string? _defaultProject;
    private string? _patToken;
    private string? _authType;
    private string? _tenantId;
    private string? _clientId;

    public AzureDevOpsConnectionProvider(ILogger<AzureDevOpsConnectionProvider> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        // Try to load from environment variables first
        _defaultOrgUrl = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_ORG_URL");
        _defaultProject = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_PROJECT");
        _patToken = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_PAT");
        _authType = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_AUTH_TYPE") ?? "pat";
        _tenantId = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_TENANT_ID");
        _clientId = Environment.GetEnvironmentVariable("NEUROMCP_AZDEVOPS_CLIENT_ID");

        // Try to load from configuration if not found in environment
        var azureDevOpsConfig = _configuration.GetSection("azureDevOps").Get<AzureDevOpsConfig>();
        if (azureDevOpsConfig != null)
        {
            _defaultOrgUrl ??= azureDevOpsConfig.OrgUrl;
            _defaultProject ??= azureDevOpsConfig.DefaultProject;

            if (azureDevOpsConfig.Authentication != null)
            {
                _authType ??= azureDevOpsConfig.Authentication.Type;
                _patToken ??= azureDevOpsConfig.Authentication.PatToken;
                _tenantId ??= azureDevOpsConfig.Authentication.TenantId;
                _clientId ??= azureDevOpsConfig.Authentication.ClientId;
            }
        }

        _logger.LogInformation("Azure DevOps configuration loaded. Default organization: {OrgUrl}",
            string.IsNullOrEmpty(_defaultOrgUrl) ? "Not set" : _defaultOrgUrl);
    }

    /// <summary>
    /// Get a VssConnection to Azure DevOps
    /// </summary>
    public async Task<VssConnection> GetConnectionAsync(string? organizationUrl = null)
    {
        var orgUrl = organizationUrl ?? _defaultOrgUrl;
        if (string.IsNullOrEmpty(orgUrl))
        {
            throw new ArgumentException("Organization URL must be provided either in configuration or as a parameter");
        }

        // Check if we already have a connection for this organization
        if (_connections.TryGetValue(orgUrl, out var existingConnection))
        {
            return existingConnection;
        }

        // Create a new connection based on authentication type
        VssConnection connection;

        switch (_authType?.ToLowerInvariant())
        {
            case "pat":
                if (string.IsNullOrEmpty(_patToken))
                {
                    throw new InvalidOperationException("PAT token is required for PAT authentication");
                }

                var patCredentials = new VssBasicCredential(string.Empty, _patToken);
                connection = new VssConnection(new Uri(orgUrl), patCredentials);
                break;

            case "azuread":
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    TenantId = _tenantId,
                    ManagedIdentityClientId = _clientId
                });

                // Define scopes for Azure DevOps
                string[] scopes = new[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" }; // Azure DevOps API scope

                connection = new VssConnection(new Uri(orgUrl), new VssAzureADCredential(credential, scopes));
                break;

            case "interactive":
                connection = new VssConnection(new Uri(orgUrl), new VssClientCredentials(useDefaultCredentials: true));
                break;

            default:
                throw new InvalidOperationException($"Unsupported authentication type: {_authType}");
        }

        // Test the connection to ensure it works
        await connection.ConnectAsync();

        // Add to connection cache
        _connections[orgUrl] = connection;

        return connection;
    }

    /// <summary>
    /// Gets the default project name if configured
    /// </summary>
    public string? GetDefaultProject()
    {
        return _defaultProject;
    }

    /// <summary>
    /// Find an identity reference by email or display name
    /// </summary>
    public async Task<Microsoft.VisualStudio.Services.WebApi.IdentityRef?> FindIdentityRefAsync(VssConnection connection, string identifier)
    {
        try
        {
            _logger.LogInformation("Finding identity for {Identifier}", identifier);

            // First try using the identity service
            var identityService = await connection.GetClientAsync<Microsoft.VisualStudio.Services.Identity.Client.IdentityHttpClient>();

            // Create a simple identity reference for email addresses
            if (identifier.Contains('@'))
            {
                // Try to find by email
                try
                {
                    // Try exact matching on DisplayName or UniqueName
                    var identities = await identityService.ReadIdentitiesAsync(
                        Microsoft.VisualStudio.Services.Identity.IdentitySearchFilter.General,
                        identifier);

                    var identity = identities?.FirstOrDefault();
                    if (identity != null)
                    {
                        return new Microsoft.VisualStudio.Services.WebApi.IdentityRef
                        {
                            Id = identity.Id.ToString(),
                            DisplayName = identity.DisplayName,
                            UniqueName = identity.Properties.TryGetValue("Mail", out var mail)
                                ? mail.ToString()
                                : identifier
                        };
                    }

                    // Fall back to creating a simple identity with the email
                    return new Microsoft.VisualStudio.Services.WebApi.IdentityRef
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = identifier.Split('@')[0],
                        UniqueName = identifier
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error finding identity by email for {Email}", identifier);
                }
            }

            // Try simple display name search as a fallback
            try
            {
                var identities = await identityService.ReadIdentitiesAsync(
                    Microsoft.VisualStudio.Services.Identity.IdentitySearchFilter.DisplayName,
                    identifier);

                var identity = identities?.FirstOrDefault();
                if (identity != null)
                {
                    return new Microsoft.VisualStudio.Services.WebApi.IdentityRef
                    {
                        Id = identity.Id.ToString(),
                        DisplayName = identity.DisplayName,
                        UniqueName = identity.Properties.TryGetValue("Mail", out var mail)
                            ? mail.ToString()
                            : identity.DisplayName
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error finding identity by display name for {DisplayName}", identifier);
            }

            // Last fallback - create an arbitrary identity reference
            // This may cause issues, but it's better than failing the entire operation
            _logger.LogWarning("Could not find identity for {Identifier}, creating placeholder", identifier);
            return new Microsoft.VisualStudio.Services.WebApi.IdentityRef
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = identifier,
                UniqueName = identifier
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding identity reference for {Identifier}", identifier);
            return null;
        }
    }
}