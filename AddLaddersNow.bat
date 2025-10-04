@echo off
SETLOCAL

echo ========================================
echo  Castle Story Ladder Injection - NOW!
echo ========================================
echo.

REM Set your Castle Story path here
set "GAME_PATH=D:\MyProjects\CASTLE STORY\Original Castle Story\Castle Story"

echo Using Castle Story path: %GAME_PATH%
echo.

REM Check if path exists
if not exist "%GAME_PATH%\CastleStory.exe" (
    echo ❌ Castle Story not found at: %GAME_PATH%
    echo Please edit this script and set the correct path.
    pause
    exit /b 1
)

echo ✅ Castle Story found!
echo.

REM Check if Castle Story is running
tasklist /FI "IMAGENAME eq CastleStory.exe" 2>NUL | find /I /N "CastleStory.exe" >NUL
if %ERRORLEVEL% EQU 0 (
    echo ⚠️  Castle Story is running. Please close it first for safe injection.
    pause
    exit /b 1
)

echo ✅ Castle Story is not running - safe to inject
echo.

REM Create backup
set "BACKUP_DIR=%GAME_PATH%\backup_%date:~-4,4%%date:~-10,2%%date:~-7,2%_%time:~0,2%%time:~3,2%%time:~6,2%"
set "BACKUP_DIR=%BACKUP_DIR: =0%"
mkdir "%BACKUP_DIR%" 2>NUL

echo 📁 Created backup directory: %BACKUP_DIR%
echo.

REM Backup files
echo 🔄 Backing up original files...
if exist "%GAME_PATH%\Info\Lua\Data_BuildingBlocks.lua" (
    copy "%GAME_PATH%\Info\Lua\Data_BuildingBlocks.lua" "%BACKUP_DIR%\Data_BuildingBlocks.lua" >NUL
    echo ✅ Backed up Data_BuildingBlocks.lua
)

if exist "%GAME_PATH%\Info\Lua\Data_BuildingCategories.lua" (
    copy "%GAME_PATH%\Info\Lua\Data_BuildingCategories.lua" "%BACKUP_DIR%\Data_BuildingCategories.lua" >NUL
    echo ✅ Backed up Data_BuildingCategories.lua
)

echo.

REM Inject ladder category
echo 🔧 Adding ladder category...
set "CATEGORIES_FILE=%GAME_PATH%\Info\Lua\Data_BuildingCategories.lua"
if exist "%CATEGORIES_FILE%" (
    echo -- LadderMod: Adding ladder category >> "%CATEGORIES_FILE%"
    echo BuildingCategories["ladder"] = { >> "%CATEGORIES_FILE%"
    echo     name = "Ladders", >> "%CATEGORIES_FILE%"
    echo     icon = "ladder_category_icon", >> "%CATEGORIES_FILE%"
    echo     order = 10 >> "%CATEGORIES_FILE%"
    echo } >> "%CATEGORIES_FILE%"
    echo. >> "%CATEGORIES_FILE%"
    echo ✅ Added ladder category
) else (
    echo ❌ Data_BuildingCategories.lua not found!
)

