@echo off
echo ========================================
echo   Building and Packaging for GitHub
echo ========================================
echo.

REM Build all .NET components
echo [1/6] Building all .NET components...
call "%~dp0build-all.bat"
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Some components failed to build, continuing anyway...
)

REM Create release directory
echo [2/6] Creating release directory...
REM Change to project root (two levels up from scripts/build)
pushd ..\..
if exist "Release" rmdir /s /q "Release"
mkdir "Release"
popd

REM Build and copy Easy Launcher (self-contained)
echo [3/6] Building Easy Launcher (self-contained)...
pushd ..\..
dotnet publish "EasyLauncher\EasyLauncher.csproj" --configuration Release --runtime win-x64 --self-contained true --output "Release\EasyLauncher-Temp" 2>nul
if exist "Release\EasyLauncher-Temp\EasyLauncher.exe" (
    copy "Release\EasyLauncher-Temp\EasyLauncher.exe" "Release\" >nul
    echo Easy Launcher built successfully
) else (
    echo WARNING: Easy Launcher build failed
)
if exist "Release\EasyLauncher-Temp" rmdir /s /q "Release\EasyLauncher-Temp"

REM Build and copy Castle Story Launcher (self-contained)
echo [4/6] Building Castle Story Launcher (self-contained)...
dotnet publish "Components\CastleStoryLauncher\CastleStoryLauncher.csproj" --configuration Release --runtime win-x64 --self-contained true --output "Release\CastleStoryLauncher-Temp" 2>nul
if exist "Release\CastleStoryLauncher-Temp\CastleStoryLauncher.exe" (
    mkdir "Release\Components\CastleStoryLauncher" 2>nul
    xcopy "Release\CastleStoryLauncher-Temp\*" "Release\Components\CastleStoryLauncher\" /E /I /Y >nul
    echo Castle Story Launcher built successfully
) else (
    echo WARNING: Castle Story Launcher build failed
)
if exist "Release\CastleStoryLauncher-Temp" rmdir /s /q "Release\CastleStoryLauncher-Temp"

REM Build and copy LAN Server (self-contained)
echo [5/6] Building LAN Server (self-contained)...
dotnet publish "Components\LANServer\LANServer.csproj" --configuration Release --runtime win-x64 --self-contained true --output "Release\LANServer-Temp" 2>nul
if exist "Release\LANServer-Temp\LANServer.exe" (
    mkdir "Release\Components\LANServer" 2>nul
    xcopy "Release\LANServer-Temp\*" "Release\Components\LANServer\" /E /I /Y >nul
    echo LAN Server built successfully
) else (
    echo WARNING: LAN Server build failed
)
if exist "Release\LANServer-Temp" rmdir /s /q "Release\LANServer-Temp"

REM Build and copy LAN Client (self-contained)
echo [6/6] Building LAN Client (self-contained)...
dotnet publish "Components\LANClient\LANClient.csproj" --configuration Release --runtime win-x64 --self-contained true --output "Release\LANClient-Temp" 2>nul
if exist "Release\LANClient-Temp\LANClient.exe" (
    mkdir "Release\Components\LANClient" 2>nul
    xcopy "Release\LANClient-Temp\*" "Release\Components\LANClient\" /E /I /Y >nul
    echo LAN Client built successfully
) else (
    echo WARNING: LAN Client build failed
)
if exist "Release\LANClient-Temp" rmdir /s /q "Release\LANClient-Temp"

REM Copy Electron app files
echo [7/7] Copying Electron app files...
REM Change to project root (two levels up from scripts/build)
pushd ..\..
mkdir "Release\ElectronApp" 2>nul
xcopy "src\*" "Release\ElectronApp\src\" /E /I /Y >nul
xcopy "package.json" "Release\ElectronApp\" /Y >nul
if exist "node_modules" xcopy "node_modules\*" "Release\ElectronApp\node_modules\" /E /I /Y >nul
popd

REM Copy version and README
pushd ..\..
copy "version.txt" "Release\" >nul
if exist "README.md" copy "README.md" "Release\" >nul
popd

REM Create logs directory
mkdir "Release\logs" 2>nul

REM Create zip file
echo.
echo Creating ZIP archive...
set VERSION=1.6.0
set ZIPNAME=CastleStoryModdingTool_v%VERSION%.zip

REM Remove old zip if exists
if exist "%ZIPNAME%" del "%ZIPNAME%"

REM Create zip using PowerShell
pushd ..\..
powershell -Command "Compress-Archive -Path 'Release\*' -DestinationPath '%ZIPNAME%' -Force"
popd

if exist "%ZIPNAME%" (
    echo.
    echo ========================================
    echo   Build Complete!
    echo ========================================
    echo.
    echo Release package created: %ZIPNAME%
    echo Release folder: Release\
    echo.
    echo Ready for GitHub release upload!
) else (
    echo.
    echo ERROR: Failed to create ZIP file
    echo Release files are in the 'Release' folder
)

echo.
pause
