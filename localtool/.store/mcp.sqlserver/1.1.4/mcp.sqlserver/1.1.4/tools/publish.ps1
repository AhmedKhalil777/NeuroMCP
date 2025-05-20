param (
    [string]$Version = "",
    [string]$ApiKey = "",
    [switch]$SkipBuild,
    [switch]$LocalOnly
)

# Set working directory to the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptDir
Set-Location $projectDir

if (-not $Version) {
    # Try to get version from csproj
    $csprojContent = Get-Content -Path "$projectDir\MCP.SqlServer.csproj" -Raw
    if ($csprojContent -match '<Version>(.*?)</Version>') {
        $Version = $matches[1]
        Write-Host "Using version from csproj: $Version"
    } else {
        $Version = "1.0.0"
        Write-Host "No version found in csproj, using default: $Version"
    }
}

# Ensure nupkg directory exists
$nupkgDir = "$projectDir\nupkg"
if (-not (Test-Path $nupkgDir)) {
    New-Item -ItemType Directory -Path $nupkgDir | Out-Null
    Write-Host "Created directory: $nupkgDir"
}

# Build and pack the project
if (-not $SkipBuild) {
    Write-Host "Building and packing MCP.SqlServer v$Version..."
    dotnet pack -c Release /p:Version=$Version
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to pack the project."
        exit 1
    }
}

# Check if the .nupkg file exists
$nupkgFile = "$nupkgDir\MCP.SqlServer.$Version.nupkg"
if (-not (Test-Path $nupkgFile)) {
    Write-Error "Package file not found: $nupkgFile"
    exit 1
}

# Install locally for testing
Write-Host "Installing package locally for testing..."
dotnet tool uninstall --global MCP.SqlServer 2>$null
dotnet tool install --global --add-source $nupkgDir MCP.SqlServer

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to install the package locally."
    exit 1
}

# Verify installation
Write-Host "Verifying installation..."
$installedTool = Get-Command mcp-mssql -ErrorAction SilentlyContinue
if (-not $installedTool) {
    Write-Error "Tool not found after installation. Please check the package."
    exit 1
}

Write-Host "Tool installed successfully at: $($installedTool.Source)"
Write-Host "Testing tool by getting version info..."
mcp-mssql --version

if ($LocalOnly) {
    Write-Host "Package installed locally only. Skipping NuGet.org publishing."
    exit 0
}

# Publish to NuGet.org
if (-not $ApiKey) {
    $ApiKey = Read-Host "Enter your NuGet.org API key"
}

if (-not $ApiKey) {
    Write-Error "API key is required to publish to NuGet.org."
    Write-Host "You can still manually publish using: dotnet nuget push $nupkgFile --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY"
    exit 1
}

Write-Host "Publishing to NuGet.org..."
dotnet nuget push $nupkgFile --source https://api.nuget.org/v3/index.json --api-key $ApiKey

if ($LASTEXITCODE -eq 0) {
    Write-Host "Package published successfully to NuGet.org!"
    Write-Host "Package URL: https://www.nuget.org/packages/MCP.SqlServer/$Version"
} else {
    Write-Error "Failed to publish package to NuGet.org."
    exit 1
} 