using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeuroMCP.SqlServer.Services;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.Reflection;

// Parse command line options
var rootCommand = new RootCommand("NeuroMCP SQL Server Tool");

var portOption = new Option<int>(
    name: "--port",
    description: "The port to listen on for HTTP requests",
    getDefaultValue: () => 5200);
rootCommand.AddOption(portOption);

var installOption = new Option<bool>(
    name: "--install",
    description: "Install as a service");
rootCommand.AddOption(installOption);

var uninstallOption = new Option<bool>(
    name: "--uninstall",
    description: "Uninstall the service");
rootCommand.AddOption(uninstallOption);

var serviceNameOption = new Option<string>(
    name: "--service-name",
    description: "Name of the service (when installing)",
    getDefaultValue: () => "NeuroMCPSqlServer");
rootCommand.AddOption(serviceNameOption);

rootCommand.SetHandler(async (int port, bool install, bool uninstall, string serviceName) =>
{
    // Handle service installation/uninstallation
    if (install)
    {
        InstallService(serviceName, port);
        return;
    }

    if (uninstall)
    {
        UninstallService(serviceName);
        return;
    }

    // If not installing/uninstalling, run the server
    await RunServer(args, port);
}, portOption, installOption, uninstallOption, serviceNameOption);

return await rootCommand.InvokeAsync(args);

async Task RunServer(string[] args, int port)
{
    // Create the builder with command line args
    var builder = WebApplication.CreateBuilder(args);

    // Configure as a Windows service or Linux systemd service
    builder.Host.UseWindowsService();
    builder.Host.UseSystemd();

    // Configure logging
    builder.Logging.AddConsole(consoleLogOptions =>
    {
        // Configure all logs to go to stderr
        consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
    });

    // Configure server URL with the specified port
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    // Add controllers and API explorer
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add CORS support
    builder.Services.AddCors();

    // Register HTTP client factory
    builder.Services.AddHttpClient();

    // Register our services
    builder.Services.AddScoped<ISqlService, SqlService>();

    // Register our MCP server
    // First define the MCP server with STDIO transport
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport() // Support standard input/output for VS Code and tools
        .WithToolsFromAssembly(); // Auto-register all tools in the assembly

    // Build the app
    var app = builder.Build();

    // Configure middleware
    app.UseSwagger();
    app.UseSwaggerUI();

    // Enable CORS
    app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Show startup message with port and version info
    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    app.Logger.LogInformation($"NeuroMCP SQL Server v{version} starting on port {port}");
    app.Logger.LogInformation($"NeuroMCP server endpoints available at:");
    app.Logger.LogInformation($"  - STDIO: Standard input/output");

    // Run the application
    await app.RunAsync();
}

void InstallService(string serviceName, int port)
{
    Console.WriteLine($"Installing service {serviceName} on port {port}...");

    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
    if (string.IsNullOrEmpty(exePath))
    {
        Console.Error.WriteLine("Failed to get executable path");
        return;
    }

    // Generate a configuration file for the service
    var configPath = Path.Combine(
        Path.GetDirectoryName(exePath) ?? ".",
        $"{serviceName}.config.json");

    File.WriteAllText(configPath, $@"{{
  ""Port"": {port}
}}");

    if (OperatingSystem.IsWindows())
    {
        InstallWindowsService(serviceName, exePath, port);
    }
    else if (OperatingSystem.IsLinux())
    {
        InstallLinuxService(serviceName, exePath, port);
    }
    else
    {
        Console.Error.WriteLine("Service installation is only supported on Windows and Linux");
    }
}

void InstallWindowsService(string serviceName, string exePath, int port)
{
    var psi = new System.Diagnostics.ProcessStartInfo
    {
        FileName = "sc",
        Arguments = $"create {serviceName} binPath= \"\"{exePath}\" --port {port}\" start= auto DisplayName= \"NeuroMCP SQL Server\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    var process = System.Diagnostics.Process.Start(psi);
    process?.WaitForExit();

    if (process?.ExitCode == 0)
    {
        Console.WriteLine($"Service {serviceName} has been installed successfully");
    }
    else
    {
        Console.Error.WriteLine($"Failed to install service: {process?.StandardError.ReadToEnd()}");
    }
}

void InstallLinuxService(string serviceName, string exePath, int port)
{
    var serviceContent = $@"[Unit]
Description=NeuroMCP SQL Server
After=network.target

[Service]
ExecStart={exePath} --port {port}
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
";

    var servicePath = $"/etc/systemd/system/{serviceName}.service";

    try
    {
        File.WriteAllText(servicePath, serviceContent);
        Console.WriteLine($"Created service file at {servicePath}");

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "systemctl",
            Arguments = "daemon-reload",
            UseShellExecute = false
        };
        System.Diagnostics.Process.Start(psi)?.WaitForExit();

        psi.Arguments = $"enable {serviceName}";
        System.Diagnostics.Process.Start(psi)?.WaitForExit();

        Console.WriteLine($"Service {serviceName} has been installed and enabled");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to install service: {ex.Message}");
    }
}

void UninstallService(string serviceName)
{
    Console.WriteLine($"Uninstalling service {serviceName}...");

    if (OperatingSystem.IsWindows())
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "sc",
            Arguments = $"delete {serviceName}",
            UseShellExecute = false
        };

        var process = System.Diagnostics.Process.Start(psi);
        process?.WaitForExit();

        if (process?.ExitCode == 0)
        {
            Console.WriteLine($"Service {serviceName} has been uninstalled successfully");
        }
        else
        {
            Console.Error.WriteLine("Failed to uninstall service");
        }
    }
    else if (OperatingSystem.IsLinux())
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = $"disable {serviceName}",
                UseShellExecute = false
            };
            System.Diagnostics.Process.Start(psi)?.WaitForExit();

            psi.Arguments = $"stop {serviceName}";
            System.Diagnostics.Process.Start(psi)?.WaitForExit();

            var servicePath = $"/etc/systemd/system/{serviceName}.service";
            if (File.Exists(servicePath))
            {
                File.Delete(servicePath);
            }

            psi.Arguments = "daemon-reload";
            System.Diagnostics.Process.Start(psi)?.WaitForExit();

            Console.WriteLine($"Service {serviceName} has been uninstalled successfully");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to uninstall service: {ex.Message}");
        }
    }
    else
    {
        Console.Error.WriteLine("Service uninstallation is only supported on Windows and Linux");
    }
}
