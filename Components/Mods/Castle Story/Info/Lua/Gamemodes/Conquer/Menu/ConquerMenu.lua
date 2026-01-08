require("LUI2.lua")


---Menu---
local _menu =
	Menu:New()

_minimapRoot = _menu
----------------------





----------------------
local LiveValue = dofile("LiveValue.lua")
local Trigger = dofile("Trigger.lua")


----begin---

_menu.ready = false
_menu.OnSlowUpdate=
	function()
		if(_menu.ready == false)then return end
		if(_menu.data == nil)then return end

		_menu.data.tr_onSlowUpdate:RunCallbacks()
		_menu.data.ev_updateCrystals:InvokeCollapsed()
	end

_menu.Begin =
	function()
		
	_menu.ready = true

	---data
	local _data = {}
	_menu.data = _data

	---on ready
	do
	_data.tr_onBegin = Trigger.New()
	_data.tr_onBegin2 = Trigger.New()
	_data.tr_onEnd = Trigger.New()

	_data.tr_onOpen = Trigger.New()
	_data.tr_onClose = Trigger.New()

	_data.tr_onSlowUpdate = Trigger.New()

	_data.tr_onUpdateCrystals = Trigger.New()

	_data.ev_updateCrystals = Event.New()
	_data.ev_updateCrystals:AddListener(||_data.tr_onUpdateCrystals:RunCallbacks())

	--_data.tr_onBegin:AddCallback(||print("tr_onBegin"))
	--_data.tr_onEnd:AddCallback(||print("tr_onEnd"))
	end
	
	---gameclock
	do
	_data.lv_elapsedTime = LiveValue.New("hh:mm:ss")
	--_data.lv_elapsedTime:AssignGetter(||GameClock.formatedTimeSinceGameStart)
	_data.lv_elapsedTime:AssignGetter(||Registry.gameTimer:FormatedElapsedSeconds(Registry.gameTimerOffset))
	_data.lv_elapsedTime:AddTrigger(_data.tr_onBegin)
	_data.lv_elapsedTime:AddTrigger(_data.tr_onSlowUpdate)
	end
	
	---hasCorrupteds
	do
	_data.lv_hasCorrupteds = LiveValue.New(false)
	_data.lv_hasCorrupteds:AssignGetter(||fy_Crystal:CountOfFaction(Faction.defaultEnemy) ~= 0)
	_data.lv_hasCorrupteds:AddTrigger(_data.tr_onBegin)
	end

	---users
	do
	_data.localUser = nil

	_data.userMap = {}

	_data.factionList = {}
	_data.factionMap = {}

	local _OnGetUsers =
		function(users, previousUsers)

			_data.localUser = fy_User.localUser
			_data.alliedFaction = _data.localUser.alliedFaction

			_data.userMap = {}

			_data.factionMap = {}
			_data.factionList = {}

			for i = 1, #users do

				local _user = users[i]
				_data.userMap[_user] = {}
				_data.userMap[_user].name = _user.name
				_data.userMap[_user].faction = _user.alliedFaction

				if not IsMultiplayer() then _data.userMap[_user].name = GetLocalized("##gamemenu_player") end
				
				local _isLocalUser = (_user == _data.localUser)

				local _faction = _data.userMap[_user].faction
				local _factionIsNew = (_data.factionMap[_faction] == nil)

				if(_factionIsNew)then

					_data.factionMap[_faction] = {}
					_data.factionMap[_faction].users = {}
					_data.factionMap[_faction].capturables = {}
					_data.factionMap[_faction].crystals = {}
					_data.factionMap[_faction].shards = {}
					_data.factionMap[_faction].color = _faction.factionColor
					
					if(_isLocalUser)then
						table.insert(_data.factionList, 1, _faction)
					else
						table.insert(_data.factionList, _faction)
					end
				end

				if(_isLocalUser)then
					table.insert(_data.factionMap[_faction].users, 1, _user)
				else
					table.insert(_data.factionMap[_faction].users, _user)
				end

			end
			
			do
				local _faction = Faction.defaultNeutral
				_data.neutralFaction = _faction

				if(_data.factionMap[_faction] == nil)then
					_data.factionMap[_faction] = {}
					_data.factionMap[_faction].users = {}
					_data.factionMap[_faction].capturables = {}
					_data.factionMap[_faction].crystals = {}
					_data.factionMap[_faction].shards = {}
					_data.factionMap[_faction].color = _faction.factionColor

					table.insert(_data.factionList, 1, _faction)
				end
			end
			
			--[[
			do
				local _faction = Faction.defaultEnemy
				_data.corruptedFaction = _faction

				local _hasCorrupteds = _data.lv_hasCorrupteds:GetValue()
				if(_hasCorrupteds and _data.factionMap[_faction] == nil)then
					_data.factionMap[_faction] = {}
					_data.factionMap[_faction].users = {}
					_data.factionMap[_faction].capturables = {}
					_data.factionMap[_faction].crystals = {}
					_data.factionMap[_faction].shards = {}
					_data.factionMap[_faction].color = _faction.factionColor
					
					table.insert(_data.factionList, _faction)
				end
			end
			--]]

		end
			
	_data.lv_users = LiveValue.New()
	_data.lv_users:AssignGetter(|| fy_User:GetAll())
	_data.lv_users:AddTrigger(_data.tr_onBegin)
	_data.lv_users:AddCallback(_OnGetUsers)
	end

	---builderCount
	do
	_data.lv_builderCount = LiveValue.New()
	_data.lv_builderCount:AssignGetter(UIGame.CountBuilders)
	_data.lv_builderCount:AddTrigger(_data.tr_onBegin)
	--_data.lv_builderCount:ConnectHook(UIGame.hk_OnOccupationChanged)
	_data.lv_builderCount:AddHook(UIGame.hk_OnOccupationChanged, _data.tr_onOpen, _data.tr_onClose)
	end

	---halberdierCount
	do
	_data.lv_halberdierCount = LiveValue.New()
	_data.lv_halberdierCount:AssignGetter(UIGame.CountHalberdiers)
	_data.lv_halberdierCount:AddTrigger(_data.tr_onBegin)
	--_data.lv_knightCount:ConnectHook(UIGame.hk_OnOccupationChanged)
	_data.lv_halberdierCount:AddHook(UIGame.hk_OnOccupationChanged, _data.tr_onOpen, _data.tr_onClose)
	end

	---knightCount
	do
	_data.lv_knightCount = LiveValue.New()
	_data.lv_knightCount:AssignGetter(UIGame.CountKnights)
	_data.lv_knightCount:AddTrigger(_data.tr_onBegin)
	--_data.lv_knightCount:ConnectHook(UIGame.hk_OnOccupationChanged)
	_data.lv_knightCount:AddHook(UIGame.hk_OnOccupationChanged, _data.tr_onOpen, _data.tr_onClose)
	end

	---archerCount
	do
	_data.lv_archerCount = LiveValue.New()
	_data.lv_archerCount:AssignGetter(UIGame.CountArchers)
	_data.lv_archerCount:AddTrigger(_data.tr_onBegin)
	--_data.lv_archerCount:ConnectHook(UIGame.hk_OnOccupationChanged)
	_data.lv_archerCount:AddHook(UIGame.hk_OnOccupationChanged, _data.tr_onOpen, _data.tr_onClose)
	end

	---crossbowtronCount
	do
	_data.lv_crossbowtronCount = LiveValue.New()
	_data.lv_crossbowtronCount:AssignGetter(UIGame.CountArbalists)
	_data.lv_crossbowtronCount:AddTrigger(_data.tr_onBegin)
	--_data.lv_archerCount:ConnectHook(UIGame.hk_OnOccupationChanged)
	_data.lv_crossbowtronCount:AddHook(UIGame.hk_OnOccupationChanged, _data.tr_onOpen, _data.tr_onClose)
	end

	---alchemistCount
	do
	_data.lv_alchemistCount = LiveValue.New()
	_data.lv_alchemistCount:AssignGetter(UIGame.CountAlchemists)
	_data.lv_alchemistCount:AddTrigger(_data.tr_onBegin)
	--_data.lv_archerCount:ConnectHook(UIGame.hk_OnOccupationChanged)
	_data.lv_alchemistCount:AddHook(UIGame.hk_OnOccupationChanged, _data.tr_onOpen, _data.tr_onClose)
	end

	---geomancerCount
	do
	_data.lv_geomancerCount = LiveValue.New()
	_data.lv_geomancerCount:AssignGetter(UIGame.CountArtificers)
	_data.lv_geomancerCount:AddTrigger(_data.tr_onBegin)
	--_data.lv_archerCount:ConnectHook(UIGame.hk_OnOccupationChanged)
	_data.lv_geomancerCount:AddHook(UIGame.hk_OnOccupationChanged, _data.tr_onOpen, _data.tr_onClose)
	end

	---crystals
	do
	_data.capturableList = {}
	_data.capturableMap = {}
	_data.crystalList = {}
	_data.shardList = {}

	_data.IsCapturableHome =
		function(capturable)
			_data.capturableMap[capturable].lv_isHome:GetValue()
		end

	local _OnSetIsCapturableHome =
		function(capturable)
			_data.tr_OnSetIsCapturableHome:RunCallbacks(capturable)
		end

	local _OnGetCapturables =
		function(capturables, previousCapturables)

			_data.capturableList = {}
			_data.capturableMap = {}
			_data.crystalList = {}
			_data.shardList = {}

			for i = 1, #capturables do
				
				local _capturable = capturables[i]
				
				table.insert(_data.capturableList, _capturable)
					
				_data.capturableMap[_capturable] = {}
				
				_data.capturableMap[_capturable].lv_isCrystal = LiveValue.New()
				_data.capturableMap[_capturable].lv_isCrystal:AssignGetter(||(_capturable.crystalType == 0))
				_data.capturableMap[_capturable].lv_isCrystal:RunGetter()
				--_data.capturableMap[_capturable].lv_isCrystal:ConnectHook(_capturable.hk_OnSetCrystalType)
				_data.capturableMap[_capturable].lv_isCrystal:AddHook(_capturable.hk_OnSetCrystalType, _data.tr_onOpen, _data.tr_onClose)

				_data.capturableMap[_capturable].lv_isHome = LiveValue.New()
				_data.capturableMap[_capturable].lv_isHome:AssignGetter(||_capturable.isFireflyHome)
				_data.capturableMap[_capturable].lv_isHome:RunGetter()
				_data.capturableMap[_capturable].lv_isHome:AddEvent(_data.ev_updateCrystals, true)
				--_data.capturableMap[_capturable].lv_isHome:AddCallback(||_data.tr_onUpdateCrystals:RunCallbacks())
				--_data.capturableMap[_capturable].lv_isHome:ConnectHook(_capturable.hk_OnSetIsFireflyHome)
				_data.capturableMap[_capturable].lv_isHome:AddHook(_capturable.hk_OnSetIsFireflyHome, _data.tr_onOpen, _data.tr_onClose)
				
				_data.capturableMap[_capturable].lv_faction = LiveValue.New()
				_data.capturableMap[_capturable].lv_faction:AssignGetter(||_capturable.faction)
				_data.capturableMap[_capturable].lv_faction:RunGetter()
				_data.capturableMap[_capturable].lv_faction:AddEvent(_data.ev_updateCrystals, true)
				--_data.capturableMap[_capturable].lv_faction:AddCallback(function() _menu.dirtyList[_data.tr_onUpdateCrystals] = true end)
				--_data.capturableMap[_capturable].lv_faction:AddCallback(||_data.tr_onUpdateCrystals:RunCallbacks())
				--_data.capturableMap[_capturable].lv_faction:ConnectHook(_capturable.hk_OnSetFaction)
				_data.capturableMap[_capturable].lv_faction:AddHook(_capturable.hk_OnSetFaction, _data.tr_onOpen, _data.tr_onClose)

				_data.capturableMap[_capturable].lv_captureFaction = LiveValue.New()
				_data.capturableMap[_capturable].lv_captureFaction:AssignGetter(||_capturable.captureFaction)
				_data.capturableMap[_capturable].lv_captureFaction:RunGetter()
				--_data.capturableMap[_capturable].lv_captureFaction:ConnectHook(_capturable.hk_OnSetCaptureFaction)
				_data.capturableMap[_capturable].lv_captureFaction:AddHook(_capturable.hk_OnSetCaptureFaction, _data.tr_onOpen, _data.tr_onClose)

				_data.capturableMap[_capturable].lv_captureRatio = LiveValue.New()
				_data.capturableMap[_capturable].lv_captureRatio:AssignGetter(||_capturable.captureRatio)
				_data.capturableMap[_capturable].lv_captureRatio:RunGetter()
				--_data.capturableMap[_capturable].lv_captureRatio:ConnectHook(_capturable.hk_OnSetCaptureRatio)
				_data.capturableMap[_capturable].lv_captureRatio:AddHook(_capturable.hk_OnSetCaptureRatio, _data.tr_onOpen, _data.tr_onClose)
				
				_data.capturableMap[_capturable].lv_soulRatio = LiveValue.New()
				_data.capturableMap[_capturable].lv_soulRatio:AssignGetter(||_capturable.crystalConversionProgress)
				_data.capturableMap[_capturable].lv_soulRatio:RunGetter()
				--_data.capturableMap[_capturable].lv_soulRatio:ConnectHook(_capturable.hk_OnSetCrystalConversionProgress)
				_data.capturableMap[_capturable].lv_soulRatio:AddHook(_capturable.hk_OnSetCrystalConversionProgress, _data.tr_onOpen, _data.tr_onClose)
				
				local _isCrystal =
					_data.capturableMap[_capturable].lv_isCrystal:GetValue()

				if(_isCrystal)then
					table.insert(_data.crystalList, _capturable)
				else
					table.insert(_data.shardList, _capturable)
				end
				

				--TODO: make homeCrystal a LiveValue eventually, as it might not be valid at this point
				local _isHome =
					_data.capturableMap[_capturable].lv_isHome:GetValue()

				local _isMine =
					_data.capturableMap[_capturable].lv_faction:GetValue() == _data.alliedFaction
					
				if(_isCrystal and _isHome and _isMine)then
					_data.homeCrystal = _capturable
				end
				
			end
		end
		
	_data.lv_crystals = LiveValue.New()
	_data.lv_crystals:AssignGetter(|| fy_Crystal:GetAll())
	_data.lv_crystals:AddTrigger(_data.tr_onBegin)
	_data.lv_crystals:AddCallback(_OnGetCapturables)
	end

	do
	local _OnBegin2 =
		function()
		
			---currentXp
			do
			_data.lv_currentXp = LiveValue.New()
			_data.lv_currentXp:AssignGetter(||_data.homeCrystal.availablePureEnergy)
			_data.lv_currentXp:RunGetter()
			--_data.lv_currentXp:ConnectHook(_data.homeCrystal.hk_OnSetAvailablePureEnergy)
			_data.lv_currentXp:AddHook(_data.homeCrystal.hk_OnSetAvailablePureEnergy, _data.tr_onOpen, _data.tr_onClose)
			end

			---currentTotalXp
			do
			_data.lv_currentTotalXp = LiveValue.New()
			_data.lv_currentTotalXp:AssignGetter(||_data.homeCrystal.newFireflyRequiredEnergy)
			_data.lv_currentTotalXp:RunGetter()
			_data.lv_currentTotalXp:AddHook(_data.homeCrystal.hk_OnSetFireflyCount, _data.tr_onOpen, _data.tr_onClose)
			_data.lv_currentTotalXp:AddHook(_data.homeCrystal.hk_OnSetNewFireflyRequiredEnergy, _data.tr_onOpen, _data.tr_onClose)
			--_data.lv_currentTotalXp:AddTrigger(_data.tr_onSlowUpdate)
			end
	
			---autoRespawnFireflyCount
			do
			_data.lv_autoRespawnFireflyCount = LiveValue.New()
			_data.lv_autoRespawnFireflyCount:AssignGetter(||_data.homeCrystal.autoRespawnFireflyCount)
			_data.lv_autoRespawnFireflyCount:RunGetter()
			--_data.lv_autoRespawnFireflyCount:ConnectHook(_data.homeCrystal.hk_OnSetFireflyCount)
			_data.lv_autoRespawnFireflyCount:AddHook(_data.homeCrystal.hk_OnSetFireflyCount, _data.tr_onOpen, _data.tr_onClose)
			end
	
			---newSpawnableFireflyCount
			do
			_data.lv_newSpawnableFireflyCount = LiveValue.New()
			_data.lv_newSpawnableFireflyCount:AssignGetter(||_data.homeCrystal.newSpawnableFireflyCount)
			_data.lv_newSpawnableFireflyCount:RunGetter()
			--_data.lv_newSpawnableFireflyCount:ConnectHook(_data.homeCrystal.hk_OnSetFireflyCount)
			_data.lv_newSpawnableFireflyCount:AddHook(_data.homeCrystal.hk_OnSetFireflyCount, _data.tr_onOpen, _data.tr_onClose)
			end

		end

	_data.tr_onBegin2:AddCallback(_OnBegin2)
	end


	
	---buildPhaseRemainingTime
	do
	_data.lv_buildPhaseRemainingTime = LiveValue.New()
	_data.lv_buildPhaseRemainingTime:AssignGetter(||Registry.timers.timeBeforeCanCapture:FormatedRemainingSeconds())
	--_data.lv_buildPhaseRemainingTime:AssignGetter(function() print("remaining: " .. tostring(Registry.timers.timeBeforeCanCapture:RemainingSeconds())) return Registry.timers.timeBeforeCanCapture:RemainingSeconds() end)
	_data.lv_buildPhaseRemainingTime:AddTrigger(_data.tr_onBegin)
	_data.lv_buildPhaseRemainingTime:AddTrigger(_data.tr_onSlowUpdate)
	end

	---buildPhaseDone
	do
	_data.lv_buildPhaseDone = LiveValue.New()
	_data.lv_buildPhaseDone:AssignGetter(||Rules.canCaptureCapturables)
	--_data.lv_buildPhaseDone:AssignGetter(function() print("remaining: " .. tostring(Registry.timers.timeBeforeCanCapture:RemainingSeconds())) return Registry.timers.timeBeforeCanCapture:RemainingSeconds() end)
	_data.lv_buildPhaseDone:AddTrigger(_data.tr_onBegin)
	--_data.lv_buildPhaseDone:ConnectHook(Rules.hk_OnSetCanCaptureCapturables)
	_data.lv_buildPhaseDone:AddHook(Rules.hk_OnSetCanCaptureCapturables, _data.tr_onOpen, _data.tr_onClose)
	end

	--begin
	_data.tr_onBegin:RunCallbacks()
	_data.tr_onBegin2:RunCallbacks()
	_data.tr_onUpdateCrystals:RunCallbacks()
	
	--spacer
	do
	local _p =
		dofile("LUI/Panels/SpacerPanel.lua")
		
	_p.Visible = ||_menu.sidebarSpacerVisible
	_p.Width = ||-1
	_p.Height = ||64
	_p.FlexibleWidth = ||1
	_p.FlexibleHeight = ||-1

	_menu.AddPanel(_p)
	_menu.spacer = _p
	end

	--sub
	do
	local _p =
		dofile("Gamemodes/conquer/Menu/ConquerFactionsPanel.lua")
		.SetData(_menu.data)
		.Build()

	_menu.AddPanel(_p)
	_menu.teamsPanel = _p
	end
	
	--[[
	--spacer
	do
	local _p =
		dofile("LUI/Panels/SpacerPanel.lua")
		
	_p.Width = ||-1
	_p.Height = ||1
	_p.FlexibleWidth = ||1
	_p.FlexibleHeight = ||-1

	_menu.AddPanel(_p)
	end
	--]]

