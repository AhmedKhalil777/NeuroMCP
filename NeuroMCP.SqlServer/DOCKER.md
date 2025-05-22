# Docker Guide for NeuroMCP.SqlServer

This guide explains how to build, run, and deploy the NeuroMCP.SqlServer service using Docker.

## Prerequisites

- Docker installed on your machine
- Docker Hub account (for publishing)

## Building the Docker Image

To build the Docker image locally:

```bash
docker build -t neuromcp-sqlserver .
```

## Running the Docker Container

Run the container with default settings:

```bash
docker run -p 5200:5200 neuromcp-sqlserver
```

### Using Environment Variables

You can configure the database connection using environment variables:

```bash
docker run -p 5200:5200 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DB_SERVER=your-sql-server \
  -e DB_NAME=your-database \
  -e DB_USER=your-username \
  -e DB_PASSWORD=your-password \
  -e DB_TRUST_SERVER_CERTIFICATE=true \
  neuromcp-sqlserver
```

### Using Volume Mounts for Configuration

Mount a configuration directory to provide custom settings:

```bash
docker run -p 5200:5200 \
  -v /path/to/config:/app/config \
  neuromcp-sqlserver
```

Create an `appsettings.json` file in your local config directory with your SQL Server settings:

```json
{
  "Database": {
    "Server": "your-sql-server",
    "Database": "your-database",
    "UserId": "your-username",
    "Password": "your-password",
    "TrustServerCertificate": true
  }
}
```

## Using Docker Compose

For easier development, you can use Docker Compose:

```bash
docker-compose up
```

To run in detached mode:

```bash
docker-compose up -d
```

## Publishing to Docker Hub

1. Tag your image:

```bash
docker tag neuromcp-sqlserver yourusername/neuromcp-sqlserver:latest
```

2. Log in to Docker Hub:

```bash
docker login
```

3. Push the image:

```bash
docker push yourusername/neuromcp-sqlserver:latest
```

## Using the Docker Hub Image

Once published, you can use the image directly:

```bash
docker pull yourusername/neuromcp-sqlserver:latest
docker run -p 5200:5200 yourusername/neuromcp-sqlserver:latest
```

## Connecting to SQL Server

The NeuroMCP.SqlServer container needs to connect to a SQL Server instance. You can:

1. Connect to an existing SQL Server instance by providing the connection details
2. Run SQL Server in Docker alongside this container

Example docker-compose.yml with SQL Server:

```yaml
version: '3.8'

services:
  neuromcp-sqlserver:
    image: yourusername/neuromcp-sqlserver:latest
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

## Security Considerations

- Never store database credentials in your Docker image
- Use environment variables or volume mounts to provide configuration
- Consider using Docker secrets for production environments
- Ensure your SQL Server instance is properly secured 