# NeuroMCP

A SQL Server tool built on the Model Context Protocol (MCP) framework, enabling seamless interactions with Microsoft SQL Server databases through an AI-friendly interface.

![NeuroMCP Logo](NeuroMCP.SqlServer/logo.png)

## Features

- **SQL Query Execution**: Execute SQL queries against SQL Server databases
- **Schema Inspection**: Explore database schema information
- **Connection Management**: Safely store and manage database connections
- **MCP Integration**: Full compliance with the Model Context Protocol for AI tools
- **Cross-Platform Support**: Runs on Windows, Linux, and macOS
- **Service Mode**: Can be installed as a Windows service or Linux daemon

## Installation

### Via .NET Tool

```bash
dotnet tool install --global NeuroMCP.SqlServer
```

### Manual Installation

1. Clone the repository
2. Build the project
```bash
dotnet build NeuroMCP.SqlServer -c Release
```
3. Install the tool locally
```bash
dotnet pack NeuroMCP.SqlServer -c Release --output ./nupkg
dotnet tool install --global --add-source ./nupkg NeuroMCP.SqlServer
```

## Usage

### CLI Mode

Start the server in command-line mode:

```bash
neuromcp-mssql --port 5200
```

### Service Mode (Windows)

Install and start as a Windows service:

```powershell
# Install the service
neuromcp-mssql --install --service-name NeuroMCPSqlServer --port 5200

# Start the service
Start-Service -Name NeuroMCPSqlServer
```

### Using with PowerShell Script

The included installation script provides additional options:

```powershell
# Install the tool and run as a service
.\NeuroMCP.SqlServer\Scripts\install-neuromcp.ps1 -Port 5200 -InstallService

# Use a local package
.\NeuroMCP.SqlServer\Scripts\install-neuromcp.ps1 -UseLocalPackage -Install
```

## API Endpoints

When running, the service provides the following endpoints:

- **SQL Execution**: Execute queries against a SQL Server database
- **Connection Test**: Test database connection strings
- **Status Check**: Check the service status and version

## Requirements

- .NET 8.0 or higher
- Microsoft SQL Server (2016 or newer) or Azure SQL Database

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request