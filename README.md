# NeuroMCP

A suite of tools built on the Model Context Protocol (MCP) framework, enabling seamless interactions with various services through an AI-friendly interface.

![NeuroMCP Logo](NeuroMCP.SqlServer/logo.png)

## Available Packages

NeuroMCP currently includes the following packages:

### NeuroMCP.SqlServer

A tool for interacting with Microsoft SQL Server databases:

- **SQL Query Execution**: Execute SQL queries against SQL Server databases
- **Schema Inspection**: Explore database schema information
- **Connection Management**: Safely store and manage database connections

### NeuroMCP.AzureDevOps

A tool for interacting with Azure DevOps APIs:

- **Authentication**: Support for Personal Access Tokens (PAT), OAuth, and Azure AD (via Azure.Identity)
- **Organizations & Projects**: List and manage Azure DevOps organizations and projects
- **Repositories**: Access Git repositories, commits, pull requests
- **Work Items**: Query, create, and update work items
- **Pipelines**: Manage build and release pipelines
- **Wikis**: Access and modify wiki content

## Installation

### SQL Server Package

```bash
dotnet tool install --global NeuroMCP.SqlServer
```

### Azure DevOps Package

```bash
dotnet tool install --global NeuroMCP.AzureDevOps
```

## Usage

### SQL Server Package

Start the SQL Server service:

```bash
neuromcp-mssql --port 5200
```

### Azure DevOps Package

Start the Azure DevOps service:

```bash
neuromcp-azdevops --port 5300
```

## Service Mode (Windows)

Both packages can be installed as Windows services:

### SQL Server Service

```powershell
# Install the service
neuromcp-mssql --install --service-name NeuroMCPSqlServer --port 5200

# Start the service
Start-Service -Name NeuroMCPSqlServer
```

### Azure DevOps Service

```powershell
# Install the service
neuromcp-azdevops --install --service-name NeuroMCPAzureDevOps --port 5300

# Start the service
Start-Service -Name NeuroMCPAzureDevOps
```

## Configuration

### SQL Server Configuration

Use environment variables or connection strings to configure database connections.

### Azure DevOps Configuration

Configure Azure DevOps integration using:

1. Environment variables:
   - `NEUROMCP_AZDEVOPS_ORG_URL`: Default organization URL
   - `NEUROMCP_AZDEVOPS_PROJECT`: Default project name
   - `NEUROMCP_AZDEVOPS_PAT`: Personal Access Token for authentication

2. Configuration file (mcp.json):
```json
{
  "azureDevOps": {
    "orgUrl": "https://dev.azure.com/your-org",
    "defaultProject": "YourProject",
    "authentication": {
      "type": "pat",
      "patToken": "your-pat-token"
    }
  }
}
```

## Requirements

- .NET 8.0 or higher
- Microsoft SQL Server (2016 or newer) or Azure SQL Database (for SQL Server package)
- Azure DevOps account with appropriate permissions (for Azure DevOps package)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request