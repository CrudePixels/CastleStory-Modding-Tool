require("LUI2.lua")


---panel
local _panel =
	LayoutPanels.Horizontal:New()

	
---build phase?
local _VisiblePanel =
	function()
		if(not(_panel.data))then return false end
		if(not(_panel.faction == _panel.data.neutralFaction))then return true end
		if(_panel.data.lv_hasCorrupteds:GetValue())then return true end
		return _panel.data.lv_buildPhaseDone:GetValue()
	end

---panel CTD
_panel
	.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||_VisiblePanel())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||0, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(6,6,6,8), true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.MiddleLeft, true)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false, true)
	

---divider
_panel.AddDividerPanel =
	function()
		do
		local _p =
			dofile("LUI/Panels/VerticalDividerPanelB.lua")

		_p.Height = ||20
		_p.FlexibleHeight = ||-1
		_p.FlexibleWidth = ||1

		_panel.data.lv_buildPhaseDone:AddCallback(function() if(_p)then _p.RefreshAll() end end)

		_panel.AddPanel(_p)
		end
	end

---crystal
_panel.crystalSoulSprite = IconKeys.UI_Crystal:Get64()
_panel.crystalCaptureSprite = IconKeys.UI_CrystalOutline:Get64()
_panel.shardSoulSprite = IconKeys.UI_Shard:Get64()
_panel.shardCaptureSprite = IconKeys.UI_ShardOutline:Get64()

_panel.GetShardIndex =
	function(_shard)
		local _shardIndex = 0
		for _capturableIndex = 1, #_panel.data.capturableList do
			local _capturable = _panel.data.capturableList[_capturableIndex]
			local _capturableIsCrystal = _panel.data.capturableMap[_capturable].lv_isCrystal:GetValue()
			local _capturableFaction = _panel.data.capturableMap[_capturable].lv_faction:GetValue()
			if(not(_capturableIsCrystal))then
				if(_capturableFaction == _panel.faction)then
					_shardIndex = _shardIndex + 1
					if(_shard == _capturable)then
						return _shardIndex
					end
				end
			end
		end
		return 0
	end

---add capturables
_panel.crystalPanels = {}
_panel.AddCrystalPanels =
	function()
		for _crystalIndex = 1, #_panel.data.capturableList do
			_panel.AddCapturablePanel(_panel.data.capturableList[_crystalIndex], _panel.crystalPanels)
		end
	end
_panel.shardPanels = {}
_panel.AddShardPanels =
	function()
		for _shardIndex = 1, #_panel.data.capturableList do
			_panel.AddCapturablePanel(_panel.data.capturableList[_shardIndex], _panel.shardPanels)
		end
	end
