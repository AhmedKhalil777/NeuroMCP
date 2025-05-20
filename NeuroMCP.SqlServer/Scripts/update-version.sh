#!/bin/bash

# Check if version is provided
if [ "$#" -ne 1 ]; then
    echo "Error: Version parameter is required"
    echo "Usage: $0 <version>"
    exit 1
fi

VERSION=$1
PROJECT_FILE="$(dirname "$0")/../MCP.SqlServer.csproj"

# Check if file exists
if [ ! -f "$PROJECT_FILE" ]; then
    echo "Error: Project file not found at: $PROJECT_FILE"
    exit 1
fi

# Update the version in the project file
sed -i "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" "$PROJECT_FILE"

echo "Updated version to $VERSION in $PROJECT_FILE" 