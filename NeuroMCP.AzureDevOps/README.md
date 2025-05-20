# NeuroMCP.AzureDevOps

A .NET Global Tool for interacting with Azure DevOps APIs through the Model Context Protocol (MCP).

## Features

- **Authentication**: Support for Personal Access Tokens (PAT), OAuth, and Azure AD (via Azure.Identity)
- **Organizations**: List and manage Azure DevOps organizations
- **Projects**: Create, list, and manage projects
- **Repositories**: Access Git repositories, commits, pull requests
- **Work Items**: Query, create, and update work items
- **Pipelines**: Manage build and release pipelines
- **Wikis**: Access and modify wiki content

## Installation

```bash
dotnet tool install --global NeuroMCP.AzureDevOps
```

## Configuration

The tool can be configured through:

1. Environment variables:
   - `NEUROMCP_AZDEVOPS_ORG_URL`: Default organization URL
   - `NEUROMCP_AZDEVOPS_PROJECT`: Default project name
   - `NEUROMCP_AZDEVOPS_PAT`: Personal Access Token for authentication

2. Configuration file: 
   Create a `mcp.json` file in your working directory:

```json
{
  "azureDevOps": {
    "orgUrl": "https://dev.azure.com/your-org",
    "defaultProject": "YourProject",
    "authentication": {
      "type": "pat", // "pat", "interactive", "azureAD"
      "patToken": "your-pat-token" // Only for "pat" type
    }
  }
}
```

## Usage

### CLI Mode

Start the server in command-line mode:

```bash
neuromcp-azdevops --port 5300
```

### Service Mode (Windows)

Install and start as a Windows service:

```powershell
# Install the service
neuromcp-azdevops --install --service-name NeuroMCPAzureDevOps --port 5300

# Start the service
Start-Service -Name NeuroMCPAzureDevOps
```

## API Endpoints

When running, the service provides MCP endpoints for:

- User information
- Organizations and projects
- Git repositories
- Work Items
- Build pipelines
- Wiki content

## Authentication Methods

### Personal Access Token (PAT)

The simplest authentication method - provide a PAT token through environment variables or configuration.

### Azure AD Authentication

For managed environments, you can use Azure.Identity for integrated authentication:

```json
{
  "azureDevOps": {
    "authentication": {
      "type": "azureAD"
    }
  }
}
```

This will use DefaultAzureCredential which tries multiple authentication methods.

## Requirements

- .NET 8.0 or higher
- Azure DevOps account with appropriate permissions 