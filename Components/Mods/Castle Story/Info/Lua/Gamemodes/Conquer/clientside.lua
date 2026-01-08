ConnectLifecycle()
IsMusicPlaying = false

dofile "config.lua"
dofile "ui.lua"


--from is an object, key is a static key, ie: fy_Crystal
local function FindClosestFromKey(from, key)

  local allKeys = key:GetAll()
  local closestDistance = 9999.0
  local closestKey = nil

  for x, mine in pairs(allKeys) do
    local dist = Vector3.Distance(from.position, mine.position)
    if dist < closestDistance then
      closestDistance = dist
      closestKey = mine
    end
  end

  return closestKey

end

local _hostReady = false
local _levelReady = false
function TryReady()
	if not _hostReady then return end
	if not _levelReady then return end
	LoadAllMenus()
end

function OnServerMessage(table)
	if table[1] == 0 then --BuildPhase timer
		Registry.timers.timeBeforeCanCapture = Timer.New(100)
		Registry.timers.timeBeforeCanCapture = table[2]
	elseif table[1] == 1 then --Host is ready
		OnGameModeReady()
	elseif table[1] == 2 then --Game mode was deserialized
		OnGameModeReady()
		GameClock.age = table[2]
	elseif table[1] == 3 then --GameTime
		Registry.gameTimer = Timer.New(0)
		if IsMultiplayer() then
			Registry.gameTimer:Start()
		end
		Registry.gameTimerOffset = table[2]
		_hostReady = true
		TryReady()
	end
end
Hooks.Connect(hk_OnServerMessage, OnServerMessage)


function OnGameModeReady()
	Hooks.Invoke(hk_OnGameModeTimersStarted)
end

function OnNewLevelReady()

	local crystals = fy_Crystal:GetAll()
	if #crystals == 0 then
		error("No spawnpoints in map")
	else
		for x, crystal in pairs( crystals) do
			if fy_User.localUser.alliedFaction == crystal.faction then
				MainCamera:SetRotation(54.0, 45.0, 0.0)
				MainCamera:LookAt(crystal.position)
			end
		end
	end
  
	Skybox.dayTime = sv_Settings.startingTimeOfDay	
	ShowIntroDialog()

end
Hooks.Connect(hk_OnNewLevelReady, OnNewLevelReady)

function OnCrystalFactionChanged(self, old, new)
	-- 1 arg == OnExplosion<LuaDriver>, 2 arg == OnFactionChanged<LuaDriver, old Faction>

	if fy_User.localUser.alliedFaction ~= Faction.spectator then
    TriggerMusicEvent()
    local clientFaction = fy_User.localUser.alliedFaction
    local enemyHomesRemaining = 0
    local myHomesRemaining = 0

    for x, crystal in pairs(fy_Crystal:GetAll()) do
      local currentFaction = crystal.faction
      
      if crystal.isFireflyHome or currentFaction.isAI then        
        if currentFaction == clientFaction then
          myHomesRemaining = myHomesRemaining + 1

        elseif currentFaction:IsEnemyTo(clientFaction) then 
          enemyHomesRemaining = enemyHomesRemaining + 1
        end
      end

    end

    -- For now, play the same event when an enemy cristal is neutralized and when the player captures one for themselves
    if old:IsEnemyTo(clientFaction) or new == clientFaction then
      if self.isFireflyHome then
        --print("Big_Crystal_Captured_Positive")
      	Sound.Play("Big_Crystal_Captured_Positive")
      else
        --print("Small_Crystal_Captured_Positive")
        Sound.Play("Small_Crystal_Captured_Positive")
      end
    end

    if myHomesRemaining == 0 then
      Picking.DeselectAll()
      Rules:SetAllowedTasks( {} )
      fy_User.localUser:JoinSpectators()
      Hooks.Invoke(hk_OnDefeat, {})
      Sound.Play("Mus_Conquest_Defeat")
      
      if IsMultiplayer() then
		ShowDefeatDialog(true)
      else
		ShowDefeatDialog()
      end
    elseif enemyHomesRemaining == 0 then
      Picking.DeselectAll()
      Faction.AllVisible(true)
      Rules:SetAllowedTasks( {} )
      Hooks.Invoke(hk_OnVictory, {})
      Sound.Play("Mus_Conquest_Victory")
	  ShowVictoryDialog()
    end
	end
