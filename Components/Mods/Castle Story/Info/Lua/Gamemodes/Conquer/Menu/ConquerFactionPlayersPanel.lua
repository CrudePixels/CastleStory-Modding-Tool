require("LUI2.lua")




---panel
local _panel =
	LayoutPanels.Horizontal:New()
	.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||0, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(8,8,8,8), true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.UpperCenter, true)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true)



---add player
_panel.playerPanels = {}
_panel.AddPlayerPanel =
	function()

		---info
		local _info =
			_panel.data.factionMap[_panel.faction]

			
		---name
		local _name = ""
		if IsMultiplayer() then
			local humanUsers = fy_User:GetAll()
			for x, item in pairs(humanUsers) do
				if x ~= #humanUsers then
					_name = _name .. item.name .. " & "
				else
					_name = _name .. item.name
				end
			end
		end
		
		local _isSelfFaction = _panel.faction == _panel.data.alliedFaction
		local _isNeutralFaction = _panel.faction == _panel.data.neutralFaction
		local _isEnemyFaction = _panel.faction == _panel.data.corruptedFaction

		local _fontSize = 12
		local _height = 24

		if(_isNeutralFaction)then
			_name = GetLocalized("##mode_conquer")
			_fontSize = 14
			_height = 24
		end

		local _Time =
			function()
				local _time = tostring(_panel.data.lv_elapsedTime:GetValue())
				return _time
			end

		local _NameText =
			function()
				if(_isEnemyFaction)then
					return _WaveText()
				end
				return _name
			end

		local _NameColor =
			function()
				if(_isNeutralFaction)then
					return Gray
				end
				return CastleYellow
			end


		---conquer timer
		if(_isNeutralFaction)then

			---name
			do
			local _p =
				Panels.Label:New()
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.AutoMinWidth, ||true, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.MinHeight, ||_height, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.Pivot, ||Vector2.New(0,0.5), true)
				.SetWidgetHandle(Widgets.Label, Handles.FontSize, ||_fontSize, true)
				.SetWidgetHandle(Widgets.Label, Handles.Alignment, ||Alignment.MiddleLeft, true)
				.SetWidgetHandle(Widgets.Label, Handles.Color, ||_NameColor(), true)
				.SetWidgetHandle(Widgets.Label, Handles.Text, ||_NameText())
	
				---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close
				_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)

			_panel.AddPanel(_p)
			_panel.nameLbl = _p
			end
		
			do
			local _p =
				Panels.Label:New()
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.MinWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.MinHeight, ||_height, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleWidth, ||1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.Pivot, ||Vector2.New(1,0.5), true)
				.SetWidgetHandle(Widgets.Label, Handles.FontSize, ||_fontSize, true)
				.SetWidgetHandle(Widgets.Label, Handles.Alignment, ||Alignment.MiddleRight, true)
				.SetWidgetHandle(Widgets.Label, Handles.Color, ||Gray, true)
				.SetWidgetHandle(Widgets.Label, Handles.Text, ||_Time())
	
			---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close
			_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)

			_panel.AddPanel(_p)
			_panel.nameLbl = _p
			end
		end

		--minimap
		if (_isSelfFaction) then
			---panel
			do
			local _p =
				LayoutPanels.Vertical:New()
				.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||true, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||0, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
				.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(8,8,8,8), true)
				.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.UpperCenter, true)
				.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false, true)

				_panel.AddPanel(_p)
				_panel.layout = _p
			end

			---name
			do
				local _p =
					Panels.Label:New()
						.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.MinHeight, ||_height, true)
						.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredWidth, ||196, true)
						.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredHeight, ||_height, true)
						.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleWidth, ||-1, true)
						.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleHeight, ||-1, true)
						.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.Pivot, ||Vector2.New(0,0.5), true)
						.SetWidgetHandle(Widgets.Label, Handles.FontSize, ||_fontSize, true)
						.SetWidgetHandle(Widgets.Label, Handles.Alignment, ||Alignment.UpperLeft, true)
						.SetWidgetHandle(Widgets.Label, Handles.Color, ||_NameColor(), true)
						.SetWidgetHandle(Widgets.Label, Handles.Text, ||_name)
	
					_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)

				_panel.layout.AddPanel(_p)
				_panel.layout.nameLbl = _p
			end


			--minimap
			do
			local m =
				dofile("LUI/Menus/MinimapMenu.lua")


				local p =
				LayoutPanels.Vertical:New()
				.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||true, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||true, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredWidth, ||256, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredHeight, ||256, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMin, ||Vector2.New(0, 1), true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMax, ||Vector2.New(0, 1), true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.Pivot, ||Vector2.New(0, 1), true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredPosition, ||Vector2.New(0,0), true)
				.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
				.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false, true)
				.SetMenu(m)

				_panel.layout.minimapPanel = p

				local pp =
				LayoutPanels.Vertical:New()
				.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||true, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||256, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||1, true)
				.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1, true)
				.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
				.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false, true)
				.AddPanel(p)

				_minimapRoot.AddMenu(m)
				_panel.layout.AddPanel(pp)
			end					
		end


		if (not _isSelfFaction and not _isNeutralFaction) then			
			_panel.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||false)			
		end
		_panel.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false)


	end

---external funcs
_panel.SetData =
	function(_data)
		_panel.data = _data
		return _panel
	end
_panel.SetFaction =
	function(_faction)
		_panel.faction = _faction
		return _panel
	end
_panel.Build =
	function()
		if(not _isEnemyFaction)then
			_panel.AddPlayerPanel()
		end
		return _panel
	end




----------------------
---return
return _panel