@echo off
echo Creating Castle Story Modding Tool Release Package...
echo.

REM Create release directory
if exist "Release" rmdir /s /q "Release"
mkdir "Release"

REM Copy main launcher
echo Copying Easy Launcher...
copy "EasyLauncher\bin\Release\net9.0-windows\win-x64\EasyLauncher.exe" "Release\"
copy "EasyLauncher\bin\Release\net9.0-windows\win-x64\EasyLauncher.dll" "Release\"

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
copy "Components\MultiplayerServer\bin\Release\net9.0-windows\MultiplayerServer.exe" "Release\Components\MultiplayerServer\"
copy "Components\MultiplayerServer\bin\Release\net9.0-windows\MultiplayerServer.dll" "Release\Components\MultiplayerServer\"

REM Copy Multiplayer Mod
echo Copying Multiplayer Mod...
mkdir "Release\Components\Mods\MultiplayerMod"
copy "Components\Mods\MultiplayerMod\bin\Release\net9.0-windows\MultiplayerMod.dll" "Release\Components\Mods\MultiplayerMod\"
copy "Components\Mods\MultiplayerMod\mod.json" "Release\Components\Mods\MultiplayerMod\"

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
