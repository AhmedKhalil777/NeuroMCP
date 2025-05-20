using NeuroMCP.AzureDevOps.Models;
using System.Collections.Generic;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListOrganizations;

/// <summary>
/// Query to list all accessible organizations
/// </summary>
public class ListOrganizationsQuery : AzureDevOpsRequest<IEnumerable<AccountModel>>
{
    // No additional parameters needed
}