#!/bin/bash

# Default values
PORT="5200"
SERVICE_NAME="MCPSqlServer"
INSTALL_SERVICE=false
UNINSTALL_SERVICE=false
INSTALL=false
UPDATE=false
FORCE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --port)
      PORT="$2"
      shift 2
      ;;
    --service-name)
      SERVICE_NAME="$2"
      shift 2
      ;;
    --install-service)
      INSTALL_SERVICE=true
      shift
      ;;
    --uninstall-service)
      UNINSTALL_SERVICE=true
      shift
      ;;
    --install)
      INSTALL=true
      shift
      ;;
    --update)
      UPDATE=true
      shift
      ;;
    --force)
      FORCE=true
      shift
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

# Check if running as root when installing/uninstalling services
if [ "$INSTALL_SERVICE" = true ] || [ "$UNINSTALL_SERVICE" = true ]; then
  if [ "$(id -u)" -ne 0 ]; then
    echo "You need root privileges to install or uninstall the service."
    exit 1
  fi
fi

# Install or update the tool
if [ "$INSTALL" = true ] || [ "$UPDATE" = true ]; then
  if [ "$UPDATE" = true ] || [ "$FORCE" = true ]; then
    echo "Uninstalling existing MCP.SqlServer tool..."
    dotnet tool uninstall --global MCP.SqlServer 2>/dev/null
  fi
  
  echo "Installing MCP.SqlServer tool..."
  dotnet tool install --global MCP.SqlServer
  
  if [ $? -ne 0 ]; then
    echo "Failed to install the tool."
    exit 1
  fi
  
  TOOL_PATH=$(which mcp-mssql 2>/dev/null)
  
  if [ -z "$TOOL_PATH" ]; then
    echo "Could not find the installed tool."
    exit 1
  fi
  
  echo "MCP.SqlServer tool installed successfully at: $TOOL_PATH"
else
  TOOL_PATH=$(which mcp-mssql 2>/dev/null)
  
  if [ -z "$TOOL_PATH" ]; then
    echo "MCP.SqlServer tool not found. Installing..."
    dotnet tool install --global MCP.SqlServer
    
    if [ $? -ne 0 ]; then
      echo "Failed to install the tool."
      exit 1
    fi
    
    TOOL_PATH=$(which mcp-mssql 2>/dev/null)
    
    if [ -z "$TOOL_PATH" ]; then
      echo "Could not find the installed tool."
      exit 1
    fi
    
    echo "MCP.SqlServer tool installed successfully at: $TOOL_PATH"
  else
    echo "Using existing installation at: $TOOL_PATH"
  fi
fi

# Install as service
if [ "$INSTALL_SERVICE" = true ]; then
  echo "Installing MCP.SqlServer as a systemd service..."
  mcp-mssql --install --service-name "$SERVICE_NAME" --port "$PORT"
  
  if [ $? -ne 0 ]; then
    echo "Failed to install the service."
    exit 1
  fi
  
  echo "Starting the service..."
  systemctl start "$SERVICE_NAME"
  
  if [ $? -eq 0 ]; then
    echo "Service installed and started successfully."
    echo "The service is now running on http://localhost:$PORT"
  else
    echo "Failed to start the service."
    exit 1
  fi
fi

# Uninstall service
if [ "$UNINSTALL_SERVICE" = true ]; then
  echo "Stopping and removing the systemd service..."
  systemctl stop "$SERVICE_NAME" 2>/dev/null
  mcp-mssql --uninstall --service-name "$SERVICE_NAME"
  
  if [ $? -ne 0 ]; then
    echo "Failed to uninstall the service."
    exit 1
  fi
  
  echo "Service uninstalled successfully."
fi

# If not installing as a service and not uninstalling, just run the tool
if [ "$INSTALL_SERVICE" = false ] && [ "$UNINSTALL_SERVICE" = false ]; then
  echo "Starting MCP.SqlServer on port $PORT..."
  mcp-mssql --port "$PORT"
fi 