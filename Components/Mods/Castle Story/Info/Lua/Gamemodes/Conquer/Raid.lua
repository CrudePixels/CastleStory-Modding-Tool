
Raid = {}


function Raid.SpawnAt(crystal, presetLevel, presetRaid, faction, raid)
	for x, pair in pairs(RaidPresets[presetLevel][presetRaid]) do
		for y = 1, pair[1] do
			local newguy = Spawner.Around(pair[2].Ref, crystal.position, faction, pair[2].Occupation)
			newguy.OriginalSpawnCrystal = crystal
			if raid ~= nil then
				Project.AssignLabor(raid, newguy)
			end
		end
	end
end

function Raid.SpawnWithAggro(crystal, aggro, raid)
	local corFaction = Faction.FindByName("Corruptrons")
	local preset, plevel, praid, paggro = RaidPresets.GetUnderBudget(aggro)
  local spawnedAggro = 0
  
  while preset ~= nil do
    Raid.StackSpawn(crystal, plevel, praid, corFaction, raid)
    spawnedAggro = spawnedAggro + paggro
    preset, plevel, praid, paggro = RaidPresets.GetUnderBudget(aggro - spawnedAggro)
  end
end

SpawnStack = {}
function Raid.StackSpawn(crystal, presetLevel, presetRaid, faction, raid)
  local SpawnParams = {crystal, presetLevel, presetRaid, faction, raid}
  table.insert(SpawnStack, SpawnParams)
end

function Raid.ProcessSpawnStack()
  if (#SpawnStack > 0) then
    local SpawnParams = table.remove(SpawnStack)
    Raid.SpawnAt(SpawnParams[1], SpawnParams[2], SpawnParams[3], SpawnParams[4], SpawnParams[5])
  end
end



function Raid.SpawnDefenders(crystal)
  
  if IsNilOrReleased(crystal.AssignedRaid) then
    Raid.CreateRaid(crystal)
  end
  
  local defenseAggroToSpawn = (sv_Settings.baseCorruptronDefense + (Registry.currentLevel * sv_Settings.defenseIncreasePerLevel)) - crystal.AssignedRaid.CrewAggro
  local defenseRaid = crystal.AssignedRaid
  if crystal.crystalType == 0 then --If it is a home crystal we try to force a warlock spawn
    defenseAggroToSpawn = defenseAggroToSpawn + 18
    if defenseAggroToSpawn > 18 then
      local corFaction = Faction.FindByName("Corruptrons")
      Raid.SpawnWithAggro(crystal, defenseAggroToSpawn - 18, defenseRaid)
      Raid.SpawnAt(crystal, "Level1", "Raid7", corFaction, defenseRaid)
    else 
      if defenseAggroToSpawn > 0 then
        Raid.SpawnWithAggro(crystal, defenseAggroToSpawn, defenseRaid)
      end
    end
  else
    if defenseAggroToSpawn > 0 then
      Raid.SpawnWithAggro(crystal, defenseAggroToSpawn, defenseRaid)
    end
  end
end



function Raid.SendNeutralAttack()
  local neutralFaction = Faction.FindByName("Neutral")
  local neutralCrystals = fy_Crystal:GetAllOfFaction(neutralFaction)
  
  if #neutralCrystals > 0 then
    local corFaction = Faction.FindByName("Corruptrons")
    local corruptedCrystals = fy_Crystal:GetAllOfFaction(corFaction)
    
    local startingCrystal = nil
    for x, crystal in pairs(neutralCrystals) do --This is necessary to reassign corruptrons that did not manage to reach their target shard
      if not IsNilOrReleased(crystal.AssignedRaid) and crystal.AssignedRaid.CrewAggro > 0 then
        startingCrystal = crystal
        break
      end
    end
    
    if IsNilOrReleased(startingCrystal) then
      for x, crystal in pairs(corruptedCrystals) do 
        if not IsNilOrReleased(crystal.AssignedRaid) and crystal.AssignedRaid.CrewAggro > 0 then
          startingCrystal = crystal
          break
        end
      end
    end
    
    if IsNilOrReleased(startingCrystal) then
      startingCrystal = corruptedCrystals[math.random(#corruptedCrystals)]
    end
    
    local targetCrystal = nil
    if sv_Settings.randomCorruptronCapture then
      targetCrystal = neutralCrystals[math.random(#neutralCrystals)]
    else
      local closestDistance = 99999.9
      for x, crystal in pairs(neutralCrystals) do 
        local distance = Vector3.Distance(startingCrystal.position, crystal.position)
        if distance < closestDistance and crystal ~= startingCrystal then
          targetCrystal = crystal
          closestDistance = distance
        end
      end
    end
    
    if IsNilOrReleased(targetCrystal.AssignedRaid) then
      Raid.CreateRaid(targetCrystal)
    end
    
    if startingCrystal.AssignedRaid.CrewAggro > 0 then
      for x, corruptron in pairs(Project.GetCrew(startingCrystal.AssignedRaid)) do 
        Project.AssignLabor(targetCrystal.AssignedRaid, corruptron)
      end
    else
      Raid.SpawnWithAggro(startingCrystal, sv_Settings.baseCorruptronOffense, targetCrystal.AssignedRaid)
    end
  end
end



function Raid.SendPlayerAttack()
  local playerCrystals = {}
  for x, crystal in pairs(fy_Crystal:GetAll()) do
    if crystal.faction:HasOwner() then
      table.insert(playerCrystals, crystal)
    end
  end
  
  if #playerCrystals > 0 then
    local corFaction = Faction.FindByName("Corruptrons")
    local corruptedCrystals = fy_Crystal:GetAllOfFaction(corFaction)
    local randomPlayer = playerCrystals[math.random(#playerCrystals)]
    if IsNilOrReleased(randomPlayer.AssignedRaid) then
      Raid.CreateRaid(randomPlayer)
    end
    
    local squad = {}
    local squadAggro = 0
    local targetAggro = sv_Settings.baseCorruptronOffense + (Registry.currentLevel * sv_Settings.offenseIncreasePerLevel)
    for x, crystal in pairs(corruptedCrystals) do 
      if not IsNilOrReleased(crystal.AssignedRaid) and crystal.AssignedRaid.CrewAggro > 0 then
        for x, unit in pairs(Project.GetCrew(crystal.AssignedRaid)) do
          if unit.UnitAggro < targetAggro - squadAggro then
            Project.AssignLabor(randomPlayer.AssignedRaid, unit)
            squadAggro = squadAggro + unit.UnitAggro
          end
          if targetAggro - squadAggro < 3 then break end -- hardcoded corruptron aggro, do that better
        end
        if targetAggro - squadAggro < 3 then break end -- hardcoded corruptron aggro
      end
    end
    
    if targetAggro - squadAggro > 3 then -- hardcoded corruptron aggro
      local randomCorrupted = corruptedCrystals[math.random(#corruptedCrystals)]
      Raid.SpawnWithAggro(randomCorrupted, targetAggro - squadAggro, randomPlayer.AssignedRaid)
    end
  end
end



function Raid.CreateRaid(crystal)
	local corFaction = Faction.FindByName("Corruptrons")
	local raid = fy_RaidProject:New(corFaction, Vector3.zero)
  raid:SetBeaconPosition(crystal.position)
	crystal.AssignedRaid = raid
	raid.assignedCrystal = crystal
  raid.AutoDeleteDisabled = true
	return raid
end


