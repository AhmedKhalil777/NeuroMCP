# NeuroMCP.AzureDevOps

## Overview

NeuroMCP.AzureDevOps is a Model Context Protocol (MCP) server for interacting with Azure DevOps APIs. It serves as a bridge between AI agents and Azure DevOps, enabling operations like querying work items, repositories, pull requests, and more through a standardized API.

## Quick Start

```bash
# Pull the image
docker pull ahmedkhalil777/neuromcp-azuredevops:latest

# Run with default settings
docker run -p 5300:5300 ahmedkhalil777/neuromcp-azuredevops

# Run with custom configuration
docker run -p 5300:5300 \
  -v /path/to/config:/app/config \
  ahmedkhalil777/neuromcp-azuredevops
```

## Configuration

The server requires Azure DevOps authentication details. You can provide these in several ways:

### 1. Using a Configuration File

Create an `appsettings.json` file in a local directory and mount it:

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

### 2. Using Environment Variables

```bash
docker run -p 5300:5300 \
  -e AZUREDEVOPS__ORGURL="https://dev.azure.com/your-organization" \
  -e AZUREDEVOPS__DEFAULTPROJECT="your-project" \
  -e AZUREDEVOPS__AUTHENTICATION__TYPE="pat" \
  -e AZUREDEVOPS__AUTHENTICATION__PATTOKEN="your-pat-token" \
  ahmedkhalil777/neuromcp-azuredevops
```

## Available Tools

This MCP server provides tools for:

- Authenticating with Azure DevOps
- Managing organizations and projects
- Working with repositories and code
- Querying and updating work items
- Managing pull requests
- Searching code and content
- Interacting with pipelines and wikis

## Docker Compose Example

```yaml
version: '3.8'

services:
  neuromcp-azuredevops:
    image: ahmedkhalil777/neuromcp-azuredevops:latest
    ports:
      - "5300:5300"
    volumes:
      - ./config:/app/config
    restart: unless-stopped
```

## Security Notes

- Never embed sensitive tokens directly in images or public repositories
- Use volume mounts or environment variables for configuration
- Create Azure DevOps PATs with minimal required permissions

## Source Code

The source code for this project is available on GitHub: [AhmedKhalil777/NeuroMCP](https://github.com/AhmedKhalil777/NeuroMCP)

## Documentation

For more detailed documentation, please see:
- [GitHub Project README](https://github.com/AhmedKhalil777/NeuroMCP/blob/main/README.md)
- [Docker Setup Guide](https://github.com/AhmedKhalil777/NeuroMCP/blob/main/DOCKER-PUBLISH.md) 