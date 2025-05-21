@echo off
echo Creating missing NuGet local folder...
mkdir "C:\Users\Ahmed.Khalil\AppData\Roaming\NuGet\LocalFolder" 2>nul

echo Deleting bin and obj folders for a clean build...
if exist "NeuroMCP.AzureDevOps\bin" rmdir /s /q "NeuroMCP.AzureDevOps\bin"
if exist "NeuroMCP.AzureDevOps\obj" rmdir /s /q "NeuroMCP.AzureDevOps\obj"

echo Cleaning solution...
dotnet clean NeuroMCP.AzureDevOps\NeuroMCP.AzureDevOps.csproj > build-log.txt 2>&1
type build-log.txt

echo Restoring packages using only official NuGet feed...
dotnet restore NeuroMCP.AzureDevOps\NeuroMCP.AzureDevOps.csproj --source https://api.nuget.org/v3/index.json --force --verbosity detailed >> build-log.txt 2>&1
type build-log.txt

echo Building NeuroMCP.AzureDevOps...
dotnet build NeuroMCP.AzureDevOps\NeuroMCP.AzureDevOps.csproj -c Release --no-restore --verbosity detailed >> build-log.txt 2>&1
if %ERRORLEVEL% neq 0 (
    echo Build failed - check build-log.txt for details
    echo Last 20 lines of log:
    powershell -Command "Get-Content build-log.txt -Tail 20"
    exit /b %ERRORLEVEL%
)

echo Creating NuGet package...
dotnet pack NeuroMCP.AzureDevOps\NeuroMCP.AzureDevOps.csproj -c Release --no-build --verbosity detailed >> build-log.txt 2>&1
if %ERRORLEVEL% neq 0 (
    echo Package creation failed - check build-log.txt for details
    echo Last 20 lines of log:
    powershell -Command "Get-Content build-log.txt -Tail 20"
    exit /b %ERRORLEVEL%
)

echo Package created successfully. Check the NeuroMCP.AzureDevOps\nupkg directory for the package.
echo Full build log saved to build-log.txt 