@echo off
echo Uninstalling NeuroMCP.SqlServer...
dotnet tool uninstall --global NeuroMCP.SqlServer
echo Installing NeuroMCP.SqlServer version 1.1.4...
dotnet tool install --global NeuroMCP.SqlServer --version 1.1.4
echo Done!
pause 