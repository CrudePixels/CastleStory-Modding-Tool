-- Ladder System Configuration
-- This file configures the enhanced ladder system for Castle Story

LadderConfig = {
    -- Basic ladder settings
    enabled = true,
    maxHeight = 50,          -- Maximum ladder height in blocks
    climbSpeed = 2.0,        -- Climbing speed multiplier
    autoSnap = true,         -- Auto-snap to ladder when near
    
    -- Ladder types
    ladderTypes = {
        {
            name = "Wooden Ladder",
            material = "Wood",
            durability = 100,
            climbSpeed = 1.0,
            cost = 5
        },
        {
            name = "Iron Ladder", 
            material = "Iron",
            durability = 200,
            climbSpeed = 1.2,
            cost = 10
        },
        {
            name = "Stone Ladder",
            material = "Stone", 
            durability = 300,
            climbSpeed = 0.8,
            cost = 8
        },
        {
            name = "Rope Ladder",
            material = "Rope",
            durability = 50,
            climbSpeed = 1.5,
            cost = 3
        }
    },
    
    -- Animation settings
    animations = {
        climbUpStart = "LadderGoingUpStart",
        climbUpCycle = "LadderGoingUpCycle", 
        climbUpEnd = "LadderGoingUpEnd",
        climbDownStart = "LadderGoingDownStart",
        climbDownCycle = "LadderGoingDownCycle",
        climbDownEnd = "LadderGoingDownEnd"
    },
    
    -- Physics settings
    physics = {
        grabDistance = 2.0,      -- Distance to grab ladder
        releaseDistance = 3.0,   -- Distance to release ladder
        snapDistance = 1.0,      -- Distance to snap to ladder
        climbHeight = 1.0,       -- Height per climb step
        fallDamage = false       -- Take fall damage when falling from ladder
    },
    
    -- Building requirements
    requirements = {
        minLevel = 1,           -- Minimum player level to build ladders
        requiresBlueprint = false, -- Requires blueprint to build
        maxPerPlayer = 10,      -- Maximum ladders per player
        cooldown = 5.0          -- Cooldown between ladder placements (seconds)
    }
}

-- Function to get ladder type by name
function GetLadderType(name)
    for i, ladderType in ipairs(LadderConfig.ladderTypes) do
        if ladderType.name == name then
            return ladderType
        end
    end
    return nil
end

-- Function to check if ladder building is allowed
function CanBuildLadder(player)
    if not LadderConfig.enabled then
        return false, "Ladder system is disabled"
    end
    
    if player.level < LadderConfig.requirements.minLevel then
        return false, "Player level too low"
    end
    
    local ladderCount = GetPlayerLadderCount(player)
    if ladderCount >= LadderConfig.requirements.maxPerPlayer then
        return false, "Maximum ladder limit reached"
    end
    
    return true, "OK"
end

-- Function to get player's ladder count
function GetPlayerLadderCount(player)
    -- This would be implemented to count existing ladders
    return 0
end

-- Function to create a new ladder
function CreateLadder(position, ladderType, player)
    local canBuild, reason = CanBuildLadder(player)
    if not canBuild then
        return false, reason
    end
    
    local ladder = {
        position = position,
        type = ladderType,
        owner = player,
        durability = ladderType.durability,
        created = os.time()
    }
    
    -- Add ladder to world
    AddLadderToWorld(ladder)
    
    return true, "Ladder created successfully"
end

-- Function to add ladder to world (placeholder)
function AddLadderToWorld(ladder)
    -- This would be implemented to actually place the ladder in the game world
    print("Ladder added to world at position: " .. tostring(ladder.position))
end

-- Function to remove ladder
function RemoveLadder(ladder, player)
    if ladder.owner ~= player then
        return false, "Not your ladder"
    end
    
    -- Remove from world
    RemoveLadderFromWorld(ladder)
    
    return true, "Ladder removed"
end

-- Function to remove ladder from world (placeholder)
function RemoveLadderFromWorld(ladder)
    -- This would be implemented to actually remove the ladder from the game world
    print("Ladder removed from world at position: " .. tostring(ladder.position))
end

-- Function to start climbing
function StartClimbing(unit, ladder)
    if not LadderConfig.enabled then
        return false
    end
    
    -- Set climbing state
    unit.isClimbing = true
    unit.currentLadder = ladder
    unit.climbDirection = 0 -- 1 = up, -1 = down, 0 = stopped
    
    -- Start climbing animation
    PlayClimbingAnimation(unit, "start")
    
    return true
end

-- Function to stop climbing
function StopClimbing(unit)
    if unit.isClimbing then
        unit.isClimbing = false
        unit.currentLadder = nil
        unit.climbDirection = 0
        
        -- Stop climbing animation
        PlayClimbingAnimation(unit, "stop")
    end
end

-- Function to play climbing animation (placeholder)
function PlayClimbingAnimation(unit, state)
    -- This would be implemented to play the actual climbing animations
    print("Playing climbing animation for unit: " .. tostring(unit) .. " state: " .. state)
end

-- Function to update ladder system
function UpdateLadderSystem(deltaTime)
    if not LadderConfig.enabled then
        return
    end
    
    -- Update all climbing units
    local climbingUnits = GetClimbingUnits()
    for i, unit in ipairs(climbingUnits) do
        UpdateClimbingUnit(unit, deltaTime)
    end
    
    -- Update ladder durability
    local ladders = GetAllLadders()
    for i, ladder in ipairs(ladders) do
        UpdateLadderDurability(ladder, deltaTime)
    end
end

-- Function to get climbing units (placeholder)
function GetClimbingUnits()
    -- This would return all units currently climbing
    return {}
end

-- Function to get all ladders (placeholder)
function GetAllLadders()
    -- This would return all ladders in the world
    return {}
end

-- Function to update climbing unit (placeholder)
function UpdateClimbingUnit(unit, deltaTime)
    -- This would handle the climbing movement and animation
end

-- Function to update ladder durability (placeholder)
function UpdateLadderDurability(ladder, deltaTime)
    -- This would handle ladder wear and tear
end
