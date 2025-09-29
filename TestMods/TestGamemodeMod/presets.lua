-- Test Presets File for Castle Story
-- This file tests the Lua Editor's preset editing capabilities

local presets = {
    -- Difficulty Presets
    difficulty = {
        easy = {
            name = "Easy",
            description = "Relaxed gameplay with more resources and weaker enemies",
            settings = {
                bricktronCap = 150,
                startingWorkersCount = 15,
                startingKnightCount = 3,
                startingArcherCount = 3,
                startingResources = 2000,
                maximumEnemyLevel = 5,
                playerAttackInterval = 900,
                fireflyCostMultiplier = 0.1
            }
        },
        normal = {
            name = "Normal",
            description = "Balanced gameplay with standard settings",
            settings = {
                bricktronCap = 100,
                startingWorkersCount = 10,
                startingKnightCount = 2,
                startingArcherCount = 2,
                startingResources = 1000,
                maximumEnemyLevel = 10,
                playerAttackInterval = 600,
                fireflyCostMultiplier = 0.2
            }
        },
        hard = {
            name = "Hard",
            description = "Challenging gameplay with limited resources and stronger enemies",
            settings = {
                bricktronCap = 75,
                startingWorkersCount = 8,
                startingKnightCount = 1,
                startingArcherCount = 1,
                startingResources = 500,
                maximumEnemyLevel = 15,
                playerAttackInterval = 300,
                fireflyCostMultiplier = 0.3
            }
        },
        expert = {
            name = "Expert",
            description = "Extreme difficulty for experienced players",
            settings = {
                bricktronCap = 50,
                startingWorkersCount = 5,
                startingKnightCount = 0,
                startingArcherCount = 0,
                startingResources = 250,
                maximumEnemyLevel = 20,
                playerAttackInterval = 150,
                fireflyCostMultiplier = 0.5
            }
        }
    },
    
    -- Game Mode Presets
    gameMode = {
        sandbox = {
            name = "Sandbox",
            description = "Creative mode with unlimited resources",
            settings = {
                bricktronCap = 999,
                startingWorkersCount = 50,
                startingKnightCount = 20,
                startingArcherCount = 20,
                startingResources = 99999,
                maximumEnemyLevel = 0,
                playerAttackInterval = 0,
                fireflyCostMultiplier = 0.0,
                canDigGround = true,
                playerRelations = 0
            }
        },
        survival = {
            name = "Survival",
            description = "Survive against waves of enemies",
            settings = {
                bricktronCap = 80,
                startingWorkersCount = 8,
                startingKnightCount = 2,
                startingArcherCount = 2,
                startingResources = 800,
                maximumEnemyLevel = 12,
                playerAttackInterval = 400,
                fireflyCostMultiplier = 0.25,
                canDigGround = true,
                playerRelations = 2
            }
        },
        pvp = {
            name = "Player vs Player",
            description = "Competitive multiplayer mode",
            settings = {
                bricktronCap = 120,
                startingWorkersCount = 12,
                startingKnightCount = 3,
                startingArcherCount = 3,
                startingResources = 1500,
                maximumEnemyLevel = 8,
                playerAttackInterval = 500,
                fireflyCostMultiplier = 0.15,
                canDigGround = false,
                playerRelations = 2
            }
        }
    },
    
    -- Resource Presets
    resources = {
        abundant = {
            name = "Abundant Resources",
            description = "More resources available",
            settings = {
                startingResources = 3000,
                fireflyCostMultiplier = 0.1,
                bricktronCap = 150
            }
        },
        scarce = {
            name = "Scarce Resources",
            description = "Limited resources for challenge",
            settings = {
                startingResources = 300,
                fireflyCostMultiplier = 0.4,
                bricktronCap = 60
            }
        },
        normal = {
            name = "Normal Resources",
            description = "Standard resource availability",
            settings = {
                startingResources = 1000,
                fireflyCostMultiplier = 0.2,
                bricktronCap = 100
            }
        }
    }
}

return presets
