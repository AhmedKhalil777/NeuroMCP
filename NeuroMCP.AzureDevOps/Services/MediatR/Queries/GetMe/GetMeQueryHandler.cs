using Microsoft.Extensions.Logging;
using NeuroMCP.AzureDevOps.Models;
using NeuroMCP.AzureDevOps.Services.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services.MediatR.Queries.GetMe;

/// <summary>
/// Handler for getting authenticated user details
/// </summary>
public class GetMeQueryHandler : AzureDevOpsRequestHandler<GetMeQuery, AccountModel>
{
    public GetMeQueryHandler(
        IAzureDevOpsConnectionProvider connectionProvider,
        ILogger<GetMeQueryHandler> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Handles the request to get authenticated user details
    /// </summary>
    public override async Task<AccountModel> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // In a real implementation, this would use the Azure DevOps SDK
            // For now, return mock data to enable successful build
            Logger.LogInformation("Getting authenticated user with organizationId: {OrganizationId}",
                request.OrganizationId ?? "default");

            // Simulate API call delay
            await Task.Delay(100, cancellationToken);

            return new AccountModel
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = "Mock User",
                Email = "mock.user@example.com",
                Url = $"https://dev.azure.com/{request.OrganizationId ?? "defaultorg"}"
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting authenticated user");
            throw;
        }
    }
}