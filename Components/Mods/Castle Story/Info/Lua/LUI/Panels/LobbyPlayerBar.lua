---Includes-----------
require("LUI2.lua")
----------------------

---Tmp----------------
local _black25			= Color.New(0,0,0,0.25)
local _black50			= Color.New(0,0,0,0.50)
local _black75			= Color.New(0,0,0,0.75)
local _black100			= Color.New(0,0,0,1)
local _black			= Color.New(0,0,0,1)
local _white			= Color.New(1,1,1,1)
local _NoneAction		= function() end

local Highlight = {}
Highlight.Left = "Left"
Highlight.None = "None"
----------------------


---Panel--------------
local _panel =
	LayoutPanels.Horizontal:New()
----------------------


---Fields-------------
_panel.hovering	= false
_panel.pressing	= false

local _cfg			= {}
local _userCfg		= {}
local _defaultCfg	= {}
local _preCfg		= {}
local _postCfg		= {}
local _guardCfg		= {}
----------------------


---Default Cfg--------
_defaultCfg.Visible					= ||true
_defaultCfg.Enabled					= ||false
_defaultCfg.Width					= ||-1
_defaultCfg.Height					= ||48
_defaultCfg.FlexibleWidth			= ||1
_defaultCfg.FlexibleHeight			= ||-1
_defaultCfg.Label_Text				= || "nothing"
_defaultCfg.Padding					= || Vector4.New(16,0,0,0)
_defaultCfg.Spacing					= || 4
_defaultCfg.Color					= ||_black  --function() if(_panel.hovering or _panel.showing)then return _black75 end return _black25 end
_defaultCfg.ListImage_Color			= ||_black
_defaultCfg.OnMenuRefresh			= function() end

_defaultCfg.Host					= || false
_defaultCfg.Image_Sprite1			= || GetIcon(IconKeys.UI_Crown).Image64
_defaultCfg.Image_Sprite2			= || GetIcon(IconKeys.UI_Check).Image64
_defaultCfg.Image_Width				= ||32
_defaultCfg.Image_Height			= ||32
_defaultCfg.Image_Sprite			= function() if(_cfg.Host()) then return _cfg.Image_Sprite1() end return _cfg.Image_Sprite2() end
_defaultCfg.Image_Color				= ||CastleYellow

_defaultCfg.Button_OnAction			= function() end
_defaultCfg.Button_OnPointerEnter	= function() end
_defaultCfg.Button_OnPointerExit	= function() end
_defaultCfg.Button_OnPointerDown	= function() end
_defaultCfg.Button_OnPointerUp		= function() end
_defaultCfg.Button_Highlight		= function() if(_panel.pressing)then return _cfg.Button_HighlightOn() end return Highlight.None end
_defaultCfg.Button_HighlightOn		= ||Highlight.Left

_defaultCfg.Dropdown_Options_Color		= ||{}
_defaultCfg.Dropdown_Selection_Color	= ||0
_defaultCfg.Dropdown_SetSelection_Color	= function(v) end
_defaultCfg.Dropdown_Options_team		= ||{}
_defaultCfg.Dropdown_Selection_team		= ||0
_defaultCfg.Dropdown_SetSelection_team	= function(v) end
_defaultCfg.Dropdown_OnShow			= function() end
_defaultCfg.Dropdown_OnHide			= function() end

_defaultCfg.ValueLabel_Text			= ||"nothing to show"
_defaultCfg.ValueLabel_Color		= ||CastleYellow
_defaultCfg.ValueLabel_FontSize		= ||12
_defaultCfg.ValueLabel_Font			= ||Font.ProximaNovaRegular
_defaultCfg.SoundContext			= ||"main"
----------------------


---Post Cfg-----------
_postCfg.Button_OnAction		= function() Data.Sound:Play(Meta.Sound[_cfg.SoundContext()][_cfg.Button_OnActionSound()]) end
_postCfg.Button_OnPointerEnter	= function() Data.Sound:Play(Meta.Sound[_cfg.SoundContext()].rolloveron)		_panel.hovering = true	_panel.Refresh() end
_postCfg.Button_OnPointerExit	= function() Data.Sound:Play(Meta.Sound[_cfg.SoundContext()].rolloveroff)	_panel.hovering = false	_panel.Refresh() end
_postCfg.Button_OnPointerDown	= function() _panel.pressing = true	_panel.button.Refresh()	end
_postCfg.Button_OnPointerUp		= function() _panel.pressing = false	_panel.button.Refresh() end
----------------------


