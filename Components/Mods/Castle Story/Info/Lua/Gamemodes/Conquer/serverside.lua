
dofile "Spawner.lua"
dofile "Raid.lua"
dofile "RaidPresets.lua"

--from is an object, key is a static key, ie: fy_Crystal
local function FindClosestFromKey(from, key)
  
  return FindClosestFromTable(from, key:GetAll(), nil)
  
end

--TODO: put this where it belongs
fy_Firefly.timeToAbsorption = 60.0

function SetAsCaptureCrystal(crystal)
  crystal:SetAsCaptureCrystal()
  crystal.isFireflyHome = false;
  crystal.doCrystalBrew = true;
  crystal.crystalBrewInterval = 5
  crystal.crystalFillingPerFirefly = 20
  crystal:ReleaseAllFireflies()
end

function SetAsHomeCrystal(crystal)
  crystal:SetAsSpawnCrystal()
  crystal.isFireflyHome = true;
  crystal.doCrystalBrew = true;
  crystal.crystalBrewInterval = 5
  crystal.crystalFillingPerFirefly = 20.0
end

function OnNewLevelLoaded()
  local function NewCrystal(faction, position, forPlayer)
    local newCrystal = fy_Crystal:New(faction, position)
    if (forPlayer == false) then
      newCrystal.isInvincible = true
    end
    Raid.CreateRaid(newCrystal)
    return newCrystal
  end
  
  local spawnPoints = Markers.GetGroup("Player Starting Point")
	local factionsToSpawn = {}
  
	for x, faction in pairs(Faction:GetAll()) do
		if faction:HasOwner() then table.insert(factionsToSpawn, faction) end
	end
  
	for i = 1, sv_Settings.startingCorruptCrystals do
		table.insert(factionsToSpawn, Faction.defaultEnemy)
	end
	
	Rules.playerRelations = sv_Settings.playerRelations	
  
  if #spawnPoints == 0 then
    error("No spawnpoints in map")
  else
    DetermineCrystals(factionsToSpawn)
    for x, spawn in pairs(spawnPoints) do
      local spawnCrystal = NewCrystal(spawn.desiredFaction, spawn.position, spawn.desiredFaction:HasOwner())
      SetAsHomeCrystal(spawnCrystal)
	  spawn.desiredFaction.GatherPoint = Random.VoxelAround(spawn.position)
      
      --Hooks.ConnectOnce(spawnCrystal.hk_OnSetFaction, |old, new| SetAsCaptureCrystal(spawnCrystal))
      --Hooks.Connect(spawnCrystal.hk_OnSetFaction, OnServersideCrystalFactionChanged)
      
      if spawn.desiredFaction:HasOwner() then
        for i = 1, sv_Settings.startingWorkersCount do
          Spawner.Around(fy_Bricktron, spawn.position, spawn.desiredFaction, nil)
        end
        for i = 1, sv_Settings.startingKnightCount do
          Spawner.Around(fy_Bricktron, spawn.position, spawn.desiredFaction, Occupations.Knight)
        end
        for i = 1, sv_Settings.startingArcherCount do
          Spawner.Around(fy_Bricktron, spawn.position, spawn.desiredFaction, Occupations.Archer)
        end
      end
    end
    
    -- Spawn neutrals
    spawnPoints = Markers.GetGroup("Crystal")
    while #spawnPoints > 0 do
      local spawn = table.remove(spawnPoints, math.random(#spawnPoints))
      local crystal = NewCrystal(Faction.defaultNeutral, spawn.position, false)
      SetAsCaptureCrystal(crystal)
      
    end
  end
  
  --if not IsMultiplayer() then
    Registry.timers.timeForPlayerAttack = Timer.New(sv_Settings.playerAttackInterval + sv_Settings.firstWaveDurationBonus)
    Registry.timers.timeForNeutralAttack = Timer.New(sv_Settings.neutralAttackInterval)
    Registry.timers.timeBetweenLevels = Timer.New(sv_Settings.levelClockInterval)
    Registry.currentLevel = sv_Settings.initialEnemyLevel
  --end

  if sv_Settings.timeBeforeCanCapture ~= nil and sv_Settings.timeBeforeCanCapture > 0 then
    Registry.timers.timeBeforeCanCapture = Timer.New(sv_Settings.timeBeforeCanCapture)
    Rules.canCaptureCapturables = false
    Registry.timers.timeBeforeCanCapture:Start()
  else
    Registry.timers.timeBeforeCanCapture = Timer.New(0)
    Rules.canCaptureCapturables = true
  end
  
  if IsMultiplayer() then
	NetworkSendTimeBeforeCanCaptureTimer()
  end

	if IsMultiplayer() then
		StartTimers()
	end
  
  SetSaveData("conquest", "doneInit", true)
end
Hooks.Connect(hk_OnNewLevelLoaded, OnNewLevelLoaded)

function OnLevelLoaded()
  sv_Settings = GetSaveData("conquest", "sv_Settings") or sv_Settings
  Registry = GetSaveData("conquest", "Registry") or Registry
	NetworkSendTimeBeforeCanCaptureTimer()
  Hooks.Connect(fy_Crystal.hk_SpawnDefenders, Raid.SpawnDefenders)
end
Hooks.Connect(hk_OnLevelLoaded, OnLevelLoaded)

function OnLevelReady()
  
    for x, crystal in pairs(fy_Crystal:GetAll()) do
      if crystal.isFireflyHome then
        Hooks.ConnectOnce(crystal.hk_OnSetFaction, function(old, new) SetAsCaptureCrystal(crystal) end)
        Hooks.Connect(crystal.hk_OnSetFaction, OnServersideCrystalFactionChanged)
        Hooks.Connect(crystal.hk_OnExplosionFaction, OnServersideCrystalBoom)
      end
    end

	--GameTime
	do
	local t = {}
	t[1] = 3
	t[2] = GameClock.timeSinceGameStart
	Hooks.Invoke(hk_OnServerMessage, {t})
	end
end
Hooks.Connect(hk_OnLevelReady, OnLevelReady)

function FindClosestFromTable(from, table, predicate)

	predicate = predicate or function(x, y) return true end
  local closestDistance = 9999.0
  local closestKey = nil

  for key, value in pairs(table) do
    local dist = Vector3.Distance(from.position, value.position)
    
    if dist < closestDistance and predicate(from, value) then
      closestDistance = dist
      closestKey = value
    end
  end
  
  return closestKey
end

function FindFurthestFromTable(from, table, predicate)
	predicate = predicate or function(x,y) return true end
  local closestDistance = 0.0
  local closestKey = nil
  for key, value in pairs(table) do
    local dist = Vector3.Distance(from.position, value.position)
    if dist > closestDistance and predicate(from, value) then
      closestDistance = dist
      closestKey = value
    end
  end
  return closestKey

end

--from is an object, key is a static key, ie: fy_Crystal
function FindFurthestFromKey(from, key)
  return FindFurthestFromTable(from, key:GetAll(), nil)
end

function SetFieldOfAll(table, fieldIndex, value)

	for x, element in pairs(table) do 
		element[fieldIndex] = value
	end

end

function DetermineCrystals(factionsToAssign)
	
	dc_spawnPoints = Markers.GetGroup("Player Starting Point")

	--SetFieldOfAll(dc_spawnPoints, "faction", Faction.defaultNeutral)
	for x, element in pairs(dc_spawnPoints) do 
		element.desiredFaction = Faction.defaultNeutral
	end
	for x, element in pairs(dc_spawnPoints) do 
		element.desiredValue = 0
	end
	--SetFieldOfAll(dc_spawnPoints, "desiredValue", 0)

	-- local stringoutput1 = ""
	-- for x, faction in pairs(factionsToAssign) do
	-- 	stringoutput1 = stringoutput1.."--"..x .. " " .. tostring(faction) .. "\n"
	-- end
	--print("-Going to assign\n"..stringoutput1)

	local factionsAssigned = 0

	for fi = 1, #factionsToAssign, 1 do
		
		local factionToAssign = factionsToAssign[fi]
    
		if factionsAssigned == 0 then
				
			dc_spawnPoints[ math.random(#dc_spawnPoints)].desiredFaction = factionToAssign
			factionsAssigned = factionsAssigned + 1
      
		else
      
			local infLoopPrev = 0
			local desisionMade = false
			while not desisionMade do
        
				--Go through every point
				for x, spawn1 in pairs(dc_spawnPoints) do 
          
					--If the base point is not neutral
					if spawn1.desiredFaction ~= Faction.defaultNeutral then
            
						local chosenSpawn = nil
            
						local predicate = function(spawn1, spawn2)
							return not spawn1.desiredFaction:IsEnemyTo(spawn2.desiredFaction) and spawn1.desiredFaction ~= spawn2.desiredFaction and spawn2.desiredFaction == Faction.defaultNeutral
						end
            
						if spawn1.desiredFaction:IsAlliedTo(factionToAssign) then
							chosenSpawn = FindClosestFromTable(spawn1, dc_spawnPoints, predicate)
						else
							chosenSpawn = FindFurthestFromTable(spawn1, dc_spawnPoints, predicate)
						end
						
            if chosenSpawn == nil then
              
              desisionMade = true
                break
              
            else
              
              chosenSpawn.desiredValue = chosenSpawn.desiredValue + 1
              --print("-- "..tostring(spawn1.desiredFaction).." is considering "..tostring(chosenSpawn).. " at "..chosenSpawn.desiredValue.."/"..factionsAssigned)
              
              if chosenSpawn.desiredValue == factionsAssigned then
                --print("--- Chose "..tostring(chosenSpawn) .. " for " .. tostring(factionToAssign))
                chosenSpawn.desiredFaction = factionToAssign
                factionsAssigned = factionsAssigned + 1
                desisionMade = true
                break
              end
            end
					end -- end of if neutral 
				end --end of for loop
        
				infLoopPrev = infLoopPrev + 1
				if infLoopPrev >= 128 then
					break
				end
			end --end of while loop
		end -- end of else factionAssigned > 0
	end -- end of for each factions

	--SetFieldOfAll(dc_spawnPoints, "desiredValue", 0)
	for x, element in pairs(dc_spawnPoints) do 
		element.desiredValue = desiredValue
	end

	
	-- local stringoutput = ""
	-- for x, spawn in pairs(dc_spawnPoints) do

	-- 	stringoutput = stringoutput.."--"..x .. " " .. tostring(spawn) .. tostring(spawn.desiredFaction) .. "\n"
	-- end
	--print("-Done!\n"..stringoutput)

end



function StartTimers()
	if not IsMultiplayer() then
		Registry.gameTimer:Start()
	end
  if sv_Settings.hasRaidMissions == true then
    Registry.timers.timeForPlayerAttack:Start()
    Registry.timers.timeForNeutralAttack:Start()
    Registry.timers.timeBetweenLevels:Start()
  end
  NetworkSendGameModeReady()
end

function NetworkSendTimeBeforeCanCaptureTimer()
    Table = {}
	Table[1] = 0
	Table[2] = Registry.timers.timeBeforeCanCapture
	Hooks.Invoke(hk_OnServerMessage,{Table})
end
function NetworkSendGameModeReady()
    Table = {}
	Table[1] = 1
	Hooks.Invoke(hk_OnServerMessage,{Table})
end
function NetworkSendDeserializedData()
    Table = {}
	Table[1] = 2
	Table[2] = GameClock.age
	Hooks.Invoke(hk_OnServerMessage,{Table})
end

function SlowUpdate()
  --if not IsMultiplayer() then
    
    if Registry.timers.timeForNeutralAttack:Check() then
      Raid.SendNeutralAttack()
      Registry.timers.timeForNeutralAttack:Reset()
    end
  
    if Registry.timers.timeForPlayerAttack:Check() then
      Raid.SendPlayerAttack()
      Registry.timers.timeForPlayerAttack:Set(sv_Settings.playerAttackInterval)
      Registry.timers.timeForPlayerAttack:Reset()
    end

    if Registry.timers.timeBetweenLevels:Check() then
      if Registry.currentLevel < sv_Settings.maximumEnemyLevel then
        Registry.currentLevel = Registry.currentLevel + 1
        print("Raids are now level " .. Registry.currentLevel)
      end
      Registry.timers.timeBetweenLevels:Reset()
    end
	
    if Registry.timers.timeBeforeCanCapture.running and Registry.timers.timeBeforeCanCapture:Check() then
      Rules.canCaptureCapturables = true
      Registry.timers.timeBeforeCanCapture:Stop()
      NetworkSendTimeBeforeCanCaptureTimer()
    end
    
    if (serverGameOverDelay.running and serverGameOverDelay:Check()) then
      serverGameOverDelay:Stop()
      for x, faction in pairs(serverBoomedFactions) do
        if faction ~= nil then
          DoServersideCrystalBoomCheck(faction)
        end
      end
      serverBoomedFactions = {}
    end
    
    Raid.ProcessSpawnStack()
  --end
end
Hooks.Connect(hk_SlowUpdate, SlowUpdate)

function OnSerializing()
  SetSaveData("conquest", "Registry", Registry)
  SetSaveData("conquest", "sv_Settings", sv_Settings)
end
Hooks.Connect(hk_OnSerializing, OnSerializing)

function OnServersideCrystalFactionChanged(old, new)
	-- 1 arg == OnExplosion<LuaDriver>, 2 arg == OnFactionChanged<LuaDriver, old Faction>

  if old ~= nil and not old.isAI then
    local oldFactionhasAHome = false
    for x, crystal in pairs(fy_Crystal:GetAllOfFaction(old)) do
      if crystal.isFireflyHome then
        oldFactionhasAHome = true
      end
    end
    
    if not oldFactionhasAHome then
      for x, firefly in pairs(fy_Firefly:GetAllOfFaction(old)) do
        if firefly.isNamed then
          firefly:Kill()
        end
      end
      
      local neutralFaction = Faction.FindByName("Neutral")
      for x, crystal in pairs(fy_Crystal:GetAllOfFaction(old)) do
        crystal.faction = neutralFaction
      end
      
      for x, bricktron in pairs(fy_Bricktron:GetAllOfFaction(old)) do
        bricktron:Kill()
      end
      
      old.hasGlobalFogVision = true
    end
  end
end

serverGameOverDelay = Timer.New(2.0)
serverBoomedFactions = {} --I miss Neo.Relais
function OnServersideCrystalBoom(faction) --Only home crystals are hooked
  table.insert(serverBoomedFactions, faction)
  serverGameOverDelay:Reset()
  serverGameOverDelay:Start()
end

function DoServersideCrystalBoomCheck(faction)
  --print("Boom signal serverside received")
  if faction ~= nil and not faction.isAI then
    local oldFactionhasAHome = false
    --print("Looking for crystals of " .. faction.FactionName)
    for x, crystal in pairs(fy_Crystal:GetAllOfFaction(faction)) do
      if crystal.isFireflyHome then
        --print("Found home crystal")
        oldFactionhasAHome = true
      end
    end
    
    if not oldFactionhasAHome then
      --print("Cleanup defeated player")
      for x, firefly in pairs(fy_Firefly:GetAllOfFaction(faction)) do
        if firefly.isNamed then
          firefly:Kill()
        end
      end
      
      local neutralFaction = Faction.FindByName("Neutral")
      for x, crystal in pairs(fy_Crystal:GetAllOfFaction(faction)) do
        crystal.faction = neutralFaction
      end
      
      for x, bricktron in pairs(fy_Bricktron:GetAllOfFaction(faction)) do
        bricktron:Kill()
      end
      
      faction.hasGlobalFogVision = true
    end
  end
end


