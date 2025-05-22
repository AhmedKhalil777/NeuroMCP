# Docker Installation Guide

This guide explains how to install and run NeuroMCP services using Docker containers.

## Prerequisites

- Docker installed on your machine ([Install Docker](https://docs.docker.com/get-docker/))
- Basic understanding of Docker commands
- Network access to Docker Hub

## Quick Start

### NeuroMCP.AzureDevOps

```bash
# Pull the image
docker pull ahmedkhalil777/neuromcp-azuredevops:latest

# Run with default settings
docker run -p 5300:5300 ahmedkhalil777/neuromcp-azuredevops
```

### NeuroMCP.SqlServer

```bash
# Pull the image
docker pull ahmedkhalil777/neuromcp-sqlserver:latest

# Run with default settings
docker run -p 5200:5200 ahmedkhalil777/neuromcp-sqlserver
```

## Configuration

### Using Volume Mounts

The recommended way to configure NeuroMCP services is by mounting a configuration directory:

#### For NeuroMCP.AzureDevOps:

1. Create a local directory for configuration:
   ```bash
   mkdir -p ./config
   ```

2. Create an `appsettings.json` file in the config directory:
   ```json
   {
     "azureDevOps": {
       "orgUrl": "https://dev.azure.com/your-organization",
       "defaultProject": "your-project",
       "authentication": {
         "type": "pat",
         "patToken": "your-pat-token"
       }
     }
   }
   ```

3. Run the container with the mounted configuration:
   ```bash
   docker run -p 5300:5300 -v $(pwd)/config:/app/config ahmedkhalil777/neuromcp-azuredevops
   ```

#### For NeuroMCP.SqlServer:

1. Create a local directory for configuration:
   ```bash
   mkdir -p ./config
   ```

2. Create an `appsettings.json` file in the config directory:
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

3. Run the container with the mounted configuration:
   ```bash
   docker run -p 5200:5200 -v $(pwd)/config:/app/config ahmedkhalil777/neuromcp-sqlserver
   ```

### Using Environment Variables

You can also configure NeuroMCP services using environment variables:

#### For NeuroMCP.AzureDevOps:

```bash
docker run -p 5300:5300 \
  -e AZUREDEVOPS__ORGURL="https://dev.azure.com/your-organization" \
  -e AZUREDEVOPS__DEFAULTPROJECT="your-project" \
  -e AZUREDEVOPS__AUTHENTICATION__TYPE="pat" \
  -e AZUREDEVOPS__AUTHENTICATION__PATTOKEN="your-pat-token" \
  ahmedkhalil777/neuromcp-azuredevops
```

#### For NeuroMCP.SqlServer:

```bash
docker run -p 5200:5200 \
  -e DB_SERVER="your-sql-server" \
  -e DB_NAME="your-database" \
  -e DB_USER="your-username" \
  -e DB_PASSWORD="your-password" \
  -e DB_TRUST_SERVER_CERTIFICATE=true \
  ahmedkhalil777/neuromcp-sqlserver
```

## Running with Docker Compose

For more complex setups, you can use Docker Compose to run multiple services together:

Create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  neuromcp-azuredevops:
    image: ahmedkhalil777/neuromcp-azuredevops:latest
    ports:
      - "5300:5300"
    volumes:
      - ./azuredevops-config:/app/config
    restart: unless-stopped

  neuromcp-sqlserver:
    image: ahmedkhalil777/neuromcp-sqlserver:latest
    ports:
      - "5200:5200"
    volumes:
      - ./sqlserver-config:/app/config
    restart: unless-stopped
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

Run the services using Docker Compose:

```bash
docker-compose up -d
```

## Building Custom Images

If you need to customize the Docker images, you can build them from source:

### NeuroMCP.AzureDevOps:

```bash
git clone https://github.com/AhmedKhalil777/NeuroMCP.git
cd NeuroMCP/NeuroMCP.AzureDevOps
docker build -t my-neuromcp-azuredevops .
```

### NeuroMCP.SqlServer:

```bash
git clone https://github.com/AhmedKhalil777/NeuroMCP.git
cd NeuroMCP/NeuroMCP.SqlServer
docker build -t my-neuromcp-sqlserver .
```

## Checking the Services

Once your containers are running, you can verify they're working correctly:

- For NeuroMCP.AzureDevOps: http://localhost:5300/swagger
- For NeuroMCP.SqlServer: http://localhost:5200/swagger

## Troubleshooting

### Common Issues

1. **Port conflicts**: If the ports are already in use, change the port mapping:
   ```bash
   docker run -p 8080:5300 ahmedkhalil777/neuromcp-azuredevops
   ```

2. **Configuration issues**: Check that your configuration files have the correct format and permissions.

3. **Connection problems**: Ensure proper network connectivity between containers if using Docker Compose.

4. **Image not found**: Make sure you're using the correct image name and tag.

### Viewing Logs

To view container logs:

```bash
docker logs <container_id_or_name>
```

Or with Docker Compose:

```bash
docker-compose logs neuromcp-azuredevops
docker-compose logs neuromcp-sqlserver
```

## Next Steps

- [Docker Hub](docker-hub.md) - More information about Docker Hub repositories
- [Docker Compose](docker-compose.md) - Advanced Docker Compose configuration
- [AzureDevOps Configuration](azuredevops-configuration.md) - Detailed AzureDevOps configuration
- [SQL Server Configuration](sqlserver-configuration.md) - Detailed SQL Server configuration 