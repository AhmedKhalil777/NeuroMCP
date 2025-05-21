# Stop AzureDevOps MCP Service
Stop-Process -Name "neuromcp-azdevops" -Force -ErrorAction SilentlyContinue
if ($?) {
    Write-Host "Azure DevOps MCP service stopped successfully" -ForegroundColor Green
} else {
    Write-Host "Azure DevOps MCP service was not running" -ForegroundColor Yellow
}

# Stop SQL Server MCP Service (uncomment if needed)
# Stop-Process -Name "neuromcp-sqlserver" -Force -ErrorAction SilentlyContinue
# if ($?) {
#     Write-Host "SQL Server MCP service stopped successfully" -ForegroundColor Green
# } else {
#     Write-Host "SQL Server MCP service was not running" -ForegroundColor Yellow
# }

Write-Host "All NeuroMCP services have been stopped" -ForegroundColor Green 