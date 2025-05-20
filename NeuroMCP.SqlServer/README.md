# NeuroMCP SQL Server

A Model Context Protocol (MCP) server for interacting with SQL Server databases, enhanced with neural context capabilities.

## What is NeuroMCP?

NeuroMCP extends the Model Context Protocol (MCP) with neural network and semantic capabilities, designed to facilitate smarter communication between AI models and applications by providing a structured way to exchange context and data. This NeuroMCP server specifically provides tools for interacting with SQL Server databases with advanced AI context awareness.

## Features

- Execute SQL queries against any MSSQL database
- Test database connections
- Get information about the current database connection
- Supports both standard input/output and HTTP/SSE transport
- Enhanced semantic understanding of database schemas

## Getting Started

### Running Locally

1. Clone this repository
2. Configure your database connection in `appsettings.json` or use environment variables
3. Run the server with `dotnet run`

```bash
cd NeuroMCP.SqlServer
dotnet run -- --port 5200
```

### Using with VS Code

To use this NeuroMCP server with Visual Studio Code:

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
        "path/to/NeuroMCP.SqlServer.csproj",
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
docker build -t neuromcp-sqlserver .
docker run -p 5200:5200 -e MSSQL_SERVER=your-server -e MSSQL_USER=your-user -e MSSQL_PASSWORD=your-password -e MSSQL_DATABASE=your-database neuromcp-sqlserver
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
dotnet run -- --install --service-name NeuroMCPSqlServer --port 5200
```

### Linux

```bash
sudo dotnet run -- --install --service-name NeuroMCPSqlServer --port 5200
```

## Uninstalling the Service

```bash
dotnet run -- --uninstall --service-name NeuroMCPSqlServer
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
dotnet nuget push ./nupkg/NeuroMCP.SqlServer.1.0.0.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
```

## License

MIT 