REM Inject ladder blocks
echo 🔧 Adding ladder blocks...
set "BLOCKS_FILE=%GAME_PATH%\Info\Lua\Data_BuildingBlocks.lua"
if exist "%BLOCKS_FILE%" (
    echo -- LadderMod: Adding ladder blocks >> "%BLOCKS_FILE%"
    echo. >> "%BLOCKS_FILE%"
    
    REM Wooden Ladder
    echo -- Wooden Ladder >> "%BLOCKS_FILE%"
    echo Data_BuildingBlocks["ladder_wood"] = { >> "%BLOCKS_FILE%"
    echo     name = "Wooden Ladder", >> "%BLOCKS_FILE%"
    echo     category = "ladder", >> "%BLOCKS_FILE%"
    echo     material = "wood", >> "%BLOCKS_FILE%"
    echo     durability = 100, >> "%BLOCKS_FILE%"
    echo     cost = { wood = 2 }, >> "%BLOCKS_FILE%"
    echo     icon = "ladder_wood_icon", >> "%BLOCKS_FILE%"
    echo     model = "ladder_wood_model", >> "%BLOCKS_FILE%"
    echo     climbSpeed = 2.0, >> "%BLOCKS_FILE%"
    echo     maxHeight = 50, >> "%BLOCKS_FILE%"
    echo     canClimb = true, >> "%BLOCKS_FILE%"
    echo     buildable = true, >> "%BLOCKS_FILE%"
    echo     placeable = true >> "%BLOCKS_FILE%"
    echo } >> "%BLOCKS_FILE%"
    echo. >> "%BLOCKS_FILE%"
    
    REM Iron Ladder
    echo -- Iron Ladder >> "%BLOCKS_FILE%"
    echo Data_BuildingBlocks["ladder_iron"] = { >> "%BLOCKS_FILE%"
    echo     name = "Iron Ladder", >> "%BLOCKS_FILE%"
    echo     category = "ladder", >> "%BLOCKS_FILE%"
    echo     material = "iron", >> "%BLOCKS_FILE%"
    echo     durability = 200, >> "%BLOCKS_FILE%"
    echo     cost = { iron = 1, wood = 1 }, >> "%BLOCKS_FILE%"
    echo     icon = "ladder_iron_icon", >> "%BLOCKS_FILE%"
    echo     model = "ladder_iron_model", >> "%BLOCKS_FILE%"
    echo     climbSpeed = 3.0, >> "%BLOCKS_FILE%"
    echo     maxHeight = 75, >> "%BLOCKS_FILE%"
    echo     canClimb = true, >> "%BLOCKS_FILE%"
    echo     buildable = true, >> "%BLOCKS_FILE%"
    echo     placeable = true >> "%BLOCKS_FILE%"
    echo } >> "%BLOCKS_FILE%"
    echo. >> "%BLOCKS_FILE%"
    
    REM Stone Ladder
    echo -- Stone Ladder >> "%BLOCKS_FILE%"
    echo Data_BuildingBlocks["ladder_stone"] = { >> "%BLOCKS_FILE%"
    echo     name = "Stone Ladder", >> "%BLOCKS_FILE%"
    echo     category = "ladder", >> "%BLOCKS_FILE%"
    echo     material = "stone", >> "%BLOCKS_FILE%"
    echo     durability = 300, >> "%BLOCKS_FILE%"
    echo     cost = { stone = 2 }, >> "%BLOCKS_FILE%"
    echo     icon = "ladder_stone_icon", >> "%BLOCKS_FILE%"
    echo     model = "ladder_stone_model", >> "%BLOCKS_FILE%"
    echo     climbSpeed = 1.6, >> "%BLOCKS_FILE%"
    echo     maxHeight = 100, >> "%BLOCKS_FILE%"
    echo     canClimb = true, >> "%BLOCKS_FILE%"
    echo     buildable = true, >> "%BLOCKS_FILE%"
    echo     placeable = true >> "%BLOCKS_FILE%"
    echo } >> "%BLOCKS_FILE%"
    echo. >> "%BLOCKS_FILE%"
    
    REM Rope Ladder
    echo -- Rope Ladder >> "%BLOCKS_FILE%"
    echo Data_BuildingBlocks["ladder_rope"] = { >> "%BLOCKS_FILE%"
    echo     name = "Rope Ladder", >> "%BLOCKS_FILE%"
    echo     category = "ladder", >> "%BLOCKS_FILE%"
    echo     material = "rope", >> "%BLOCKS_FILE%"
    echo     durability = 50, >> "%BLOCKS_FILE%"
    echo     cost = { rope = 3, wood = 1 }, >> "%BLOCKS_FILE%"
    echo     icon = "ladder_rope_icon", >> "%BLOCKS_FILE%"
    echo     model = "ladder_rope_model", >> "%BLOCKS_FILE%"
    echo     climbSpeed = 2.4, >> "%BLOCKS_FILE%"
    echo     maxHeight = 40, >> "%BLOCKS_FILE%"
    echo     canClimb = true, >> "%BLOCKS_FILE%"
    echo     buildable = true, >> "%BLOCKS_FILE%"
    echo     placeable = true >> "%BLOCKS_FILE%"
    echo } >> "%BLOCKS_FILE%"
    echo. >> "%BLOCKS_FILE%"
    
    echo ✅ Added all ladder blocks
) else (
    echo ❌ Data_BuildingBlocks.lua not found!
)

echo.
echo ========================================
echo  Ladder Injection Complete!
echo ========================================
echo.
echo ✅ Ladders have been injected into Castle Story!
echo.
echo You should now see a "Ladders" category in the Structures tab with:
echo - Wooden Ladder (2 wood)
echo - Iron Ladder (1 iron + 1 wood)  
echo - Stone Ladder (2 stone)
echo - Rope Ladder (3 rope + 1 wood)
echo.
echo Backup created at: %BACKUP_DIR%
echo.
echo Start Castle Story to see the ladders!
echo.

pause
ENDLOCAL
