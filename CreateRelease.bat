@echo off
echo Creating Castle Story Modding Tool Release Package...
echo.

REM Create release directory
if exist "Release" rmdir /s /q "Release"
mkdir "Release"

REM Build and copy main launcher (self-contained)
echo Building Easy Launcher (self-contained)...
dotnet publish "EasyLauncher\EasyLauncher.csproj" --configuration Release --runtime win-x64 --self-contained true --output "Release\EasyLauncher-Temp"
copy "Release\EasyLauncher-Temp\EasyLauncher.exe" "Release\"
rmdir /s /q "Release\EasyLauncher-Temp"

REM Copy Castle Story Launcher
echo Copying Castle Story Launcher...
mkdir "Release\Components\CastleStoryLauncher"
copy "Components\CastleStoryLauncher\bin\Release\net9.0-windows\CastleStoryLauncher.exe" "Release\Components\CastleStoryLauncher\"
copy "Components\CastleStoryLauncher\bin\Release\net9.0-windows\CastleStoryLauncher.dll" "Release\Components\CastleStoryLauncher\"

REM Copy LAN Server
echo Copying LAN Server...
mkdir "Release\Components\LANServer"
copy "Components\LANServer\bin\Release\net9.0-windows\LANServer.exe" "Release\Components\LANServer\"
copy "Components\LANServer\bin\Release\net9.0-windows\LANServer.dll" "Release\Components\LANServer\"

REM Copy LAN Client
echo Copying LAN Client...
mkdir "Release\Components\LANClient"
copy "Components\LANClient\bin\Release\net9.0-windows\LANClient.exe" "Release\Components\LANClient\"
copy "Components\LANClient\bin\Release\net9.0-windows\LANClient.dll" "Release\Components\LANClient\"

REM Copy Multiplayer Server
echo Copying Multiplayer Server...
mkdir "Release\Components\MultiplayerServer"
if exist "Components\MultiplayerServer\bin\Release\net6.0\MultiplayerServer.exe" (
    copy "Components\MultiplayerServer\bin\Release\net6.0\MultiplayerServer.exe" "Release\Components\MultiplayerServer\"
    copy "Components\MultiplayerServer\bin\Release\net6.0\MultiplayerServer.dll" "Release\Components\MultiplayerServer\"
) else (
    echo Multiplayer Server not found, skipping...
)

REM Copy Multiplayer Mod
echo Copying Multiplayer Mod...
mkdir "Release\Components\Mods\MultiplayerMod"
if exist "Components\Mods\MultiplayerMod\bin\Release\net6.0\MultiplayerMod.dll" (
    copy "Components\Mods\MultiplayerMod\bin\Release\net6.0\MultiplayerMod.dll" "Release\Components\Mods\MultiplayerMod\"
    copy "Components\Mods\MultiplayerMod\mod.json" "Release\Components\Mods\MultiplayerMod\"
) else (
    echo Multiplayer Mod not found, skipping...
)

REM Copy version file
copy "version.txt" "Release\"

REM Copy README
if exist "README.md" copy "README.md" "Release\"

REM Create logs directory
mkdir "Release\logs"

echo.
echo Release package created in 'Release' folder
echo Ready for GitHub release upload!
echo.
pause
