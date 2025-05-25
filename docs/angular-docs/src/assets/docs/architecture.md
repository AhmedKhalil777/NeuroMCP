# NeuroMCP Architecture

This document describes the architecture of the NeuroMCP system, its components, and how they interact.

## System Overview

NeuroMCP (Neural Model Context Protocol) is designed as a collection of specialized microservices that enable AI agents to interact with various backend systems through a standardized API. Each service follows the Model Context Protocol (MCP) specification to provide a consistent interface for AI agents.

## High-Level Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│                 │    │                 │    │                 │
│   AI Agent 1    │    │    AI Agent 2   │    │    AI Agent 3   │
│                 │    │                 │    │                 │
└────────┬────────┘    └────────┬────────┘    └────────┬────────┘
         │                      │                      │
         │                      │                      │
         │                      │                      │
         ▼                      ▼                      ▼
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    Model Context Protocol                   │
│                                                             │
└────────┬────────────────────┬─────────────────────┬─────────┘
         │                    │                     │
         │                    │                     │
         ▼                    ▼                     ▼
┌─────────────────┐    ┌─────────────────┐   ┌─────────────────┐
│                 │    │                 │   │                 │
│NeuroMCP.Azure   │    │NeuroMCP.SQL     │   │NeuroMCP.Future  │
│DevOps           │    │Server           │   │Services         │
│                 │    │                 │   │                 │
└────────┬────────┘    └────────┬────────┘   └────────┬────────┘
         │                      │                     │
         │                      │                     │
         ▼                      ▼                     ▼
┌─────────────────┐    ┌─────────────────┐   ┌─────────────────┐
│                 │    │                 │   │                 │
│Azure DevOps     │    │SQL Server       │   │Other Backend    │
│                 │    │                 │   │Systems          │
│                 │    │                 │   │                 │
└─────────────────┘    └─────────────────┘   └─────────────────┘
```

## Model Context Protocol (MCP)

The Model Context Protocol is a standardized way for AI agents to interact with external systems. Key aspects of MCP include:

1. **Tool-based API**: Functionality is exposed as tools that agents can use
2. **JSON-RPC based**: Interactions follow JSON-RPC 2.0 specification
3. **Consistent interface**: All services present a similar interface pattern
4. **Self-describing**: Services provide metadata about available tools

## NeuroMCP Services

### NeuroMCP.AzureDevOps

This service acts as a bridge between AI agents and Azure DevOps, providing tools for:

- Work item management
- Repository access
- Pull request operations
- Pipeline management
- Wiki access
- Search functionality

#### Internal Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  NeuroMCP.AzureDevOps                   │
│                                                         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │             │    │             │    │             │  │
│  │ MCP API     │    │ Azure DevOps│    │ Auth        │  │
│  │ Controllers │◄───┤ Service     │◄───┤ Manager     │  │
│  │             │    │             │    │             │  │
│  └─────────────┘    └─────────────┘    └─────────────┘  │
│                            ▲                            │
│                            │                            │
│                     ┌──────┴──────┐                     │
│                     │             │                     │
│                     │ Azure DevOps│                     │
│                     │ Client      │                     │
│                     │             │                     │
│                     └─────────────┘                     │
└─────────────────────────────────────────────────────────┘
```

### NeuroMCP.SqlServer

This service provides a bridge between AI agents and SQL Server databases, offering tools for:

- SQL query execution
- Connection management
- Schema exploration

#### Internal Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   NeuroMCP.SqlServer                    │
│                                                         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │             │    │             │    │             │  │
│  │ MCP API     │    │ SQL Server  │    │ Connection  │  │
│  │ Controllers │◄───┤ Service     │◄───┤ Manager     │  │
│  │             │    │             │    │             │  │
│  └─────────────┘    └─────────────┘    └─────────────┘  │
│                            ▲                            │
│                            │                            │
│                     ┌──────┴──────┐                     │
│                     │             │                     │
│                     │ SQL Client  │                     │
│                     │             │                     │
│                     └─────────────┘                     │
└─────────────────────────────────────────────────────────┘
```

## Common Components

Both services share common architectural components:

### API Layer

- **MCPController**: Handles JSON-RPC requests
- **HealthController**: Provides health check endpoints
- **SwaggerUI**: API documentation

### Service Layer

- **Core Services**: Implement the business logic
- **Authentication**: Manages credentials and tokens
- **Configuration**: Manages service settings

### Infrastructure

- **Logging**: Records service activities
- **Error Handling**: Manages exceptions
- **Configuration Management**: Loads settings from files or environment

## Communication Flow

1. AI agent sends a JSON-RPC request to a NeuroMCP service
2. The service validates the request
3. The appropriate tool handler processes the request
4. The service makes API calls to the backend system
5. Results are processed and returned to the AI agent

Example JSON-RPC request:

```json
{
  "jsonrpc": "2.0",
  "id": "123",
  "method": "azureDevOps_listWorkItems",
  "params": {
    "wiql": "SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'Active'"
  }
}
```

Example JSON-RPC response:

```json
{
  "jsonrpc": "2.0",
  "id": "123",
  "result": [
    {
      "id": 42,
      "title": "Implement feature X",
      "state": "Active",
      "type": "User Story"
    },
    {
      "id": 43,
      "title": "Fix bug Y",
      "state": "Active",
      "type": "Bug"
    }
  ]
}
```

## Deployment Architecture

NeuroMCP services are designed to be deployed as containerized applications using Docker. They can be deployed:

1. **Individually**: Each service in its own container
2. **Together**: Using Docker Compose to orchestrate multiple services
3. **In Kubernetes**: For production environments with scaling requirements

### Docker Deployment

The simplest deployment model uses Docker containers:

```
┌─────────────────┐    ┌─────────────────┐
│                 │    │                 │
│ NeuroMCP.Azure  │    │ NeuroMCP.SQL    │
│ DevOps Container│    │ Server Container│
│                 │    │                 │
└────────┬────────┘    └────────┬────────┘
         │                      │
         │    Docker Network    │
         └──────────┬───────────┘
                    │
                    ▼
        ┌─────────────────────────┐
        │                         │
        │ Docker Host             │
        │                         │
        └─────────────────────────┘
```

## Security Architecture

NeuroMCP implements several security features:

1. **Authentication**: Services authenticate with backend systems using:
   - Personal Access Tokens (PATs)
   - OAuth tokens
   - SQL authentication

2. **Configuration Security**:
   - Sensitive configuration can be stored in environment variables
   - Supports Docker secrets
   - Credentials can be injected at runtime

3. **API Security**:
   - Input validation to prevent injection attacks
   - Parameterized SQL queries
   - Rate limiting

## Performance Considerations

- Services are designed to be stateless for horizontal scaling
- Caching is used to reduce API calls to backend systems
- Query optimization for SQL operations
- Connection pooling for database operations

## Future Architecture

The NeuroMCP platform is designed for extensibility. Future services may include:

- **NeuroMCP.GitHub**: For GitHub integration
- **NeuroMCP.Jira**: For Jira integration
- **NeuroMCP.MongoDB**: For MongoDB database access
- **NeuroMCP.Kubernetes**: For Kubernetes management

## References

- [Model Context Protocol Specification](https://github.com/AhmedKhalil777/MCP-Spec)
- [Azure DevOps REST API](https://docs.microsoft.com/en-us/rest/api/azure/devops)
- [SQL Server Client Libraries](https://docs.microsoft.com/en-us/sql/connect/sql-connection-libraries) 