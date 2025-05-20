#!/bin/bash

# Export environment variables
export MSSQL_SERVER=$SQL_SERVER
export MSSQL_DATABASE=$SQL_DATABASE
export MSSQL_USER=$SQL_USER
export MSSQL_PASSWORD=$SQL_PASSWORD

echo "Environment variables set:"
echo "MSSQL_SERVER: $MSSQL_SERVER"
echo "MSSQL_DATABASE: $MSSQL_DATABASE"
echo "MSSQL_USER: $MSSQL_USER"
echo "MSSQL_PASSWORD: ********"

# Build and run the application
echo "Building and running the application with .NET 8.0..."
dotnet run --framework net8.0 