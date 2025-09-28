@echo off
echo ========================================
echo   Building Castle Story Modding Tool
echo ========================================
echo.

echo Building all components...
echo.

REM Build main launcher
echo [1/5] Building Castle Story Launcher...
cd Components\CastleStoryLauncher
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build Castle Story Launcher
    pause
    exit /b 1
)
cd ..\..

REM Build LAN Server
echo [2/5] Building LAN Server...
cd Components\LANServer
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build LAN Server
    pause
    exit /b 1
)
cd ..\..

REM Build LAN Client
echo [3/5] Building LAN Client...
cd Components\LANClient
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build LAN Client
    pause
    exit /b 1
)
cd ..\..

REM Build Easy Launcher
echo [4/5] Building Easy Launcher...
cd EasyLauncher
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build Easy Launcher
    pause
    exit /b 1
)
cd ..

REM Build Multiplayer Server
echo [5/6] Building Multiplayer Server...
cd Components\MultiplayerServer
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build Multiplayer Server
    pause
    exit /b 1
)
cd ..\..

REM Build Multiplayer Mod
echo [6/6] Building Multiplayer Mod...
cd Components\Mods\MultiplayerMod
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build Multiplayer Mod
    pause
    exit /b 1
)
cd ..\..\..

echo.
echo ========================================
echo   Build Complete!
echo ========================================
echo.
echo All components have been built successfully.
echo.
echo Available launchers:
echo - EasyLauncher.exe (recommended - with auto-update)
echo - CastleStoryLauncher.exe (main launcher)
echo - LAN Server and Client (LAN multiplayer)
echo.
echo To create a release package, run: CreateRelease.bat
echo.
echo Press any key to launch the Easy Launcher...
pause >nul

REM Launch Easy Launcher
if exist "EasyLauncher\bin\Release\net9.0-windows\win-x64\EasyLauncher.exe" (
    start "" "EasyLauncher\bin\Release\net9.0-windows\win-x64\EasyLauncher.exe"
) else (
    echo Easy Launcher not found. Please check the build.
)
