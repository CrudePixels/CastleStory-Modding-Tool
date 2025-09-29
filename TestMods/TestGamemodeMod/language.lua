-- Test Language File for Castle Story
-- This file tests the Lua Editor's language editing capabilities

local language = {
    -- UI Text
    ui = {
        mainMenu = "Main Menu",
        settings = "Settings",
        multiplayer = "Multiplayer",
        singleplayer = "Singleplayer",
        quit = "Quit",
        back = "Back",
        next = "Next",
        cancel = "Cancel",
        confirm = "Confirm",
        save = "Save",
        load = "Load",
        new = "New",
        delete = "Delete"
    },
    
    -- Game Messages
    messages = {
        welcome = "Welcome to Castle Story!",
        gameStart = "Game Starting...",
        gameEnd = "Game Ended",
        playerJoined = "Player joined the game",
        playerLeft = "Player left the game",
        serverFull = "Server is full",
        connectionLost = "Connection lost",
        reconnecting = "Reconnecting...",
        reconnected = "Reconnected successfully"
    },
    
    -- Tooltips
    tooltips = {
        build = "Build structures to expand your castle",
        gather = "Gather resources to build and survive",
        defend = "Defend your castle from enemies",
        research = "Research new technologies",
        trade = "Trade with other players",
        diplomacy = "Manage diplomatic relations"
    },
    
    -- Error Messages
    errors = {
        invalidInput = "Invalid input",
        notEnoughResources = "Not enough resources",
        cannotBuildHere = "Cannot build here",
        structureDestroyed = "Structure destroyed",
        unitKilled = "Unit killed",
        researchFailed = "Research failed"
    },
    
    -- Settings
    settings = {
        graphics = "Graphics",
        audio = "Audio",
        controls = "Controls",
        gameplay = "Gameplay",
        multiplayer = "Multiplayer",
        language = "Language",
        resolution = "Resolution",
        fullscreen = "Fullscreen",
        vsync = "Vertical Sync",
        antialiasing = "Antialiasing",
        shadows = "Shadows",
        textureQuality = "Texture Quality",
        masterVolume = "Master Volume",
        musicVolume = "Music Volume",
        sfxVolume = "Sound Effects Volume",
        voiceVolume = "Voice Volume"
    }
}

return language
