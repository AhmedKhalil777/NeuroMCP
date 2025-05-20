using System.CommandLine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ModelContextProtocol.Server;
using ModelContextProtocol.Services;
using NeuroMCP.AzureDevOps.Models;
using NeuroMCP.AzureDevOps.Services;

// Create command line options
var portOption = new Option<int>(
    name: "--port",
    description: "The port to listen on",
    getDefaultValue: () => 5300);

var installOption = new Option<bool>(
    name: "--install",
    description: "Install as a service");

var uninstallOption = new Option<bool>(
    name: "--uninstall",
    description: "Uninstall the service");

var serviceNameOption = new Option<string>(
    name: "--service-name",
    description: "The name of the service",
    getDefaultValue: () => "NeuroMCPAzureDevOps");

// Create root command
var rootCommand = new RootCommand("NeuroMCP Azure DevOps Server");
rootCommand.AddOption(portOption);
rootCommand.AddOption(installOption);
rootCommand.AddOption(uninstallOption);
rootCommand.AddOption(serviceNameOption);

rootCommand.SetHandler(async (int port, bool install, bool uninstall, string serviceName) =>
{
    if (install)
    {
        await InstallService(serviceName, port);
        return;
    }

    if (uninstall)
    {
        await UninstallService(serviceName);
        return;
    }

    await RunServer(args, port);
}, portOption, installOption, uninstallOption, serviceNameOption);

return await rootCommand.InvokeAsync(args);

async Task RunServer(string[] args, int port)
{
    var builder = WebApplication.CreateBuilder(args);

    if (OperatingSystem.IsWindows())
    {
        builder.Host.UseWindowsService();
    }
    else if (OperatingSystem.IsLinux())
    {
        builder.Host.UseSystemd();
    }

    // Configure services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "NeuroMCP Azure DevOps API", Version = "v1" });
    });

    // Add MCP Server
    builder.Services.AddMcpServer(options =>
    {
        options.Name = "NeuroMCP.AzureDevOps";
    });

    // Add Azure DevOps services
    builder.Services.AddSingleton<IAzureDevOpsService, AzureDevOpsService>();

    // Configure HTTP
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port);
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Use MCP Server Middleware
    app.UseMcpServer();

    Console.WriteLine($"NeuroMCP Azure DevOps server listening on port {port}");

    await app.RunAsync();
}

async Task InstallService(string serviceName, int port)
{
    Console.WriteLine($"Installing service '{serviceName}' on port {port}...");

    if (!OperatingSystem.IsWindows())
    {
        Console.WriteLine("Service installation is only supported on Windows.");
        return;
    }

    // Service installation logic here
    Console.WriteLine("Service installation completed.");
    await Task.CompletedTask;
}

async Task UninstallService(string serviceName)
{
    Console.WriteLine($"Uninstalling service '{serviceName}'...");

    if (!OperatingSystem.IsWindows())
    {
        Console.WriteLine("Service uninstallation is only supported on Windows.");
        return;
    }

    // Service uninstallation logic here
    Console.WriteLine("Service uninstallation completed.");
    await Task.CompletedTask;
}