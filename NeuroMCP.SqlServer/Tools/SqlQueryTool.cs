using System.ComponentModel;
using NeuroMCP.SqlServer.Models;
using NeuroMCP.SqlServer.Services;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace NeuroMCP.SqlServer.Tools
{
    [McpServerToolType]
    public class SqlQueryTool
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlQueryTool(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [McpServerTool, Description("Execute an SQL query on the MSSQL server")]
        public async Task<Dictionary<string, object?>> ExecuteSql(string query, Dictionary<string, object?>? parameters = null, Dictionary<string, object?>? databaseConfig = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var sqlService = scope.ServiceProvider.GetRequiredService<ISqlService>();

            var request = new SqlQueryRequest
            {
                Query = query,
                Parameters = parameters
            };

            SqlQueryResponse result;

            if (databaseConfig != null)
            {
                var config = ConvertToDatabaseConfig(databaseConfig);
                result = await sqlService.ExecuteQueryWithConfigAsync(request, config);
            }
            else
            {
                result = await sqlService.ExecuteQueryAsync(request);
            }

            return new Dictionary<string, object?>
            {
                ["success"] = result.Success,
                ["error"] = result.ErrorMessage,
                ["data"] = result.Results,
                ["rowsAffected"] = result.RowsAffected
            };
        }

        private DatabaseConfig ConvertToDatabaseConfig(Dictionary<string, object?> configDict)
        {
            var config = new DatabaseConfig();

            if (configDict.TryGetValue("connectionString", out var connectionString) && connectionString is string connStr)
            {
                config.ConnectionString = connStr;
            }

            if (configDict.TryGetValue("server", out var server) && server is string serverStr)
            {
                config.Server = serverStr;
            }

            if (configDict.TryGetValue("database", out var database) && database is string dbStr)
            {
                config.Database = dbStr;
            }

            if (configDict.TryGetValue("userId", out var userId) && userId is string userIdStr)
            {
                config.UserId = userIdStr;
            }

            if (configDict.TryGetValue("password", out var password) && password is string passwordStr)
            {
                config.Password = passwordStr;
            }

            if (configDict.TryGetValue("trustServerCertificate", out var trustCert) && trustCert is bool trustCertBool)
            {
                config.TrustServerCertificate = trustCertBool;
            }

            if (configDict.TryGetValue("integratedSecurity", out var intSecurity) && intSecurity is bool intSecurityBool)
            {
                config.IntegratedSecurity = intSecurityBool;
            }

            return config;
        }
    }
}