using Microsoft.Extensions.DependencyInjection;
using NeuroMCP.AzureDevOps.Services.Commands;
using NeuroMCP.AzureDevOps.Services.Common;
using NeuroMCP.AzureDevOps.Services.Queries;
using System.Reflection;

namespace NeuroMCP.AzureDevOps.Services;

/// <summary>
/// Extension methods for setting up Azure DevOps services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure DevOps services to the service collection
    /// </summary>
    public static IServiceCollection AddAzureDevOpsServices(this IServiceCollection services)
    {

        // Register common services
        services.AddSingleton<IAzureDevOpsConnectionProvider, AzureDevOpsConnectionProvider>();

        // Register MediatR for CQRS
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });


        return services;
    }
}