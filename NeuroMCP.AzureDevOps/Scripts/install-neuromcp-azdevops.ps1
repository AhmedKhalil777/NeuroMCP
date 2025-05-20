[CmdletBinding()]
param (
    [Parameter()]
    [switch]$UseLocalPackage,
    
    [Parameter()]
    [string]$Version,
    
    [Parameter()]
    [switch]$AsService,
    
    [Parameter()]
    [string]$ServiceName = "NeuroMCPAzureDevOps",
    
    [Parameter()]
    [int]$Port = 5300,
    
    [Parameter()]
    [string]$OrgUrl,
    
    [Parameter()]
    [string]$DefaultProject,
    
    [Parameter()]
    [string]$PatToken,
    
    [Parameter()]
    [string]$AuthType = "pat",
    
    [Parameter()]
    [string]$TenantId,
    
    [Parameter()]
    [string]$ClientId
)

# Ensure running as administrator for service installation
if ($AsService -and -not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "You need to run this script as an Administrator when installing as a service."
    exit 1
}

Write-Host "Uninstalling existing NeuroMCP.AzureDevOps tool..."
dotnet tool uninstall --global NeuroMCP.AzureDevOps

Write-Host "Installing NeuroMCP.AzureDevOps tool..."
if ($UseLocalPackage) {
    $packagePath = Join-Path $PSScriptRoot "..\nupkg"
    $package = Get-ChildItem -Path $packagePath -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($null -eq $package) {
        Write-Error "No package found in $packagePath. Build the project first with 'dotnet pack'."
        exit 1
    }
    
    Write-Host "Installing from local package: $($package.FullName)"
    dotnet tool install --global --add-source $packagePath NeuroMCP.AzureDevOps
}
elseif ($Version) {
    Write-Host "Installing version $Version from NuGet..."
    dotnet tool install --global NeuroMCP.AzureDevOps --version $Version
}
else {
    Write-Host "Installing latest version from NuGet..."
    dotnet tool install --global NeuroMCP.AzureDevOps
}

# Set up environment variables if provided
if ($OrgUrl -or $DefaultProject -or $PatToken -or $AuthType -or $TenantId -or $ClientId) {
    Write-Host "Setting environment variables..."
    
    $envPath = Join-Path $env:USERPROFILE ".neuromcp"
    if (-not (Test-Path $envPath)) {
        New-Item -ItemType Directory -Path $envPath | Out-Null
    }
    
    $envFile = Join-Path $envPath "azdevops.env"
    
    $envContent = @()
    
    if ($OrgUrl) {
        $envContent += "NEUROMCP_AZDEVOPS_ORG_URL=$OrgUrl"
    }
    
    if ($DefaultProject) {
        $envContent += "NEUROMCP_AZDEVOPS_PROJECT=$DefaultProject"
    }
    
    if ($PatToken) {
        $envContent += "NEUROMCP_AZDEVOPS_PAT=$PatToken"
    }
    
    if ($AuthType) {
        $envContent += "NEUROMCP_AZDEVOPS_AUTH_TYPE=$AuthType"
    }
    
    if ($TenantId) {
        $envContent += "NEUROMCP_AZDEVOPS_TENANT_ID=$TenantId"
    }
    
    if ($ClientId) {
        $envContent += "NEUROMCP_AZDEVOPS_CLIENT_ID=$ClientId"
    }
    
    $envContent | Out-File -FilePath $envFile -Force
    
    Write-Host "Environment variables saved to $envFile"
}

if ($AsService) {
    Write-Host "Installing as Windows service: $ServiceName"
    
    # Get the path to the installed tool
    $toolPath = [System.IO.Path]::Combine($env:USERPROFILE, ".dotnet", "tools", "neuromcp-azdevops.exe")
    
    if (-not (Test-Path $toolPath)) {
        Write-Error "Tool executable not found at expected path: $toolPath"
        exit 1
    }
    
    # Stop and remove the service if it already exists
    if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
        Write-Host "Stopping and removing existing service..."
        Stop-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 2
        sc.exe delete $ServiceName
    }
    
    # Create the service
    $serviceBinPath = "`"$toolPath`" --port $Port"
    sc.exe create $ServiceName binPath= $serviceBinPath start= auto DisplayName= "NeuroMCP Azure DevOps Service"
    
    # Start the service
    Write-Host "Starting service..."
    Start-Service -Name $ServiceName
    
    # Check service status
    $serviceStatus = (Get-Service -Name $ServiceName).Status
    Write-Host "Service status: $serviceStatus"
    
    if ($serviceStatus -eq "Running") {
        Write-Host "NeuroMCP Azure DevOps service installed and running successfully on port $Port."
    }
    else {
        Write-Warning "Service is not running. Check the Windows Event Log for errors."
    }
}
else {
    # Just display how to start the tool
    Write-Host "`nNeuroMCP Azure DevOps installed successfully!"
    Write-Host "To start the server: neuromcp-azdevops --port $Port"
}

Write-Host "Done!" 