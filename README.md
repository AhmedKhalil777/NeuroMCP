# NeuroMCP

NeuroMCP (Neural Model Context Protocol) is a collection of specialized microservices that enable AI agents to interact with various backend systems through a standardized API. Each service follows the Model Context Protocol (MCP) specification to provide a consistent interface for AI agents.

## Services

The project currently includes the following services:

- **[NeuroMCP.AzureDevOps](NeuroMCP.AzureDevOps)**: Integrates with Azure DevOps for work items, repositories, and more
- **[NeuroMCP.SqlServer](NeuroMCP.SqlServer)**: Provides SQL Server database access and query execution capabilities

## Documentation

Complete documentation for NeuroMCP is available in the [docs](docs) directory:

- [Installation Guide](docs/docker-installation.md)
- [Azure DevOps Service](docs/azure-devops.md)
- [SQL Server Service](docs/sql-server.md)
- [Architecture Overview](docs/architecture.md)
- [Security Best Practices](docs/security.md)
- [Docker Compose Guide](docs/docker-compose.md)

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

For more detailed instructions, see the [Docker Installation Guide](docs/docker-installation.md).

## Docker Compose Example

For running multiple services together:

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

For more advanced configurations, see the [Docker Compose Guide](docs/docker-compose.md).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.