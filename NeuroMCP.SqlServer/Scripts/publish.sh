#!/bin/bash

# Default values
VERSION=""
API_KEY=""
SKIP_BUILD=false
LOCAL_ONLY=false

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --version)
      VERSION="$2"
      shift 2
      ;;
    --api-key)
      API_KEY="$2"
      shift 2
      ;;
    --skip-build)
      SKIP_BUILD=true
      shift
      ;;
    --local-only)
      LOCAL_ONLY=true
      shift
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

# Get script directory and project directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_DIR" || exit 1

if [ -z "$VERSION" ]; then
    # Try to get version from csproj
    if grep -q '<Version>' "$PROJECT_DIR/MCP.SqlServer.csproj"; then
        VERSION=$(grep '<Version>' "$PROJECT_DIR/MCP.SqlServer.csproj" | sed -E 's/.*<Version>(.*)<\/Version>.*/\1/')
        echo "Using version from csproj: $VERSION"
    else
        VERSION="1.0.0"
        echo "No version found in csproj, using default: $VERSION"
    fi
fi

# Ensure nupkg directory exists
NUPKG_DIR="$PROJECT_DIR/nupkg"
if [ ! -d "$NUPKG_DIR" ]; then
    mkdir -p "$NUPKG_DIR"
    echo "Created directory: $NUPKG_DIR"
fi

# Build and pack the project
if [ "$SKIP_BUILD" = false ]; then
    echo "Building and packing MCP.SqlServer v$VERSION..."
    dotnet pack -c Release /p:Version="$VERSION"
    
    if [ $? -ne 0 ]; then
        echo "Failed to pack the project."
        exit 1
    fi
fi

# Check if the .nupkg file exists
NUPKG_FILE="$NUPKG_DIR/MCP.SqlServer.$VERSION.nupkg"
if [ ! -f "$NUPKG_FILE" ]; then
    echo "Package file not found: $NUPKG_FILE"
    exit 1
fi

# Install locally for testing
echo "Installing package locally for testing..."
dotnet tool uninstall --global MCP.SqlServer 2>/dev/null
dotnet tool install --global --add-source "$NUPKG_DIR" MCP.SqlServer

if [ $? -ne 0 ]; then
    echo "Failed to install the package locally."
    exit 1
fi

# Verify installation
echo "Verifying installation..."
if ! command -v mcp-mssql &> /dev/null; then
    echo "Tool not found after installation. Please check the package."
    exit 1
fi

TOOL_PATH=$(which mcp-mssql)
echo "Tool installed successfully at: $TOOL_PATH"
echo "Testing tool by getting version info..."
mcp-mssql --version

if [ "$LOCAL_ONLY" = true ]; then
    echo "Package installed locally only. Skipping NuGet.org publishing."
    exit 0
fi

# Publish to NuGet.org
if [ -z "$API_KEY" ]; then
    echo -n "Enter your NuGet.org API key: "
    read -r API_KEY
fi

if [ -z "$API_KEY" ]; then
    echo "API key is required to publish to NuGet.org."
    echo "You can still manually publish using: dotnet nuget push $NUPKG_FILE --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY"
    exit 1
fi

echo "Publishing to NuGet.org..."
dotnet nuget push "$NUPKG_FILE" --source https://api.nuget.org/v3/index.json --api-key "$API_KEY"

if [ $? -eq 0 ]; then
    echo "Package published successfully to NuGet.org!"
    echo "Package URL: https://www.nuget.org/packages/MCP.SqlServer/$VERSION"
else
    echo "Failed to publish package to NuGet.org."
    exit 1
fi 