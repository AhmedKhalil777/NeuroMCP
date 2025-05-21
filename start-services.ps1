# Start AzureDevOps MCP Service
Start-Process -FilePath "neuromcp-azdevops" -ArgumentList "--port", "5400" -WindowStyle Normal

# Start SQL Server MCP Service (uncomment if needed)
# Start-Process -FilePath "neuromcp-sqlserver" -ArgumentList "--port", "5200" -WindowStyle Normal

Write-Host "Started NeuroMCP services:" -ForegroundColor Green
Write-Host "  - Azure DevOps MCP: http://localhost:5400/mcp" -ForegroundColor Cyan
Write-Host "  - Azure DevOps Swagger UI: http://localhost:5400/swagger/index.html" -ForegroundColor Cyan
# Write-Host "  - SQL Server MCP: http://localhost:5200/mcp" -ForegroundColor Cyan

Write-Host "`nPress Ctrl+C to exit this script, but services will continue running in the background." -ForegroundColor Yellow 