_panel.AddCapturablePanel =
	function(_capturable, _panels)
	
		---info
		local _info =
			_panel.data.capturableMap[_capturable]

		local _wantsCrystal =
			_panels == _panel.crystalPanels

		local _isNeutral =
			_panel.faction == _panel.data.neutralFaction
		local _isCorrupted =
			_panel.faction == _panel.data.corruptedFaction
		
		local _IsCrystal =
			||_capturable.isFireflyHome
			--||_info.lv_isCrystal:GetValue()

		local _VisibleFaction =
			||_panel.faction == _capturable.faction
			--||_panel.faction == _info.lv_faction:GetValue()
		local _VisibleType =
			||_IsCrystal() == _wantsCrystal

		local _Visible =
			||_VisibleFaction() and _VisibleType()

		local _Width =
			function()
				if(_IsCrystal())then
					return 24
				end
				return 12
			end
		
		local _height = 32
		
		local _iconWidth = 32
		local _iconHeight = 32

		local _alignment = Alignment.MiddleLeft

		local _CaptureSprite =
			function()
				if(_IsCrystal())then
					return _panel.crystalCaptureSprite
				end
				return _panel.shardCaptureSprite
			end

		local _SoulSprite =
			function()
				if(_IsCrystal())then
					return _panel.crystalSoulSprite
				end
				return _panel.shardSoulSprite
			end

		local _IconPos = 
			function()
				local _iconPosOffet = 0
				if(not(_IsCrystal()))then
					_iconPosOffet = 5
					local _i = _panel.GetShardIndex(_capturable)
					if(_i % 2 == 0)then _iconPosOffet = -_iconPosOffet end
				end
				return Vector2.New(0, _iconPosOffet)
			end
		local _iconPos = _IconPos()

		_panel.data.tr_onUpdateCrystals:AddCallback(function() _iconPos = _IconPos() end)


		local _CaptureBgColor =
			function()
				--local _iFaction = _info.lv_faction:GetValue()
				local _iFaction = _capturable.faction
				local _iFactionColor = _panel.data.factionMap[_iFaction].color
				return _iFactionColor
			end
			
		local _CaptureFgColor =
			function()
				--local _iFaction = _info.lv_captureFaction:GetValue()
				local _iFaction = _capturable.captureFaction
				local _iFactionColor = _panel.data.factionMap[_iFaction].color
				return _iFactionColor
			end

		local _CaptureProgress =
			function()
				--local captureRatio = _info.lv_captureRatio:GetValue()
				local captureRatio = _capturable.captureRatio
				if(_IsCrystal())then
					local _min = 0.05
					local _max = 0.95
					return ((captureRatio) * (_max - _min)) + _min
				else
					local _min = 0.085
					local _max = 0.91
					return ((captureRatio) * (_max - _min)) + _min
				end
			end
			
		local _SoulColor =
			function()
				--local _iFaction = _info.lv_faction:GetValue()
				local _iFaction = _capturable.faction
				local _iFactionColor = _panel.data.factionMap[_iFaction].color
				return _iFactionColor
			end

		local _SoulProgress =
			function()
				--local soulRatio = _info.lv_soulRatio:GetValue()
				local soulRatio = _capturable.crystalConversionProgress
				if(_IsCrystal())then
					local _min = 0.1
					local _max = 0.86
					return ((soulRatio) * (_max - _min)) + 0.05
				else
					local _min = 0.1
					local _max = 0.85
					return ((soulRatio) * (_max - _min)) + 0.05
				end
			end

		---layout
		do
		local _p =
			LayoutPanels.Horizontal:New()
			.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||_Visible()) -- crystal.faction, crystal.crystalType
			.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||false, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||_Width()) -- crystal.crystalType
			.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||_height, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
			.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.zero, true)
			.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||_alignment, true)
			.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false, true)
			
		---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close
		---TODO: replace with proper trigger
		--_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)
		_panel.data.tr_onUpdateCrystals:AddCallback(function() if(_p)then _p.Refresh() end end)
		_panel.data.lv_buildPhaseDone:AddCallback(function() if(_p)then _p.Refresh() end end)
		--_info.lv_isCrystal:AddCallback(function() if(_p)then _p.Refresh() end end)
		--_info.lv_faction:AddCallback(function() if(_p)then _p.Refresh() end end)

		_panel.AddPanel(_p)
		_panels[_capturable] = _p
		end
	
		---capture bg
		do

		local _p =
			Panels.Image:New()
			.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||true, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredWidth, ||_iconWidth, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredHeight, ||_iconHeight, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMin, ||Vector2.New(0.5, 0.5), true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMax, ||Vector2.New(0.5, 0.5), true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredPosition, ||_iconPos) -- anyCrystal.crystalType, anyCrystal.faction
			.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true, true)
			.SetWidgetHandle(Widgets.Image, Handles.Sprite, ||_CaptureSprite()) -- crystal.crystalType
			.SetWidgetHandle(Widgets.Image, Handles.Color, ||_CaptureBgColor()) -- crystal.faction
			
		---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close
		---TODO: replace with proper trigger
		--_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)--tmp: ensure this is necessary (for perf)
		_panel.data.tr_onUpdateCrystals:AddCallback(function() if(_p)then _p.Refresh() end end)
		_panel.data.lv_buildPhaseDone:AddCallback(function() if(_p)then _p.Refresh() end end)

		_panels[_capturable].AddPanel(_p)
		_panels[_capturable].captureBg = _p
		end
	
		---capture fg
		do
		local _p =
			Panels.Image:New()
			.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||true, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredWidth, ||_iconWidth, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredHeight, ||_iconHeight, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMin, ||Vector2.New(0.5, 0.5), true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMax, ||Vector2.New(0.5, 0.5), true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredPosition, ||_iconPos)
			.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true, true)
			.SetWidgetHandle(Widgets.Image, Handles.Sprite, ||_CaptureSprite()) -- capturable.crystalType
			.SetWidgetHandle(Widgets.Image, Handles.Color, ||_CaptureFgColor()) -- capturable.faction
			.SetWidgetHandle(Widgets.Image, Handles.Fill, ||_CaptureProgress()) -- capturable.captureRatio
			.SetWidgetHandle(Widgets.Image, Handles.FillMethod, ||FillMethod.Vertical, true)
			
		---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close
		---TODO: replace with proper trigger
		--_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)
		_panel.data.tr_onUpdateCrystals:AddCallback(function() if(_p)then _p.Refresh() end end)
		_panel.data.lv_buildPhaseDone:AddCallback(function() if(_p)then _p.Refresh() end end)
		_info.lv_captureRatio:AddCallback(function() if(_p)then _p.Refresh() end end)
			
		_panels[_capturable].AddPanel(_p)
		_panels[_capturable].captureFg = _p
		end

	
		---soul fg
		--if(not(_isNeutral) and not(_isCorrupted))then do
		if(not(_isNeutral))then do
		local _p =
			Panels.Image:New()
			.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||true, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||true, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredWidth, ||_iconWidth, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredHeight, ||_iconHeight, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMin, ||Vector2.New(0.5, 0.5), true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMax, ||Vector2.New(0.5, 0.5), true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredPosition, ||_iconPos)
			.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true, true)
			.SetWidgetHandle(Widgets.Image, Handles.Sprite, ||_SoulSprite()) -- capturable.crystalType
			.SetWidgetHandle(Widgets.Image, Handles.Color, ||_SoulColor()) -- capturable.faction
			.SetWidgetHandle(Widgets.Image, Handles.Fill, ||_SoulProgress()) -- capturable.soulRatio
			.SetWidgetHandle(Widgets.Image, Handles.FillMethod, ||FillMethod.Vertical, true)
			
		---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close
		---TODO: replace with proper trigger
		--_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)
		_panel.data.tr_onUpdateCrystals:AddCallback(function() if(_p)then _p.Refresh() end end)
		_panel.data.lv_buildPhaseDone:AddCallback(function() if(_p)then _p.Refresh() end end)
		_info.lv_soulRatio:AddCallback(function() if(_p)then _p.Refresh() end end)
			
		_panels[_capturable].AddPanel(_p)
		_panels[_capturable].soulFg = _p
		end
    end
	end

---external funcs
_panel.SetData =
	function(_data)

		_panel.data = _data
		_panel.data.lv_buildPhaseDone:AddCallback(function() _panel.Refresh() end)
		_panel.data.lv_hasCorrupteds:AddCallback(function() _panel.Refresh() end)

		return _panel
	end
_panel.SetFaction =
	function(_faction)
		_panel.faction = _faction
		return _panel
	end
_panel.Build =
	function()
		_panel.AddCrystalPanels()
		_panel.AddDividerPanel()
		_panel.AddShardPanels()
		return _panel
	end




----------------------
---return
return _panel