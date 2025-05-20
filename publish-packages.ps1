[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [Parameter()]
    [string]$NuGetSource = "https://api.nuget.org/v3/index.json",
    
    [Parameter()]
    [switch]$SkipBuild,
    
    [Parameter()]
    [switch]$SkipPack
)

# Packages to build and publish
$packages = @(
    "NeuroMCP.SqlServer",
    "NeuroMCP.AzureDevOps"
)

# Save API key for future use (optional)
dotnet nuget add source $NuGetSource --name "NuGetOfficial" --store-password-in-clear-text
dotnet nuget setapikey $ApiKey --source "NuGetOfficial"

foreach ($package in $packages) {
    Write-Host "Processing package: $package" -ForegroundColor Cyan
    
    if (-not $SkipBuild) {
        # Build the package
        Write-Host "Building $package..." -ForegroundColor Yellow
        dotnet build "$package/$package.csproj" -c Release
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed for $package"
            continue
        }
    }
    
    if (-not $SkipPack) {
        # Pack the package
        Write-Host "Packing $package..." -ForegroundColor Yellow
        dotnet pack "$package/$package.csproj" -c Release --no-build
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Pack failed for $package"
            continue
        }
    }
}

# Publish all packages using glob pattern
Write-Host "Publishing all packages to $NuGetSource..." -ForegroundColor Green

# Use globbing to find and push all nupkg files
$packagesPath = "./**/bin/Release/*.nupkg"
dotnet nuget push $packagesPath --api-key $ApiKey --source $NuGetSource --skip-duplicate

Write-Host "Package publishing completed!" -ForegroundColor Green 