end




----------------------
--Flags---
do
local OnOpen =
	function()
		if not _menu.ready then return end
		_menu.data.tr_onOpen:RunCallbacks()
	end
local OnClose =
	function()
		if not _menu.ready then return end
		_menu.data.tr_onClose:RunCallbacks()
	end
_menu.sidebarSpacerVisible = false
_menu.OnSetSidebarSpacer =
	function(flag)
		_menu.sidebarSpacerVisible = flag
		_menu.spacer.Refresh()
	end
	
_menu.SetFlag(MenuFlag.OnOpen, OnOpen)
_menu.SetFlag(MenuFlag.OnClose, OnClose)
_menu.SetFlag(MenuFlag.OpenOnLoad)
_menu.SetFlag(MenuFlag.ReopenOnReload)
_menu.SetFlag(MenuFlag.OpensChildMenus)
end



-------
--init
Data.Storage:OnSetContent()
-------


---Connections---
_menu.ConnectWhen(FactionStorage.hk_OnSetContent,			||Data.Storage:OnSetContent(),				MenuState.Opened)
_menu.ConnectWhen(hk_SlowUpdate, ||_menu.OnSlowUpdate(), MenuState.Opened)
_menu.ConnectWhen(UIGame.hk_SetSidebarSpacer, _menu.OnSetSidebarSpacer, MenuState.Opened)


---Return---
return _menu