---Cfg----------------
local _UpdateCfg =
	function()
		for _k, _V in pairs(_defaultCfg) do
			_cfg[_k] =
				function(a)
					if(_preCfg[_k])then _preCfg[_k](a) end
					local _v = (_userCfg[_k] or _defaultCfg[_k])(a)
					if(_postCfg[_k])then _postCfg[_k](a) end
					return _v
				end
		end
	end
_UpdateCfg()
----------------------


---External Setters---
local _AddSetters =
	function()
		for _k, _v in pairs(_cfg) do
			_panel["Set_" .. _k] = function(v) _userCfg[_k] = v _UpdateCfg() return _panel end
		end
	end
_AddSetters()
----------------------

--------------------------------------------

---Panel--------------
_panel
	.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||_cfg.Visible(), true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||_cfg.Width(), true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||_cfg.Height(), true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||_cfg.Width(), true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||_cfg.Height(), true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||_cfg.FlexibleWidth(), true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||_cfg.FlexibleHeight(), true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.UpperLeft, true)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true, true)
	.SetWidgetHandle(Widgets.Image, Handles.Color, ||Color.New(1,0,0,0.5), true)
----------------------


----dropdown "color"
do
local _optionsColor = {}

do
	local _i = 1

	do
		local _t = {}
		_t.text = GetLocalized("green")
		_t.image = GetIcon(IconKeys.FlagGreen).Image64
		_t.index = _i
		table.insert(_optionsColor, _t)
		_i = _i + 1
	end

	do
		local _t = {}
		_t.text = GetLocalized("blue")
		_t.image = GetIcon(IconKeys.FlagBlue).Image64
		_t.index = _i
		table.insert(_optionsColor, _t)
		_i = _i + 1
	end

	do
		local _t = {}
		_t.text = GetLocalized("red")
		_t.image = GetIcon(IconKeys.FlagRed).Image64
		_t.index = _i
		table.insert(_optionsColor, _t)
		_i = _i + 1
	end

	do
		local _t = {}
		_t.text = GetLocalized("orange")
		_t.image = GetIcon(IconKeys.FlagOrange).Image64
		_t.index = _i
		table.insert(_optionsColor, _t)
		_i = _i + 1
	end
	
	do
		local _t = {}
		_t.text = GetLocalized("yellow")
		_t.image = GetIcon(IconKeys.FlagYellow).Image64
		_t.index = _i
		table.insert(_optionsColor, _t)
		_i = _i + 1
	end
end


local _ddValueColor = 1
local _SelectionColor =
	function()
		return _ddValueColor
	end

local _SetSelectionColor =
	function(v)
		_ddValueColor = v
		_menu.Refresh()
	end


local _p =
	dofile("LUI/Panels/DropdownPanel.lua")
	.Set_Height(|| 48)
	.Set_FlexibleWidth(|| 1)
	.Set_Dropdown_Selection(_SelectionColor)
	.Set_Dropdown_SetSelection(_SetSelectionColor)
	.Set_Dropdown_Options(||_optionsColor)
	.Set_ValueLabel_Color(|| _white)
	.Set_ValueLabel_Font(|| 14)
	.Set_ValueImage_Enabled(|| true)

_panel.AddPanel(_p)
_panel.colorDropdown = _p
end


----input field "Player Name"
do
local _p =
	dofile("LUI/Panels/InputFieldPanel.lua")
	.Set_Height(|| 48)
	--.Set_FlexibleWidth(|| 1)
	--.Set_Width(|| buttonWidth)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	--.Set_Label_Alignment(||Alignment.MiddleCenter)
	--.Set_Label_Text(||"Assign Key Binding")
	--.Set_Label_Font(|| Font.KorolevLight)
	.Set_Label_FontSize(|| 14)
	--.Set_Label_Color(||CastleYellow)
	.Set_InputField_Color(|| Color.New(0,0,0,1))

_panel.AddPanel(_p)
end
----------------------


do
local _optionsTeam = {}

do
	local _i = 1

	do
		local _t = {}
		_t.text = GetLocalized("Team 1")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end

	do
		local _t = {}
		_t.text = GetLocalized("Team 2")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end

	do
		local _t = {}
		_t.text = GetLocalized("Team 3")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end

	do
		local _t = {}
		_t.text = GetLocalized("Team 4")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end
	
	do
		local _t = {}
		_t.text = GetLocalized("Team 5")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end
	
	do
		local _t = {}
		_t.text = GetLocalized("Team 6")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end
	
	do
		local _t = {}
		_t.text = GetLocalized("Team 7")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end

	do
		local _t = {}
		_t.text = GetLocalized("Team 8")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end
	
	do
		local _t = {}
		_t.text = GetLocalized("Team 9")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end
	
	do
		local _t = {}
		_t.text = GetLocalized("Team 10")
		_t.image = nil
		_t.index = _i
		table.insert(_optionsTeam, _t)
		_i = _i + 1
	end