end

clientGameOverDelay = Timer.New(2.0)
clientBoomedFactions = {} --I miss Neo.Relais
function OnCrystalBoom(faction) --Only home crystals are hooked
  --print("Boom signal clientside received")
  table.insert(clientBoomedFactions, faction)
  clientGameOverDelay:Reset()
  clientGameOverDelay:Start()
end

function DoCrystalBoomCheck(faction) 
  --print("Checking exploded crystal")
  --print("Belonged to " .. faction.FactionName)
  --print("Local user is " .. fy_User.localUser.alliedFaction.FactionName)
  if faction == fy_User.localUser.alliedFaction or fy_User.localUser.alliedFaction == Faction.spectator then 
    --print("Trying to lose")
    Picking.DeselectAll()
    Rules:SetAllowedTasks( {} )
    fy_User.localUser:JoinSpectators()
    Hooks.Invoke(hk_OnDefeat, {})
    Sound.Play("Mus_Conquest_Defeat")
    
    if IsMultiplayer() then
		ShowDefeatDialog(true)
    else
		ShowDefeatDialog()
    end
  else
    --print("checking if ennemies remain")
    local enemyRemaining = false;
    for x, crystal in pairs(fy_Crystal:GetAll()) do
      --print("checking a crystal of " .. crystal.faction.FactionName)
      if (crystal.isFireflyHome or crystal.faction.isAI) and crystal.faction:IsEnemyTo(fy_User.localUser.alliedFaction) then
        --print("Is enemy home or AI")
        enemyRemaining = true;
      end
    end
    
    if enemyRemaining == false then
      --print("Trying to win")
      Picking.DeselectAll()
      Faction.AllVisible(true)
      Rules:SetAllowedTasks( {} )
      Hooks.Invoke(hk_OnVictory, {})
      Sound.Play("Mus_Conquest_Victory")
	  ShowVictoryDialog()
    end
  end
end

function SlowUpdate()
  if (clientGameOverDelay.running and clientGameOverDelay:Check()) then
    clientGameOverDelay:Stop()
    for x, faction in pairs(clientBoomedFactions) do
      if faction ~= nil then
        DoCrystalBoomCheck(faction)
      end
    end
    clientBoomedFactions = {}
  end
end

function OnLevelReady()
	Sound.Play("Mus_Conquest_Beginning")
	SetupMusicCooldown()
	if sv_Settings.canDigGround == false then
		Rules:SetDigRules( {Diggables.Crust, Diggables.Artificial})
		Rules:SetExplosionRules( {Diggables.Crust, Diggables.Artificial})
		Rules:SetAllowedTasks({TaskType.All})
		Mergeback.allowMergeback = false
	end

	Rules.canSpawnMilitaryUnits = true
	Rules.playerRelations = sv_Settings.playerRelations
	Rules.proportionalRefund = 0.7
  Rules.bricktronCap = sv_Settings.bricktronCap

	Rules.forcePlayerFirefliesToPlayerCrystal = sv_Settings.forcePlayerFirefliesToPlayerCrystal

	local crystals = fy_Crystal:GetAll()
	if #crystals == 0 then
		error("No spawnpoints in map")
	else
		for x, crystal in pairs( crystals) do
			--Pure clients don't have serialized camera positions.
			if not IsServer() and fy_User.localUser.alliedFaction == crystal.faction then
				MainCamera:SetRotation(54.0, 45.0, 0.0)
				MainCamera:LookAt(crystal.position)
			end
			--print("clientside:156 " .. tostring(crystal.hk_OnSetFaction))
			Hooks.Connect(crystal.hk_OnSetFaction, function(old,new) return OnCrystalFactionChanged(crystal, old, new) end)
      Hooks.Connect(crystal.hk_OnExplosionFaction, OnCrystalBoom)
      --print("client boom signal connected")
		end
	end
  
  Skybox.dayLengthFactor = sv_Settings.daytimeFactor
	Skybox.nightLengthFactor = sv_Settings.nighttimeFactor
	Skybox.dayPause = sv_Settings.pauseTimeOfDay
  	if (sv_Settings.sunElevationMin ~= nil) then
		Skybox.Sun.SunElevationLimits = Vector2.New(sv_Settings.sunElevationMin, sv_Settings.sunElevationMax)
	end
	
	if sv_Settings.daynightCycleSetting == 1 then
		Skybox.nightLengthFactor = 0.001
	elseif sv_Settings.daynightCycleSetting == 2 then
		Skybox.dayLengthFactor = 0.001
	end
	
	_levelReady = true
	TryReady()
	
