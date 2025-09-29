@echo off
echo Castle Story Ladder Integration Tool
echo ====================================
echo.

cd /d "%~dp0"

echo Building Ladder Integration Tool...
dotnet build Components\Mods\LadderMod\LadderMod.csproj -c Release

if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ Build failed! Please check for errors.
    pause
    exit /b 1
)

echo.
echo Running Ladder Integration...
echo.

dotnet run --project Components\Mods\LadderMod\LadderMod.csproj --configuration Release

echo.
echo Integration complete!
pause