end


local _ddValueTeam = 1
local _SelectionTeam =
	function()
		return _ddValueTeam
	end

local _SetSelectionTeam =
	function(v)
		_ddValueTeam = v
		_menu.Refresh()
	end

local _p =
	dofile("LUI/Panels/DropdownPanel.lua")
	.Set_Height(|| 48)
	.Set_FlexibleWidth(|| 1)
	.Set_Color(|| Color.New(0,0,0,1))
	.Set_Dropdown_Selection(_SelectionTeam)
	.Set_Dropdown_SetSelection(_SetSelectionTeam)
	.Set_Dropdown_Options(||_optionsTeam)
	.Set_ValueLabel_Font(|| 14)
	.Set_ValueLabel_Color(|| _white)

_panel.AddPanel(_p)
_panel.teamDropdown= _p
end


----labeled value panel "Ping"
do
local _p =
	dofile("LUI/Panels/LabeledValuePanel.lua")
	.Set_Height(|| 48)
	.Set_Width(|| 48)
	.Set_FlexibleWidth(|| -1)
	.Set_Padding(||Vector4.New(0,0,5,12))
	.Set_Label_Alignment(||Alignment.MiddleCenter)
	.Set_Label_Text(||"Ping")
	.Set_Label_Font(|| Font.KorolevLight)
	--.Set_Label_FontSize(|| 20)
	.Set_Label_Color(||Gray)
	.Set_Value_Text(||"0")
	.Set_Value_Color(||Gray)
	.Set_Value_Alignment(||Alignment.UpperCenter)
	.Set_Color(|| Color.New(0,0,0,1))

_panel.AddPanel(_p)
end
----------------------


---ValueLayout-----------
do
local _p =
	LayoutPanels.Horizontal:New()
	--.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||_cfg.Visible()) -- is still visible
	--.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||true)
	--.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredWidth, || 100)
	--.SetWidgetHandle(Widgets.LayoutElement, Handles.Pivot, ||Vector2.New(0.5, 0.5))
	--.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchoredPosition, ||Vector2.New(-50, 0))
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||_cfg.Height())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||_cfg.Height())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||_cfg.Height())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||_cfg.Height())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||-1)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.zero)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.MiddleCenter)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true) -- need to be set to true if the panel is to appear
	.SetWidgetHandle(Widgets.Image, Handles.Color, ||_cfg.Color())

_panel.CheckboxLayout = _p
_panel.AddPanel(_p)
end
----------------------


---ValueImage-----------
do
local _p =
	Panels.Image:New()
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||_cfg.Image_Width())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||_cfg.Image_Height())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||_cfg.Image_Width())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||_cfg.Image_Height())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||-1)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1)
	.SetWidgetHandle(Widgets.Image, Handles.Sprite, || _cfg.Image_Sprite())
	.SetWidgetHandle(Widgets.Image, Handles.Color, ||CastleYellow)

_panel.CheckboxLayout.AddPanel(_p)
end
----------------------


---ValueButton-----------
do
local _p =
	Panels.Buttons.Standard:New()
	.SetWidgetHandle(Widgets.Button, Handles.OnPointerEnter, _PlayHoverSound)
    .SetWidgetHandle(Widgets.Button, Handles.OnPointerExit, _PlayUnhoverSound)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.IgnoreLayout, ||true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.OffsetMin, ||Vector2.zero)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.OffsetMax, ||Vector2.zero)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMin, ||Vector2.zero)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.AnchorMax, ||Vector2.one)
	.SetWidgetHandle(Widgets.Button, Handles.Interactable, ||_cfg.Button_Interactable())
	.SetWidgetHandle(Widgets.Button, Handles.OnAction, ||_cfg.Button_OnAction())
	.SetWidgetHandle(Widgets.Button, Handles.OnPointerEnter, ||_cfg.Button_OnPointerEnter())
	.SetWidgetHandle(Widgets.Button, Handles.OnPointerExit, ||_cfg.Button_OnPointerExit())
	.SetWidgetHandle(Widgets.Button, Handles.OnPointerUp, ||_cfg.Button_OnPointerUp())
	.SetWidgetHandle(Widgets.Button, Handles.OnPointerDown, ||_cfg.Button_OnPointerDown())



	--.SetWidgetHandle(Widgets.Image, Handles.Sprite, ||GetIcon(IconKeys.UI_Check).Image64)
	--.SetWidgetHandle(Widgets.Image, Handles.Color, function() if(_panel.Value())then return CastleYellow else return Gray end end)

_panel.CheckboxLayout.AddPanel(_p)
end
----------------------


----------------------
---Return-------------
return _panel
----------------------