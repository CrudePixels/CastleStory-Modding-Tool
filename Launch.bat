@echo off
title Castle Story Modding Tool
color 0A

echo.
echo  ██████╗ █████╗ ███████╗████████╗██╗     ███████╗
echo ██╔════╝██╔══██╗██╔════╝╚══██╔══╝██║     ██╔════╝
echo ██║     ███████║███████╗   ██║   ██║     ███████╗
echo ██║     ██╔══██║╚════██║   ██║   ██║     ╚════██║
echo ╚██████╗██║  ██║███████║   ██║   ███████╗███████║
echo  ╚═════╝╚═╝  ╚═╝╚══════╝   ╚═╝   ╚══════╝╚══════╝
echo.
echo           🏰 Castle Story Modding Tool 🏰
echo.
echo ================================================
echo.

REM Check if Easy Launcher exists
if exist "EasyLauncher\bin\Release\net9.0-windows\win-x64\EasyLauncher.exe" (
    echo Starting Easy Launcher...
    start "" "EasyLauncher\bin\Release\net9.0-windows\win-x64\EasyLauncher.exe"
    exit /b 0
)

REM Check if main launcher exists
if exist "Components\CastleStoryLauncher\bin\Release\net9.0-windows\CastleStoryLauncher.exe" (
    echo Starting Castle Story Launcher...
    start "" "Components\CastleStoryLauncher\bin\Release\net9.0-windows\CastleStoryLauncher.exe"
    exit /b 0
)

REM If nothing exists, offer to build
echo No launcher found. Would you like to build the components?
echo.
echo 1. Build all components
echo 2. Exit
echo.
set /p choice="Enter your choice (1-2): "

if "%choice%"=="1" (
    echo.
    echo Building all components...
    call build-all.bat
) else (
    echo.
    echo Exiting...
    exit /b 0
)

pause
