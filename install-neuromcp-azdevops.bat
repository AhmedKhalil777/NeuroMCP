@echo off
echo Uninstalling NeuroMCP.AzureDevOps...
dotnet tool uninstall --global NeuroMCP.AzureDevOps
echo Installing NeuroMCP.AzureDevOps version 1.0.0...
dotnet tool install --global NeuroMCP.AzureDevOps --version 1.0.0
echo Done!
pause 