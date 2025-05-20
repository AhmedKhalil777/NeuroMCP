@echo off
dotnet build NeuroMCP.AzureDevOps/NeuroMCP.AzureDevOps.csproj > build-output.log 2>&1
echo Build complete. Check build-output.log for results. 