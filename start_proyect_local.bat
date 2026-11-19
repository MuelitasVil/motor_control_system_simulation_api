@echo off
setlocal

set "PROJECT_ROOT=%~dp0"
set "BACKEND_DIR=%PROJECT_ROOT%back_end_control_system_simulation"
set "BACKEND_PROJECT=%BACKEND_DIR%\motor_control_system_simulation_api\motor_control_system_simulation_api.csproj"
set "FRONTEND_DIR=%PROJECT_ROOT%front_end\motor_control_system"

if not exist "%BACKEND_PROJECT%" (
    echo Backend project was not found:
    echo "%BACKEND_PROJECT%"
    pause
    exit /b 1
)

if not exist "%FRONTEND_DIR%\package.json" (
    echo Frontend project was not found:
    echo "%FRONTEND_DIR%"
    pause
    exit /b 1
)

if not exist "%FRONTEND_DIR%\node_modules" (
    echo Installing frontend dependencies. This can take a few minutes...
    pushd "%FRONTEND_DIR%"
    call npm install
    if errorlevel 1 (
        echo Frontend dependency installation failed. Check that Node.js and npm are installed.
        popd
        pause
        exit /b 1
    )
    popd
)

start "Motor Control API" /D "%BACKEND_DIR%" cmd /k "dotnet run --project motor_control_system_simulation_api\motor_control_system_simulation_api.csproj"
start "Motor Control Frontend" /D "%FRONTEND_DIR%" cmd /k "npm run dev"

echo Backend and frontend startup commands were launched in separate windows.
echo Frontend: http://localhost:5173
echo API:      http://localhost:5168
endlocal
