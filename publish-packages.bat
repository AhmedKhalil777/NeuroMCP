@echo off
set /p API_KEY=Enter your NuGet API key: 
powershell -ExecutionPolicy Bypass -File publish-packages.ps1 -ApiKey "%API_KEY%"
pause 