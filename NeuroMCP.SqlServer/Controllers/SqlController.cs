using Microsoft.AspNetCore.Mvc;
using NeuroMCP.SqlServer.Models;
using NeuroMCP.SqlServer.Services;
using System.Threading.Tasks;

namespace NeuroMCP.SqlServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SqlController : ControllerBase
    {
        private readonly ISqlService _sqlService;
        private readonly ILogger<SqlController> _logger;

        public SqlController(ISqlService sqlService, ILogger<SqlController> logger)
        {
            _sqlService = sqlService;
            _logger = logger;
        }

        [HttpPost("query")]
        public async Task<IActionResult> ExecuteQuery([FromBody] SqlQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { Error = "Query cannot be empty" });
            }

            try
            {
                var result = await _sqlService.ExecuteQueryAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("connection")]
        public IActionResult GetConnectionInfo()
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
                    ConnectionString = maskedConnectionString,
                    Server = server,
                    Database = database,
                    User = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection info");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] DatabaseConfig config)
        {
            try
            {
                // Test the connection with a simple query
                var request = new SqlQueryRequest { Query = "SELECT 1 AS TestConnection" };
                var result = await _sqlService.ExecuteQueryWithConfigAsync(request, config);

                return Ok(new
                {
                    Success = result.Success,
                    Error = result.ErrorMessage,
                    ConnectionString = MaskConnectionString(config.GetEffectiveConnectionString())
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection");
                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message,
                    ConnectionString = MaskConnectionString(config.GetEffectiveConnectionString())
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
    }
}