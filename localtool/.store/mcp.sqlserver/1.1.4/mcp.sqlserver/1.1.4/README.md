# MCP SQL Server

A Model Context Protocol (MCP) server for interacting with SQL Server databases.

## What is MCP?

The Model Context Protocol (MCP) is a standardized protocol designed to facilitate communication between AI models and applications by providing a structured way to exchange context and data. This MCP server specifically provides tools for interacting with SQL Server databases.

## Features

- Execute SQL queries against any MSSQL database
- Test database connections
- Get information about the current database connection
- Supports both standard input/output and HTTP/SSE transport

## Getting Started

### Running Locally

1. Clone this repository
2. Configure your database connection in `appsettings.json` or use environment variables
3. Run the server with `dotnet run`

```bash
cd MCP.SqlServer
dotnet run -- --port 5200
```

### Using with VS Code

To use this MCP server with Visual Studio Code:

1. Create or update your `.vscode/mcp.json` file:

```json
{
  "servers": {
    "mssql": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "path/to/MCP.SqlServer.csproj",
        "--",
        "--port",
        "5200"
      ],
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

2. Open VS Code and enable GitHub Copilot
3. Toggle on Agent mode in GitHub Copilot
4. You should see the SQL Server tools available in the dropdown

### Using Docker

You can also run the server using Docker:

```bash
docker-compose up
```

Or build and run the Docker image directly:

```bash
docker build -t mcp-sqlserver .
docker run -p 5200:5200 -e MSSQL_SERVER=your-server -e MSSQL_USER=your-user -e MSSQL_PASSWORD=your-password -e MSSQL_DATABASE=your-database mcp-sqlserver
```

## Available Tools

### SqlQueryTool

Execute SQL queries against an MSSQL database.

- `ExecuteSql(query, parameters, databaseConfig)`

### DatabaseConnectionTool

Tools for working with database connections.

- `GetConnectionInfo()`
- `TestConnection(databaseConfig)`

## Configuration

### Environment Variables

- `MSSQL_SERVER`: SQL Server hostname or IP address
- `MSSQL_DATABASE`: Database name
- `MSSQL_USER`: SQL Server username
- `MSSQL_PASSWORD`: SQL Server password

### Connection String

Alternatively, you can specify a full connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=your-server;Database=your-database;User Id=your-user;Password=your-password;TrustServerCertificate=True;"
  }
}
```

## Installing as a Service

### Windows

```bash
dotnet run -- --install --service-name MCPSqlServer --port 5200
```

### Linux

```bash
sudo dotnet run -- --install --service-name MCPSqlServer --port 5200
```

## Uninstalling the Service

```bash
dotnet run -- --uninstall --service-name MCPSqlServer
```

## Development

### Prerequisites

- .NET 8.0 SDK or later

### Build

```bash
dotnet build
```

### Pack

```bash
dotnet pack
```

### Publish

```bash
dotnet nuget push ./nupkg/MCP.SqlServer.1.0.0.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
```

## License

MIT 