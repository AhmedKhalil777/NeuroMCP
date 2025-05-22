# Docker Compose Guide

This guide explains how to use Docker Compose to run multiple NeuroMCP services together in a coordinated environment.

## Prerequisites

- Docker and Docker Compose installed ([Install Docker Compose](https://docs.docker.com/compose/install/))
- Basic understanding of Docker Compose
- Docker Hub access

## Basic Docker Compose Setup

Create a file named `docker-compose.yml` in your project directory:

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
```

## Configuration Setup

Before starting the services, create configuration directories and files:

```bash
# Create config directories
mkdir -p azuredevops-config
mkdir -p sqlserver-config

# Create AzureDevOps config
cat > azuredevops-config/appsettings.json << EOF
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
EOF

# Create SQL Server config
cat > sqlserver-config/appsettings.json << EOF
{
  "Database": {
    "Server": "your-sql-server",
    "Database": "your-database",
    "UserId": "your-username",
    "Password": "your-password",
    "TrustServerCertificate": true
  }
}
EOF
```

## Starting the Services

Run the services using Docker Compose:

```bash
docker-compose up -d
```

This will start both NeuroMCP services in detached mode.

## Advanced Docker Compose Configuration

### Including SQL Server Database

To include a SQL Server database in your Docker Compose setup:

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
    environment:
      - DB_SERVER=sqlserver
      - DB_NAME=master
      - DB_USER=sa
      - DB_PASSWORD=YourStrongPassword123
      - DB_TRUST_SERVER_CERTIFICATE=true
    depends_on:
      - sqlserver
    restart: unless-stopped

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrongPassword123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrongPassword123 -Q 'SELECT 1' || exit 1"]
      interval: 10s
      retries: 10
      start_period: 10s
      timeout: 3s

volumes:
  sqlserver-data:
```

### Using Environment Variables

You can use environment variables instead of mounted configuration files:

```yaml
version: '3.8'

services:
  neuromcp-azuredevops:
    image: ahmedkhalil777/neuromcp-azuredevops:latest
    ports:
      - "5300:5300"
    environment:
      - AZUREDEVOPS__ORGURL=https://dev.azure.com/your-organization
      - AZUREDEVOPS__DEFAULTPROJECT=your-project
      - AZUREDEVOPS__AUTHENTICATION__TYPE=pat
      - AZUREDEVOPS__AUTHENTICATION__PATTOKEN=your-pat-token
    restart: unless-stopped

  neuromcp-sqlserver:
    image: ahmedkhalil777/neuromcp-sqlserver:latest
    ports:
      - "5200:5200"
    environment:
      - DB_SERVER=your-sql-server
      - DB_NAME=your-database
      - DB_USER=your-username
      - DB_PASSWORD=your-password
      - DB_TRUST_SERVER_CERTIFICATE=true
    restart: unless-stopped
```

### Using Environment Files

For better security, you can use environment files:

```yaml
version: '3.8'

services:
  neuromcp-azuredevops:
    image: ahmedkhalil777/neuromcp-azuredevops:latest
    ports:
      - "5300:5300"
    env_file:
      - ./azuredevops.env
    restart: unless-stopped

  neuromcp-sqlserver:
    image: ahmedkhalil777/neuromcp-sqlserver:latest
    ports:
      - "5200:5200"
    env_file:
      - ./sqlserver.env
    restart: unless-stopped
```

Create the environment files:

```bash
# azuredevops.env
AZUREDEVOPS__ORGURL=https://dev.azure.com/your-organization
AZUREDEVOPS__DEFAULTPROJECT=your-project
AZUREDEVOPS__AUTHENTICATION__TYPE=pat
AZUREDEVOPS__AUTHENTICATION__PATTOKEN=your-pat-token
```

```bash
# sqlserver.env
DB_SERVER=your-sql-server
DB_NAME=your-database
DB_USER=your-username
DB_PASSWORD=your-password
DB_TRUST_SERVER_CERTIFICATE=true
```

## Managing the Services

### Viewing Logs

```bash
# View logs for all services
docker-compose logs

# View logs for a specific service
docker-compose logs neuromcp-azuredevops

# Follow logs
docker-compose logs -f
```

### Starting and Stopping Services

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# Stop services and remove volumes
docker-compose down -v

# Restart a specific service
docker-compose restart neuromcp-sqlserver
```

### Scaling Services

For load balancing, you can run multiple instances of a service:

```bash
docker-compose up -d --scale neuromcp-azuredevops=2
```

Note: If you scale services, you'll need to use a load balancer or change the port assignments.

## Production Considerations

For production environments, consider:

1. **Using Docker Secrets** for sensitive information
2. **Setting resource limits** for containers
3. **Implementing healthchecks** for reliability
4. **Using a reverse proxy** like Nginx or Traefik for SSL termination
5. **Setting up proper logging** with external log aggregation

Example with resource limits and healthchecks:

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
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5300/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 10s

  neuromcp-sqlserver:
    image: ahmedkhalil777/neuromcp-sqlserver:latest
    ports:
      - "5200:5200"
    volumes:
      - ./sqlserver-config:/app/config
    restart: unless-stopped
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5200/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 10s
```

## Next Steps

- [Docker Installation](docker-installation.md) - Basic Docker installation guide
- [Docker Hub](docker-hub.md) - Docker Hub repositories information
- [AzureDevOps Configuration](azuredevops-configuration.md) - AzureDevOps configuration details
- [SQL Server Configuration](sqlserver-configuration.md) - SQL Server configuration details 