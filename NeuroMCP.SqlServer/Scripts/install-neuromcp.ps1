param (
    [string]$Port = "5200",
    [string]$ServiceName = "NeuroMCPSqlServer",
    [switch]$InstallService,
    [switch]$UninstallService,
    [switch]$Install,
    [switch]$Update,
    [switch]$Force,
    [switch]$UseLocalPackage
)

function Test-Administrator {
    $user = [Security.Principal.WindowsIdentity]::GetCurrent();
    (New-Object Security.Principal.WindowsPrincipal $user).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

# Check for administrator rights if installing as a service
if ($InstallService -or $UninstallService) {
    if (-not (Test-Administrator)) {
        Write-Error "You need administrator rights to install or uninstall the service."
        exit 1
    }
}

# Path to the tool
$toolPath = ""

# Install or update the tool
if ($Install -or $Update) {
    if ($Update -or $Force) {
        Write-Host "Uninstalling existing NeuroMCP.SqlServer tool..."
        dotnet tool uninstall --global NeuroMCP.SqlServer 2>$null
    }
    
    Write-Host "Installing NeuroMCP.SqlServer tool..."
    
    # If using local package
    if ($UseLocalPackage) {
        $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
        $projectDir = Split-Path -Parent $scriptDir
        $localPackagePath = Join-Path $projectDir "nupkg"
        
        if (Test-Path $localPackagePath) {
            dotnet tool install --global --add-source $localPackagePath NeuroMCP.SqlServer
        } else {
            Write-Error "Local package directory not found: $localPackagePath"
            exit 1
        }
    } else {
        dotnet tool install --global NeuroMCP.SqlServer
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install the tool."
        exit 1
    }
    
    $toolPath = (Get-Command neuromcp-mssql -ErrorAction SilentlyContinue).Source
    
    if (-not $toolPath) {
        Write-Error "Could not find the installed tool."
        exit 1
    }
    
    Write-Host "NeuroMCP.SqlServer tool installed successfully at: $toolPath"
}
else {
    $toolPath = (Get-Command neuromcp-mssql -ErrorAction SilentlyContinue).Source
    
    if (-not $toolPath) {
        Write-Host "NeuroMCP.SqlServer tool not found. Installing..."
        
        # If using local package
        if ($UseLocalPackage) {
            $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
            $projectDir = Split-Path -Parent $scriptDir
            $localPackagePath = Join-Path $projectDir "nupkg"
            
            if (Test-Path $localPackagePath) {
                dotnet tool install --global --add-source $localPackagePath NeuroMCP.SqlServer
            } else {
                Write-Error "Local package directory not found: $localPackagePath"
                exit 1
            }
        } else {
            dotnet tool install --global NeuroMCP.SqlServer
        }
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to install the tool."
            exit 1
        }
        
        $toolPath = (Get-Command neuromcp-mssql -ErrorAction SilentlyContinue).Source
        
        if (-not $toolPath) {
            Write-Error "Could not find the installed tool."
            exit 1
        }
        
        Write-Host "NeuroMCP.SqlServer tool installed successfully at: $toolPath"
    }
    else {
        Write-Host "Using existing installation at: $toolPath"
    }
}

# Install as service
if ($InstallService) {
    Write-Host "Installing NeuroMCP.SqlServer as a Windows service..."
    & neuromcp-mssql --install --service-name $ServiceName --port $Port
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install the service."
        exit 1
    }
    
    Write-Host "Starting the service..."
    Start-Service -Name $ServiceName
    
    if ($?) {
        Write-Host "Service installed and started successfully."
        Write-Host "The service is now running on http://localhost:$Port"
    }
    else {
        Write-Error "Failed to start the service."
        exit 1
    }
}

# Uninstall service
if ($UninstallService) {
    Write-Host "Stopping and removing the Windows service..."
    Stop-Service -Name $ServiceName -ErrorAction SilentlyContinue
    & neuromcp-mssql --uninstall --service-name $ServiceName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to uninstall the service."
        exit 1
    }
    
    Write-Host "Service uninstalled successfully."
}

# If not installing as a service and not uninstalling, just run the tool
if (-not $InstallService -and -not $UninstallService) {
    Write-Host "Starting NeuroMCP.SqlServer on port $Port..."
    & neuromcp-mssql --port $Port
} 