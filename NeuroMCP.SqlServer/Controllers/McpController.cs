using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Reflection;
using System.Text.Json.Nodes;
using ModelContextProtocol.Server;
using NeuroMCP.SqlServer.Services;
using NeuroMCP.SqlServer.Models;

namespace NeuroMCP.SqlServer.Controllers
{
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly ILogger<McpController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISqlService _sqlService;

        public McpController(IServiceProvider serviceProvider, ILogger<McpController> logger, ISqlService sqlService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _sqlService = sqlService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessMcpRequest([FromBody] JsonDocument requestBody)
        {
            try
            {
                _logger.LogInformation("Received MCP request: {Request}", requestBody.RootElement.ToString());

                // Parse the request to extract the required information
                string id = "0";
                string method = "";
                JsonElement paramsElement = new JsonElement();

                if (requestBody.RootElement.TryGetProperty("id", out var idElement))
                {
                    id = idElement.ToString();
                }

                if (requestBody.RootElement.TryGetProperty("method", out var methodElement))
                {
                    method = methodElement.GetString() ?? "";
                }

                if (requestBody.RootElement.TryGetProperty("params", out var foundParams))
                {
                    paramsElement = foundParams;
                }

                // Build the response based on the method
                if (string.IsNullOrEmpty(method))
                {
                    return BadRequest(new
                    {
                        id,
                        error = new
                        {
                            code = -32600,
                            message = "Invalid Request: no method specified"
                        },
                        jsonrpc = "2.0"
                    });
                }

                // Process method based on the name
                switch (method)
                {
                    case "initialize":
                        return HandleInitialize(id, paramsElement);

                    case "getTools":
                        return Ok(new
                        {
                            id,
                            result = new
                            {
                                tools = new object[]
                                {
                                    new
                                    {
                                        name = "executeSql",
                                        description = "Execute an SQL query on the MSSQL server",
                                        parameters = new
                                        {
                                            type = "object",
                                            properties = new
                                            {
                                                query = new { type = "string", description = "SQL query to execute" },
                                                parameters = new { type = "object", description = "Query parameters" },
                                            },
                                            required = new string[] { "query" }
                                        }
                                    },
                                    new
                                    {
                                        name = "testConnection",
                                        description = "Test the database connection",
                                        parameters = new
                                        {
                                            type = "object",
                                            properties = new
                                            {
                                                connectionString = new { type = "string", description = "Optional connection string" }
                                            }
                                        }
                                    },
                                    new
                                    {
                                        name = "getDatabaseInfo",
                                        description = "Get information about the current database connection",
                                        parameters = new
                                        {
                                            type = "object",
                                            properties = new { }
                                        }
                                    },
                                    new
                                    {
                                        name = "listTables",
                                        description = "List all tables in the database",
                                        parameters = new
                                        {
                                            type = "object",
                                            properties = new { }
                                        }
                                    }
                                }
                            },
                            jsonrpc = "2.0"
                        });

                    case "executeSql":
                        return await ExecuteSql(id, paramsElement);

                    case "testConnection":
                        return await TestConnection(id, paramsElement);

                    case "getDatabaseInfo":
                        return GetDatabaseInfo(id);

                    case "listTables":
                        return await ListTables(id);

                    default:
                        return BadRequest(new
                        {
                            id,
                            error = new
                            {
                                code = -32601,
                                message = $"Method '{method}' not found"
                            },
                            jsonrpc = "2.0"
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MCP request");
                return StatusCode(500, new
                {
                    id = "0",
                    error = new
                    {
                        code = -32603,
                        message = "Internal error: " + ex.Message
                    },
                    jsonrpc = "2.0"
                });
            }
        }

        private async Task<IActionResult> ExecuteSql(string id, JsonElement paramsElement)
        {
            try
            {
                string query = "";
                Dictionary<string, object?>? parameters = null;

                if (paramsElement.TryGetProperty("query", out var queryElement))
                {
                    query = queryElement.GetString() ?? "";
                }

                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new
                    {
                        id,
                        error = new { code = -32602, message = "Invalid params: query is required" },
                        jsonrpc = "2.0"
                    });
                }

                if (paramsElement.TryGetProperty("parameters", out var paramsObj))
                {
                    parameters = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        paramsObj.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                var request = new SqlQueryRequest
                {
                    Query = query,
                    Parameters = parameters
                };

                var result = await _sqlService.ExecuteQueryAsync(request);

                return Ok(new
                {
                    id,
                    result = new
                    {
                        success = result.Success,
                        error = result.ErrorMessage,
                        data = result.Results,
                        rowsAffected = result.RowsAffected
                    },
                    jsonrpc = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL");
                return BadRequest(new
                {
                    id,
                    error = new { code = -32000, message = "SQL Error: " + ex.Message },
                    jsonrpc = "2.0"
                });
            }
        }

        private async Task<IActionResult> TestConnection(string id, JsonElement paramsElement)
        {
            try
            {
                string? connectionString = null;

                if (paramsElement.TryGetProperty("connectionString", out var connStrElement))
                {
                    connectionString = connStrElement.GetString();
                }

                DatabaseConfig config = new DatabaseConfig();

                if (!string.IsNullOrEmpty(connectionString))
                {
                    config.ConnectionString = connectionString;
                }

                // Test the connection with a simple query
                var request = new SqlQueryRequest { Query = "SELECT 1 AS TestConnection" };
                var result = await _sqlService.ExecuteQueryWithConfigAsync(request, config);

                return Ok(new
                {
                    id,
                    result = new
                    {
                        success = result.Success,
                        error = result.ErrorMessage,
                        connectionString = MaskConnectionString(config.GetEffectiveConnectionString())
                    },
                    jsonrpc = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection");
                return BadRequest(new
                {
                    id,
                    error = new { code = -32000, message = "Connection Error: " + ex.Message },
                    jsonrpc = "2.0"
                });
            }
        }

        private IActionResult GetDatabaseInfo(string id)
        {
            try
            {
                // Get the connection string but mask sensitive information
                var connectionString = _sqlService.GetConnectionString();
                var maskedConnectionString = MaskConnectionString(connectionString);

                // Get database info from environment variables
                var server = Environment.GetEnvironmentVariable("MSSQL_SERVER") ?? "(not set)";
                var database = Environment.GetEnvironmentVariable("MSSQL_DATABASE") ?? "(not set)";
                var user = Environment.GetEnvironmentVariable("MSSQL_USER") ?? "(not set)";

                return Ok(new
                {
                    id,
                    result = new
                    {
                        connectionString = maskedConnectionString,
                        server,
                        database,
                        user
                    },
                    jsonrpc = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info");
                return BadRequest(new
                {
                    id,
                    error = new { code = -32000, message = "Database Info Error: " + ex.Message },
                    jsonrpc = "2.0"
                });
            }
        }

        private async Task<IActionResult> ListTables(string id)
        {
            try
            {
                // Query to list all tables
                var query = @"
                    SELECT 
                        t.name AS TableName,
                        s.name AS SchemaName,
                        p.rows AS RowCount,
                        CONVERT(VARCHAR(50), CONVERT(DECIMAL(19,0), SUM(a.total_pages) * 8)) + ' KB' AS TotalSpace
                    FROM 
                        sys.tables t
                    INNER JOIN      
                        sys.schemas s ON s.schema_id = t.schema_id
                    INNER JOIN 
                        sys.indexes i ON t.object_id = i.object_id
                    INNER JOIN 
                        sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
                    INNER JOIN 
                        sys.allocation_units a ON p.partition_id = a.container_id
                    WHERE 
                        t.is_ms_shipped = 0 AND i.object_id > 255 
                    GROUP BY 
                        t.name, s.name, p.rows
                    ORDER BY 
                        s.name, t.name";

                var request = new SqlQueryRequest { Query = query };
                var result = await _sqlService.ExecuteQueryAsync(request);

                return Ok(new
                {
                    id,
                    result = new
                    {
                        success = result.Success,
                        tables = result.Results,
                        error = result.ErrorMessage
                    },
                    jsonrpc = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing tables");
                return BadRequest(new
                {
                    id,
                    error = new { code = -32000, message = "Table Listing Error: " + ex.Message },
                    jsonrpc = "2.0"
                });
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

        private IActionResult HandleInitialize(string id, JsonElement paramsElement)
        {
            // Extract protocol version from params if needed
            string protocolVersion = "2025-03-26";
            if (paramsElement.TryGetProperty("protocolVersion", out var versionElement))
            {
                protocolVersion = versionElement.GetString() ?? protocolVersion;
            }

            // Return server capabilities
            return Ok(new
            {
                id,
                result = new
                {
                    protocolVersion = protocolVersion,
                    capabilities = new
                    {
                        tools = true,
                        prompts = false,
                        resources = false,
                        logging = false
                    },
                    serverInfo = new
                    {
                        name = "MCP.SqlServer",
                        version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
                    }
                },
                jsonrpc = "2.0"
            });
        }

        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "ok",
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
            });
        }
    }
}