end
Hooks.Connect(hk_OnLevelReady, OnLevelReady)

function _OnEnd()
  newMenu.Close()
end
Hooks.Connect(hk_OnEnd, _OnEnd)


local MusicCooldown = Timer.New(0)
local MusicCooldownLimit = Timer.New(360)
local MusicWinningRatio = 0
local MusicRandomChoice = -1
local MusicRandomPrevChoice = -1

function TriggerMusicEvent(dontStop)
	if IsMusicPlaying and dontStop then
		-- Ignore
	elseif MusicCooldown:Check() == true then
		if IsMusicPlaying == false then
      StartConquestMusic()
		else
      StopConquestMusic()
		end
		ResetMusicCooldown()
	end
end

function ForceEnableMusicEvent()
  StartConquestMusic()
	ResetMusicCooldown()
end

function SetupMusicCooldown()
	MusicCooldown:Set(180)
	MusicCooldown:Start()
	MusicCooldownLimit:Set(360)
	MusicCooldownLimit:Start()
end

function ResetMusicCooldown()
  MusicCooldown:Reset()
  MusicCooldownLimit:Reset()
end


function UpdateMusicWinningRatio()
    local clientFaction = fy_User.localUser.alliedFaction
    local alliedCrystals = 0;
    local enemyCrystals = 0;
    local totalCrystals = 0;

    for x, crystal in pairs(fy_Crystal:GetAll()) do
      local currentFaction = crystal.faction

	  if currentFaction:IsAlliedTo(clientFaction) then
        alliedCrystals = alliedCrystals + 1
      elseif currentFaction:IsEnemyTo(clientFaction) then 
        enemyCrystals = enemyCrystals + 1
      end
      totalCrystals = totalCrystals + 1
    end

    MusicWinningRatio = (alliedCrystals - enemyCrystals) / totalCrystals * 0.5 + 0.5
end

function StartConquestMusic()
  IsMusicPlaying = true
  UpdateMusicWinningRatio()

  local r = -1

  while true do
    if MusicWinningRatio < 0.33 then
      r = math.random(4)
    elseif MusicWinningRatio < 0.67 then
      r = math.random(8)
    else
      r = math.random(4) + 4
    end

    if r ~= MusicRandomChoice and r ~= MusicRandomPrevChoice then
      break
    end
  end

  MusicRandomPrevChoice = MusicRandomChoice
  MusicRandomChoice = r

  if r == 1 then
    Sound.Play("Mus_Conquest_Borden")
  elseif r == 2 then
    Sound.Play("Mus_Conquest_CoL")
  elseif r == 3 then
    Sound.Play("Mus_Conquest_OoTM")
  elseif r == 4 then
    Sound.Play("Mus_Conquest_Unwanted")
  elseif r == 5 then
    Sound.Play("Mus_Conquest_DeepConquest")
  elseif r == 6 then
    Sound.Play("Mus_Conquest_GreatestCastle")
  elseif r == 7 then
    Sound.Play("Mus_Conquest_RarelySeen")
  else
    Sound.Play("Mus_Conquest_UpsideDown")
  end

end

function StopConquestMusic()
  IsMusicPlaying = false
  Sound.Play("Mus_Conquest_Stop")
end

function SlowUpdateMusic()
	if MusicCooldownLimit:Check() == true then
		ForceEnableMusicEvent()
	end
end
Hooks.Connect(hk_SlowUpdate, SlowUpdateMusic)
Hooks.Connect(hk_SlowUpdate, SlowUpdate)
