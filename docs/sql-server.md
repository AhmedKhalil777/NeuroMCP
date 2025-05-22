# NeuroMCP.SqlServer

![SQL Server Logo](images/sqlserver-logo.png)

NeuroMCP.SqlServer is a Model Context Protocol (MCP) server for interacting with Microsoft SQL Server databases. It provides a standardized API that allows AI agents to execute SQL queries, explore database schemas, and perform database operations.

## Features

- **SQL Query Execution**: Run SQL queries with parameters
- **Connection Management**: Test and manage database connections
- **Schema Exploration**: Explore database tables and schema information
- **Query Parameterization**: Support for parameterized queries for security
- **Transaction Support**: Execute queries within transactions
- **Error Handling**: Robust error handling and reporting

## Installation

### Docker Installation

The easiest way to run NeuroMCP.SqlServer is using Docker:

```bash
docker pull ahmedkhalil777/neuromcp-sqlserver:latest
docker run -p 5200:5200 ahmedkhalil777/neuromcp-sqlserver
```

For more details, see the [Docker Installation](docker-installation.md) guide.

### Local Installation

To install NeuroMCP.SqlServer as a .NET tool:

```bash
dotnet tool install --global NeuroMCP.SqlServer
neuromcp-mssql --port 5200
```

For more details, see the [Local Installation](local-installation.md) guide.

## Configuration

### SQL Server Connection

NeuroMCP.SqlServer requires SQL Server connection details. You can provide these through an `appsettings.json` file:

```json
{
  "Database": {
    "Server": "your-sql-server",
    "Database": "your-database",
    "UserId": "your-username",
    "Password": "your-password",
    "TrustServerCertificate": true,
    "ApplicationName": "NeuroMCP.SqlServer",
    "ConnectionTimeout": 30
  }
}
```

You can also use connection strings directly:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=master;User Id=sa;Password=Password123;TrustServerCertificate=True;"
  }
}
```

For more detailed configuration options, see the [SQL Server Configuration](sqlserver-configuration.md) guide.

## API Reference

NeuroMCP.SqlServer implements the Model Context Protocol (MCP) and provides the following tools:

| Tool Name | Description |
|-----------|-------------|
| `SIT-MSSQL_GetConnectionInfo` | Get information about the current database connection |
| `SIT-MSSQL_ExecuteSql` | Execute an SQL query with optional parameters |
| `SIT-MSSQL_TestConnection` | Test a database connection with provided configuration |

For more details on each tool, see the [SQL Server API Reference](sqlserver-api-reference.md).

## Examples

### Executing an SQL Query

```json
{
  "method": "SIT-MSSQL_ExecuteSql",
  "params": {
    "query": "SELECT * FROM Customers WHERE CustomerID = @CustomerID",
    "parameters": {
      "CustomerID": "ALFKI"
    }
  }
}
```

### Testing a Connection

```json
{
  "method": "SIT-MSSQL_TestConnection",
  "params": {
    "databaseConfig": {
      "Server": "localhost",
      "Database": "Northwind",
      "UserId": "sa",
      "Password": "Password123",
      "TrustServerCertificate": true
    }
  }
}
```

For more examples, see the [SQL Server Examples](sqlserver-examples.md) guide.

## Security Considerations

- Always use parameterized queries to prevent SQL injection
- Create database users with minimal required permissions
- Store connection credentials securely and never in source code
- Consider using environment variables or a secrets manager for password storage
- Use encryption for data in transit (TLS)

For more security best practices, see the [Security Best Practices](security.md) guide.

## Docker Integration

NeuroMCP.SqlServer can be run alongside a SQL Server container:

```yaml
version: '3.8'

services:
  neuromcp-sqlserver:
    image: ahmedkhalil777/neuromcp-sqlserver:latest
    ports:
      - "5200:5200"
    environment:
      - DB_SERVER=sqlserver
      - DB_NAME=master
      - DB_USER=sa
      - DB_PASSWORD=YourStrongPassword123
      - DB_TRUST_SERVER_CERTIFICATE=true
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrongPassword123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

volumes:
  sqlserver-data:
```

For more details, see the [Docker Compose](docker-compose.md) guide.

## Troubleshooting

Common issues and their solutions:

- **Connection Failures**: Check server name, credentials, and network connectivity
- **Authentication Issues**: Verify SQL Server authentication mode and credentials
- **Permission Errors**: Ensure the database user has appropriate permissions
- **Query Timeouts**: Check query complexity and server performance

For more troubleshooting help, see the [Common Issues](common-issues.md) guide.

## Related Documentation

- [Docker Hub](docker-hub.md) - Docker Hub repository information
- [API Reference](sqlserver-api-reference.md) - Detailed API documentation
- [Configuration Guide](sqlserver-configuration.md) - Advanced configuration options 