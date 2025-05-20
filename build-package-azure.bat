@echo off
echo Building NeuroMCP.AzureDevOps...
dotnet build NeuroMCP.AzureDevOps\NeuroMCP.AzureDevOps.csproj -c Release
if %ERRORLEVEL% neq 0 (
    echo Build failed
    exit /b %ERRORLEVEL%
)

echo Creating NuGet package...
dotnet pack NeuroMCP.AzureDevOps\NeuroMCP.AzureDevOps.csproj -c Release --no-build
if %ERRORLEVEL% neq 0 (
    echo Package creation failed
    exit /b %ERRORLEVEL%
)

echo Package created successfully. Check the NeuroMCP.AzureDevOps\nupkg directory for the package. 