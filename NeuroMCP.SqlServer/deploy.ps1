param (
    [string]$SqlServer = "ddd",
    [string]$SqlDatabase = "d",
    [string]$SqlUser = "d",
    [string]$SqlPassword = "d#ddd"
)

# Set environment variables
$env:MSSQL_SERVER = $SqlServer
$env:MSSQL_DATABASE = $SqlDatabase
$env:MSSQL_USER = $SqlUser
$env:MSSQL_PASSWORD = $SqlPassword

Write-Host "Environment variables set:"
Write-Host "MSSQL_SERVER: $env:MSSQL_SERVER"
Write-Host "MSSQL_DATABASE: $env:MSSQL_DATABASE"
Write-Host "MSSQL_USER: $env:MSSQL_USER"
Write-Host "MSSQL_PASSWORD: ********"

# Build and run the application
Write-Host "Building and running the application with .NET 8.0..."
dotnet run --framework net8.0

# Script will not get here until the application is stopped 