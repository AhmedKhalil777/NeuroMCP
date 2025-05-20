using NeuroMCP.AzureDevOps.Models;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetMe;

/// <summary>
/// Query to get the authenticated user
/// </summary>
public class GetMeQuery : AzureDevOpsRequest<AccountModel>
{
    // AzureDevOpsRequest already contains OrganizationId
}