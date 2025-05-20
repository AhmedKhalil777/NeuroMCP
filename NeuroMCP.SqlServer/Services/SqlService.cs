using System.Data;
using NeuroMCP.SqlServer.Models;
using Microsoft.Data.SqlClient;

namespace NeuroMCP.SqlServer.Services
{
    public interface ISqlService
    {
        Task<SqlQueryResponse> ExecuteQueryAsync(SqlQueryRequest request);
        Task<SqlQueryResponse> ExecuteQueryWithConfigAsync(SqlQueryRequest request, DatabaseConfig config);
        string GetConnectionString();
    }

    public class SqlService : ISqlService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlService> _logger;
        private string _connectionString;

        public SqlService(IConfiguration configuration, ILogger<SqlService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Try to load from environment variables first
            var envConfig = LoadFromEnvironment();
            if (envConfig != null && !string.IsNullOrEmpty(envConfig.GetEffectiveConnectionString()))
            {
                _connectionString = envConfig.GetEffectiveConnectionString();
                _logger.LogInformation("Using connection string from environment variables");
                return;
            }

            // If environment variables not available, load from configuration
            var defaultConfig = new DatabaseConfig();
            configuration.GetSection("Database").Bind(defaultConfig);

            _connectionString = !string.IsNullOrEmpty(defaultConfig.ConnectionString)
                ? defaultConfig.ConnectionString
                : configuration.GetConnectionString("SqlServer") ?? "";

            // If still no connection string found, use a default one that will at least not crash
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogWarning("No SQL Server connection string found in configuration or environment variables. Using default localhost connection string.");
                _connectionString = "Server=localhost;Database=master;User Id=sa;Password=SqlServerPassword123;TrustServerCertificate=True;";
            }
        }

        private DatabaseConfig? LoadFromEnvironment()
        {
            string? server = Environment.GetEnvironmentVariable("MSSQL_SERVER");
            string? database = Environment.GetEnvironmentVariable("MSSQL_DATABASE");
            string? user = Environment.GetEnvironmentVariable("MSSQL_USER");
            string? password = Environment.GetEnvironmentVariable("MSSQL_PASSWORD");

            // Return null if essential variables are missing
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
            {
                return null;
            }

            return new DatabaseConfig
            {
                Server = server,
                Database = database,
                UserId = user,
                Password = password,
                TrustServerCertificate = true
            };
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public async Task<SqlQueryResponse> ExecuteQueryAsync(SqlQueryRequest request)
        {
            return await ExecuteQueryInternalAsync(request, _connectionString);
        }

        public async Task<SqlQueryResponse> ExecuteQueryWithConfigAsync(SqlQueryRequest request, DatabaseConfig config)
        {
            string connectionString = config.GetEffectiveConnectionString();
            return await ExecuteQueryInternalAsync(request, connectionString);
        }

        private async Task<SqlQueryResponse> ExecuteQueryInternalAsync(SqlQueryRequest request, string connectionString)
        {
            var response = new SqlQueryResponse { Success = false };

            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand(request.Query, connection);

                if (request.Parameters != null)
                {
                    foreach (var param in request.Parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                await connection.OpenAsync();

                // Determine if the query is a SELECT statement or DML (INSERT/UPDATE/DELETE)
                var isSelect = request.Query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);

                if (isSelect)
                {
                    using var reader = await command.ExecuteReaderAsync();
                    var results = new List<Dictionary<string, object?>>();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            row[columnName] = value;
                        }
                        results.Add(row);
                    }

                    response.Results = results;
                    response.RowsAffected = results.Count;
                }
                else
                {
                    response.RowsAffected = await command.ExecuteNonQueryAsync();
                }

                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL query: {Query}", request.Query);
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}