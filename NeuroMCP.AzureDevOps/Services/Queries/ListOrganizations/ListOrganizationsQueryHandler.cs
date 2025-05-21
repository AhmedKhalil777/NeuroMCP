using Microsoft.Extensions.Logging;
using NeuroMCP.AzureDevOps.Models;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.ListOrganizations;

/// <summary>
/// Handler for listing all accessible organizations
/// </summary>
public class ListOrganizationsQueryHandler : AzureDevOpsRequestHandler<ListOrganizationsQuery, IEnumerable<AccountModel>>
{
    public ListOrganizationsQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<ListOrganizationsQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to list organizations
    /// </summary>
    public override async Task<IEnumerable<AccountModel>> Handle(ListOrganizationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // In a real implementation, this would use the Azure DevOps SDK
            // For now, return mock data to enable successful build
            Logger.LogInformation("Listing organizations");

            // Simulate API call delay
            await Task.Delay(100, cancellationToken);

            // Return mock organizations
            return new List<AccountModel>
            {
                new AccountModel
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayName = "Sample Organization 1",
                    Email = string.Empty,
                    Url = "https://dev.azure.com/sample-org-1"
                },
                new AccountModel
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayName = "Sample Organization 2",
                    Email = string.Empty,
                    Url = "https://dev.azure.com/sample-org-2"
                },
                new AccountModel
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayName = "Sample Organization 3",
                    Email = string.Empty,
                    Url = "https://dev.azure.com/sample-org-3"
                }
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error listing organizations");
            throw;
        }
    }
}