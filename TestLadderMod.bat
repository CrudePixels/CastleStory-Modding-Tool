@echo off
SETLOCAL

echo ========================================
echo  Testing Ladder Mod
echo ========================================
echo.

echo Building LadderMod...
dotnet build Components/Mods/LadderMod/LadderMod.csproj --configuration Release

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Failed to build LadderMod
    pause
    exit /b 1
)

echo ✅ LadderMod built successfully!
echo.

echo Copying LadderMod to Mods directory...
copy "Components\Mods\LadderMod\bin\Release\net8.0\LadderMod.dll" "Mods\LadderMod.dll" >nul
copy "Components\Mods\LadderMod\mod.json" "Mods\LadderMod.json" >nul

echo ✅ LadderMod copied to Mods directory!
echo.

echo ========================================
echo  Ladder Mod Ready!
echo ========================================
echo.
echo The LadderMod is now ready to be used in the Castle Story Launcher!
echo.
echo To use it:
echo 1. Run the Castle Story Launcher
echo 2. The LadderMod should appear in the Mod Manager
echo 3. Select it and click "Apply Selected Mods"
echo 4. Start Castle Story - ladders will be automatically injected!
echo.
echo The mod will:
echo - Add 4 ladder types (Wooden, Iron, Stone, Rope)
echo - Create a "Ladders" category in the Structures tab
echo - Work automatically when Castle Story starts
echo.

pause
ENDLOCAL
