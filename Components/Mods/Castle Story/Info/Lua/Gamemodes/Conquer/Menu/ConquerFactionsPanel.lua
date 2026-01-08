require("LUI2.lua")
-------



-------
--panels

--panels: root
local _p =
	LayoutPanels.Vertical:New()
	.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||true, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||true, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredWidth, ||272, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredPosition, ||Vector2.New(-24, 0), true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.zero, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.UpperLeft, true)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false, true)

--panels: factions
_p.factionPanels = {}
_p.AddFactionPanels =
	function()
		for i, faction in ipairs(_p.data.factionList) do
			_p.AddFactionPanel(faction)
		end
	end
_p.AddFactionPanel =
	function(faction)
	
		if faction == _p.data.corruptedFaction then return end

		---layout
		do
		local p =
			LayoutPanels.Vertical:New()
			.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||true, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||0, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||0, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
			.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.zero, true)
			.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.MiddleRight, true)
			.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true, true)
			.SetWidgetHandle(Widgets.Image, Handles.Color, ||_bgColorDark, true)

		_p.AddPanel(p)
		_p.factionPanels[faction] = p
		end
		
		---players
		do
		local p =
			dofile("Gamemodes/Conquer/Menu/ConquerFactionPlayersPanel.lua")
			.SetData(_p.data)
			.SetFaction(faction)
			.Build()
	
		_p.factionPanels[faction].AddPanel(p)
		_p.factionPanels[faction].players = p
		end
		
		--storage
		do
		if faction == _p.data.alliedFaction then
		local p = dofile("LUI/Panels/Gamemodes/Panel_Storage.lua")

		_p.factionPanels[faction].AddPanel(p)
		_p.factionPanels[faction].storage = p
		end
		end
		
	end
-------



-------
--external funcs
_p.SetData =
	function(data)
		_p.data = data
		return _p
	end
_p.Build =
	function()
		_p.AddFactionPanels()
		return _p
	end
-------
	


-------
--return
return _p