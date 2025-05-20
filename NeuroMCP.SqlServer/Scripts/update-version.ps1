param (
    [Parameter(Mandatory=$true)]
    [string]$Version
)

# Get the project file path
$ProjectFile = Join-Path $PSScriptRoot "..\MCP.SqlServer.csproj"

# Check if file exists
if (!(Test-Path $ProjectFile)) {
    Write-Error "Project file not found at: $ProjectFile"
    exit 1
}

# Read the project file content
$Content = Get-Content $ProjectFile -Raw

# Update the version
$NewContent = $Content -replace '<Version>(.*?)</Version>', "<Version>$Version</Version>"

# Write the updated content back to the file
$NewContent | Set-Content $ProjectFile

Write-Host "Updated version to $Version in $ProjectFile" 