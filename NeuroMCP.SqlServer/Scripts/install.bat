@echo off
setlocal enabledelayedexpansion

:: Default values
set PORT=5200
set SERVICE_NAME=MCPSqlServer
set INSTALL_SERVICE=false
set UNINSTALL_SERVICE=false
set INSTALL=false
set UPDATE=false
set FORCE=false

:: Parse arguments
:parse_args
if "%~1"=="" goto :done_parsing
if /i "%~1"=="--port" (
  set PORT=%~2
  shift
  shift
  goto :parse_args
)
if /i "%~1"=="--service-name" (
  set SERVICE_NAME=%~2
  shift
  shift
  goto :parse_args
)
if /i "%~1"=="--install-service" (
  set INSTALL_SERVICE=true
  shift
  goto :parse_args
)
if /i "%~1"=="--uninstall-service" (
  set UNINSTALL_SERVICE=true
  shift
  goto :parse_args
)
if /i "%~1"=="--install" (
  set INSTALL=true
  shift
  goto :parse_args
)
if /i "%~1"=="--update" (
  set UPDATE=true
  shift
  goto :parse_args
)
if /i "%~1"=="--force" (
  set FORCE=true
  shift
  goto :parse_args
)
echo Unknown option: %~1
exit /b 1

:done_parsing

:: Check if running as administrator when installing/uninstalling services
if "%INSTALL_SERVICE%"=="true" (
  goto :check_admin
)
if "%UNINSTALL_SERVICE%"=="true" (
  goto :check_admin
)
goto :skip_admin_check

:check_admin
echo Checking for administrator rights...
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo You need administrator rights to install or uninstall the service.
  echo Please run this script as an administrator.
  exit /b 1
)

:skip_admin_check

:: Install or update the tool
if "%INSTALL%"=="true" goto :do_install
if "%UPDATE%"=="true" goto :do_install
goto :skip_install

:do_install
if "%UPDATE%"=="true" (
  echo Uninstalling existing MCP.SqlServer tool...
  dotnet tool uninstall --global MCP.SqlServer 2>nul
) else if "%FORCE%"=="true" (
  echo Uninstalling existing MCP.SqlServer tool...
  dotnet tool uninstall --global MCP.SqlServer 2>nul
)

echo Installing MCP.SqlServer tool...
dotnet tool install --global MCP.SqlServer

if %errorlevel% neq 0 (
  echo Failed to install the tool.
  exit /b 1
)

where mcp-mssql >nul 2>&1
if %errorlevel% neq 0 (
  echo Could not find the installed tool.
  exit /b 1
)

for /f "tokens=*" %%i in ('where mcp-mssql') do set TOOL_PATH=%%i
echo MCP.SqlServer tool installed successfully at: !TOOL_PATH!
goto :after_install

:skip_install
where mcp-mssql >nul 2>&1
if %errorlevel% neq 0 (
  echo MCP.SqlServer tool not found. Installing...
  dotnet tool install --global MCP.SqlServer

  if %errorlevel% neq 0 (
    echo Failed to install the tool.
    exit /b 1
  )

  where mcp-mssql >nul 2>&1
  if %errorlevel% neq 0 (
    echo Could not find the installed tool.
    exit /b 1
  )

  for /f "tokens=*" %%i in ('where mcp-mssql') do set TOOL_PATH=%%i
  echo MCP.SqlServer tool installed successfully at: !TOOL_PATH!
) else (
  for /f "tokens=*" %%i in ('where mcp-mssql') do set TOOL_PATH=%%i
  echo Using existing installation at: !TOOL_PATH!
)

:after_install

:: Install as service
if "%INSTALL_SERVICE%"=="true" (
  echo Installing MCP.SqlServer as a Windows service...
  mcp-mssql --install --service-name %SERVICE_NAME% --port %PORT%

  if %errorlevel% neq 0 (
    echo Failed to install the service.
    exit /b 1
  )

  echo Starting the service...
  net start %SERVICE_NAME%

  if %errorlevel% equ 0 (
    echo Service installed and started successfully.
    echo The service is now running on http://localhost:%PORT%
  ) else (
    echo Failed to start the service.
    exit /b 1
  )
)

:: Uninstall service
if "%UNINSTALL_SERVICE%"=="true" (
  echo Stopping and removing the Windows service...
  net stop %SERVICE_NAME% 2>nul
  mcp-mssql --uninstall --service-name %SERVICE_NAME%

  if %errorlevel% neq 0 (
    echo Failed to uninstall the service.
    exit /b 1
  )

  echo Service uninstalled successfully.
)

:: If not installing as a service and not uninstalling, just run the tool
if "%INSTALL_SERVICE%"=="false" (
  if "%UNINSTALL_SERVICE%"=="false" (
    echo Starting MCP.SqlServer on port %PORT%...
    mcp-mssql --port %PORT%
  )
)

endlocal 