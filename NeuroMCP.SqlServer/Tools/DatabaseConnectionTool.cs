using System.ComponentModel;
using NeuroMCP.SqlServer.Models;
using NeuroMCP.SqlServer.Services;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace NeuroMCP.SqlServer.Tools
{
    [McpServerToolType]
    public class DatabaseConnectionTool
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseConnectionTool(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [McpServerTool, Description("Get information about the current database connection")]
        public Dictionary<string, object?> GetConnectionInfo()
        {
            using var scope = _serviceProvider.CreateScope();
            var sqlService = scope.ServiceProvider.GetRequiredService<ISqlService>();

            // Mask sensitive information in connection string
            var connectionString = sqlService.GetConnectionString();
            var maskedConnectionString = MaskConnectionString(connectionString);

            return new Dictionary<string, object?>
            {
                ["connectionString"] = maskedConnectionString,
                ["server"] = Environment.GetEnvironmentVariable("MSSQL_SERVER") ?? "(not set)",
                ["database"] = Environment.GetEnvironmentVariable("MSSQL_DATABASE") ?? "(not set)",
                ["user"] = Environment.GetEnvironmentVariable("MSSQL_USER") ?? "(not set)"
            };
        }

        [McpServerTool, Description("Test a database connection with the provided configuration")]
        public async Task<Dictionary<string, object>> TestConnection(Dictionary<string, object> databaseConfig)
        {
            using var scope = _serviceProvider.CreateScope();
            var sqlService = scope.ServiceProvider.GetRequiredService<ISqlService>();

            var config = new DatabaseConfig();

            if (databaseConfig.TryGetValue("connectionString", out var connectionString) && connectionString is string connStr)
            {
                config.ConnectionString = connStr;
            }

            if (databaseConfig.TryGetValue("server", out var server) && server is string serverStr)
            {
                config.Server = serverStr;
            }

            if (databaseConfig.TryGetValue("database", out var database) && database is string dbStr)
            {
                config.Database = dbStr;
            }

            if (databaseConfig.TryGetValue("userId", out var userId) && userId is string userIdStr)
            {
                config.UserId = userIdStr;
            }

            if (databaseConfig.TryGetValue("password", out var password) && password is string passwordStr)
            {
                config.Password = passwordStr;
            }

            if (databaseConfig.TryGetValue("trustServerCertificate", out var trustCert) && trustCert is bool trustCertBool)
            {
                config.TrustServerCertificate = trustCertBool;
            }

            if (databaseConfig.TryGetValue("integratedSecurity", out var intSecurity) && intSecurity is bool intSecurityBool)
            {
                config.IntegratedSecurity = intSecurityBool;
            }

            // Test the connection with a simple query
            var request = new SqlQueryRequest { Query = "SELECT 1 AS TestConnection" };

            try
            {
                var result = await sqlService.ExecuteQueryWithConfigAsync(request, config);

                return new Dictionary<string, object>
                {
                    ["success"] = result.Success,
                    ["error"] = result.ErrorMessage ?? string.Empty,
                    ["connectionString"] = MaskConnectionString(config.GetEffectiveConnectionString())
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = ex.Message,
                    ["connectionString"] = MaskConnectionString(config.GetEffectiveConnectionString())
                };
            }
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return string.Empty;

            // Mask password
            var maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
                connectionString,
                @"Password=([^;]*)",
                "Password=********",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Mask user id
            maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
                maskedConnectionString,
                @"User Id=([^;]*)",
                "User Id=********",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return maskedConnectionString;
        }
    }
}