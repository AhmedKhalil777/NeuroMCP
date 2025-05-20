namespace NeuroMCP.SqlServer.Models
{
    public class SqlQueryRequest
    {
        public string Query { get; set; } = string.Empty;
        public Dictionary<string, object?>? Parameters { get; set; }
    }

    public class SqlQueryResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Dictionary<string, object?>>? Results { get; set; }
        public int RowsAffected { get; set; }
    }
}