# NeuroMCP.AzureDevOps

![Azure DevOps Logo](images/azuredevops-logo.png)

NeuroMCP.AzureDevOps is a Model Context Protocol (MCP) server for interacting with Azure DevOps APIs. It serves as a bridge between AI agents and Azure DevOps, enabling operations like querying work items, repositories, pull requests, and more through a standardized API.

## Features

- **Work Item Management**: Query, create, update, and link work items
- **Repository Access**: Browse repositories, access file content, search code
- **Pull Request Operations**: Create, list, and manage pull requests
- **Pipeline Integration**: List pipelines, get details, trigger pipeline runs
- **Wiki Management**: Access and update wiki content
- **Search Capabilities**: Search across code, wikis, and work items

## Installation

### Docker Installation

The easiest way to run NeuroMCP.AzureDevOps is using Docker:

```bash
docker pull ahmedkhalil777/neuromcp-azuredevops:latest
docker run -p 5300:5300 ahmedkhalil777/neuromcp-azuredevops
```

For more details, see the [Docker Installation](docker-installation.md) guide.

### Local Installation

To install NeuroMCP.AzureDevOps as a .NET tool:

```bash
dotnet tool install --global NeuroMCP.AzureDevOps
neuromcp-azdevops --port 5300
```

For more details, see the [Local Installation](local-installation.md) guide.

## Configuration

### Azure DevOps Authentication

NeuroMCP.AzureDevOps requires Azure DevOps authentication details. You can provide these through an `appsettings.json` file:

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

For more detailed configuration options, see the [AzureDevOps Configuration](azuredevops-configuration.md) guide.

## API Reference

NeuroMCP.AzureDevOps implements the Model Context Protocol (MCP) and provides the following tools:

| Tool Name | Description |
|-----------|-------------|
| `azureDevOps_getMe` | Get details of the authenticated user |
| `azureDevOps_listOrganizations` | List all accessible organizations |
| `azureDevOps_listProjects` | List all projects in an organization |
| `azureDevOps_getProject` | Get details of a specific project |
| `azureDevOps_listRepositories` | List repositories in a project |
| `azureDevOps_getRepository` | Get details of a specific repository |
| `azureDevOps_getFileContent` | Get content of a file from a repository |
| `azureDevOps_listWorkItems` | List work items in a project |
| `azureDevOps_getWorkItem` | Get details of a specific work item |
| `azureDevOps_createWorkItem` | Create a new work item |
| `azureDevOps_updateWorkItem` | Update an existing work item |
| `azureDevOps_createPullRequest` | Create a new pull request |
| `azureDevOps_listPullRequests` | List pull requests in a repository |
| `azureDevOps_searchCode` | Search for code across repositories |
| `azureDevOps_searchWiki` | Search for content across wiki pages |
| `azureDevOps_searchWorkItems` | Search for work items across projects |

For more details on each tool, see the [AzureDevOps API Reference](azuredevops-api-reference.md).

## Examples

### Listing Work Items

```json
{
  "method": "azureDevOps_listWorkItems",
  "params": {
    "wiql": "SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'Active' ORDER BY [System.CreatedDate] DESC"
  }
}
```

### Creating a Work Item

```json
{
  "method": "azureDevOps_createWorkItem",
  "params": {
    "workItemType": "Task",
    "title": "Implement new feature",
    "description": "<div>This is a task to implement the new feature</div>"
  }
}
```

For more examples, see the [AzureDevOps Examples](azuredevops-examples.md) guide.

## Security Considerations

- Always use Personal Access Tokens (PATs) with minimal required permissions
- Store PATs securely and never in source code
- Consider using environment variables or a secrets manager for PAT storage
- Implement proper authentication for the MCP server itself

For more security best practices, see the [Security Best Practices](security.md) guide.

## Troubleshooting

Common issues and their solutions:

- **Authentication Failures**: Ensure your PAT is valid and has the required permissions
- **API Limitations**: Be aware of Azure DevOps API rate limits
- **Connection Issues**: Check network connectivity to the Azure DevOps services

For more troubleshooting help, see the [Common Issues](common-issues.md) guide.

## Related Documentation

- [Docker Hub](docker-hub.md) - Docker Hub repository information
- [API Reference](azuredevops-api-reference.md) - Detailed API documentation
- [Configuration Guide](azuredevops-configuration.md) - Advanced configuration options 