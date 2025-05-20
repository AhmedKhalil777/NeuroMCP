#!/usr/bin/env pwsh
# Test script for local package building and publishing to a local NuGet feed

param (
    [string]$Version = "1.0.0-local.$([DateTime]::Now.ToString('yyyyMMddHHmm'))",
    [string]$OutputPath = "..\nupkg"
)

# Ensure output directory exists
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
    Write-Host "Created output directory: $OutputPath"
}

# Update version in project file
Write-Host "Updating version to $Version..."
& "$PSScriptRoot\update-version.ps1" -Version $Version

# Build and pack the project
Write-Host "Building and packing project..."
dotnet build -c Release
dotnet pack -c Release -o $OutputPath

# List generated packages
Write-Host "`nGenerated packages:"
Get-ChildItem $OutputPath -Filter "*.nupkg" | Select-Object Name, Length, LastWriteTime

# Instructions for testing
Write-Host "`nTo test with the local package:"
Write-Host "dotnet tool uninstall --global MCP.SqlServer"
Write-Host "dotnet tool install --global --add-source $OutputPath MCP.SqlServer"
Write-Host "`nTo run the tool:"
Write-Host "mcp-mssql --port 5200" 