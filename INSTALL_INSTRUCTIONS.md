# NeuroMCP SQL Server Installation Instructions

## Installation Options

### Install from NuGet (Global)
```
dotnet tool install --global NeuroMCP.SqlServer
```

### Install from Local Package
```
dotnet tool install --global --add-source ./NeuroMCP.SqlServer/nupkg NeuroMCP.SqlServer
```

### Update Existing Installation
```
dotnet tool update --global NeuroMCP.SqlServer
```

## Running the NeuroMCP SQL Server

### Run as a Command Line Tool
```
neuromcp-mssql --port 5200
```

### Run as a Windows Service
```
neuromcp-mssql --install --service-name NeuroMCPSqlServer --port 5200
```

### Run as a Linux Service
```
sudo neuromcp-mssql --install --service-name NeuroMCPSqlServer --port 5200
```

## Configuration

### Environment Variables
The tool accepts the following environment variables:
- `MSSQL_SERVER`: SQL Server hostname
- `MSSQL_DATABASE`: Database name
- `MSSQL_USER`: Username
- `MSSQL_PASSWORD`: Password

### Configure NeuroMCP in Cursor
Edit `mcp.json` in your project root:

```json
{
  "servers": {
    "mssql": {
      "type": "http",
      "url": "http://localhost:5200/mcp",
      "formatVersion": "2",
      "rpcVersion": "2.0",
      "env": {
        "MSSQL_SERVER": "your-server-name",
        "MSSQL_USER": "your-username",
        "MSSQL_PASSWORD": "your-password",
        "MSSQL_DATABASE": "your-database-name"
      }
    }
  }
}
```

## NeuroMCP SQL Server Capabilities

The NeuroMCP SQL Server tool supports the following operations:

1. **executeSql** - Execute SQL queries against the database
2. **testConnection** - Test connection to the database
3. **getDatabaseInfo** - Get current database connection information
4. **listTables** - List all tables in the database with schema, row count and size

## Troubleshooting

### CORS Issues
The tool includes CORS fixes. If you encounter CORS errors, ensure:
1. You're using version 1.1.0 or later
2. The server is accessible from the client
3. The URL in mcp.json points to the /mcp endpoint

### Connection Errors
If the SQL connection fails:
1. Verify your SQL Server credentials
2. Check if the SQL Server is running and accessible
3. Ensure your firewall allows connections to the SQL Server port

### Cursor MCP Integration Issues
If Cursor doesn't discover or connect to the NeuroMCP server:
1. Ensure the NeuroMCP server is running (`neuromcp-mssql --port 5200`)
2. Verify your mcp.json has the correct format with `formatVersion` and `rpcVersion` fields
3. Check that the URL is pointing to http://localhost:5200/mcp
4. Restart Cursor after making changes to mcp.json

### Testing the NeuroMCP Server

#### Using PowerShell
```powershell
# Test the getTools method
$body = @{
    jsonrpc = '2.0'
    id = '1'
    method = 'getTools'
    params = @{}
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri 'http://localhost:5200/mcp' -Body $body -ContentType 'application/json' | ConvertTo-Json -Depth 5

# Test the executeSql method
$body = @{
    jsonrpc = '2.0'
    id = '2'
    method = 'executeSql'
    params = @{
        query = 'SELECT @@VERSION AS SqlVersion'
    }
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri 'http://localhost:5200/mcp' -Body $body -ContentType 'application/json' | ConvertTo-Json -Depth 5
``` 