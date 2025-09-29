@echo off
echo Starting Castle Story LAN Server...
echo.

REM Change to the directory where this batch file is located
cd /d "%~dp0"

REM Check if the executable exists
if not exist "bin\Release\LANServer.exe" (
    echo ERROR: LANServer.exe not found!
    echo Please build the project first using build-all.bat
    echo.
    pause
    exit /b 1
)

REM Start the LAN Server
echo Launching LAN Server...
start "Castle Story LAN Server" "bin\Release\LANServer.exe"

echo LAN Server launched successfully!
echo Check the server window for status and commands.
echo.
echo Press any key to close this window...
pause >nul
