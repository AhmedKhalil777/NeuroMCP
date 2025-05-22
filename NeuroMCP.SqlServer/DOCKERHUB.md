# NeuroMCP.SqlServer

## Overview

NeuroMCP.SqlServer is a Model Context Protocol (MCP) server for interacting with Microsoft SQL Server databases. It provides a standardized API that allows AI agents to execute SQL queries, explore database schemas, and perform database operations.

## Quick Start

```bash
# Pull the image
docker pull ahmedkhalil777/neuromcp-sqlserver:latest

# Run with default settings
docker run -p 5200:5200 ahmedkhalil777/neuromcp-sqlserver

# Run with custom configuration
docker run -p 5200:5200 \
  -v /path/to/config:/app/config \
  ahmedkhalil777/neuromcp-sqlserver
```

## Configuration

The server requires SQL Server connection details. You can provide these in several ways:

### 1. Using a Configuration File

Create an `appsettings.json` file in a local directory and mount it:

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

### 2. Using Environment Variables

```bash
docker run -p 5200:5200 \
  -e DB_SERVER="your-sql-server" \
  -e DB_NAME="your-database" \
  -e DB_USER="your-username" \
  -e DB_PASSWORD="your-password" \
  -e DB_TRUST_SERVER_CERTIFICATE=true \
  ahmedkhalil777/neuromcp-sqlserver
```

## Docker Compose with SQL Server

Here's an example that spins up both the MCP server and a SQL Server instance:

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

## Available Tools

This MCP server provides tools for:

- Retrieving connection information
- Executing SQL queries with parameters
- Testing database connections
- Exploring database schemas

## Security Notes

- Never embed database credentials directly in images or public repositories
- Use volume mounts or environment variables for configuration
- Create database users with minimal required permissions

## Source Code

The source code for this project is available on GitHub: [AhmedKhalil777/NeuroMCP](https://github.com/AhmedKhalil777/NeuroMCP)

## Documentation

For more detailed documentation, please see:
- [GitHub Project README](https://github.com/AhmedKhalil777/NeuroMCP/blob/main/README.md)
- [Docker Setup Guide](https://github.com/AhmedKhalil777/NeuroMCP/blob/main/DOCKER-PUBLISH.md) 