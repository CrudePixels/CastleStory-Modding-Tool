@echo off
echo Building Castle Story Modding Tool - Electron Version
echo.

cd ..\..

echo Installing dependencies...
call npm install
if %errorlevel% neq 0 (
    echo Failed to install dependencies
    pause
    exit /b 1
)

echo.
echo Building Electron app...
call npm run build
if %errorlevel% neq 0 (
    echo Failed to build Electron app
    pause
    exit /b 1
)

echo.
echo Electron app built successfully!
echo Check the 'dist' folder for the built application.
echo.

pause
