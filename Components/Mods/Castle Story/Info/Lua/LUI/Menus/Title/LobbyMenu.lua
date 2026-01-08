require("LUI2.lua")
---------


---menu
local _menu =
	Menu:New()

_menu.CloseCurrentMenu = ||print("Missing function: CloseCurrentMenu")
---------


if(Network == nil) then
Network = {}
end
if(Network.player1 == nil) then
Network.player1 = {}
end
if(Network.player2 == nil) then
Network.player2 = {}
end
Network.player1.isHost = true
Network.player2.isHost = false


---------
---panels


----horizontal
do
local _p =
	LayoutPanels.Horizontal:New()
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||4)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(0,0,0,0))
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, || Alignment.UpperLeft)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true)
	.SetWidgetHandle(Widgets.Image, Handles.Color, ||Color.New(0,0,0,0))

_menu.AddPanel(_p)
_menu.lyt = _p
end


----vertical
do
local _p =
	LayoutPanels.Vertical:New()
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||4)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(0,0,0,0))
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, || Alignment.UpperLeft)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true)
	.SetWidgetHandle(Widgets.Image, Handles.Color, ||Color.New(0,0,0,0.7))

_menu.lyt.AddPanel(_p)
_menu.lyt.leftSide = _p
end


----lobby player bar host
do
local _p =
	dofile("LUI/Panels/LobbyPlayerBar.lua")
	.Set_Color(|| Color.New(0,0,0,1))
	.Set_Host(|| Network.player1.isHost)

_menu.lyt.leftSide.AddPanel(_p)
end


----lobby player bar host
do
local _p =
	dofile("LUI/Panels/LobbyPlayerBar.lua")
	.Set_Color(|| Color.New(0,0,0,1))
	.Set_Host(|| Network.player2.isHost)

_menu.lyt.leftSide.AddPanel(_p)
end


----spacer
do
local _p =
	dofile("LUI/Panels/SpacerPanel.lua")

_p.Width = ||-1
_p.Height = ||-1
_p.FlexibleHeight = ||1
_p.FlexibleWidth = ||1

_menu.lyt.leftSide.AddPanel(_p)
end


----chat field
do
local _p =
	dofile("LUI/Panels/ChatField.lua")
	--.Set_Height(|| 64)
	--.Set_Width(|| buttonWidth)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	--.Set_Label_Alignment(||Alignment.MiddleCenter)
	--.Set_Label_Text(||"Assign Key Binding")
	--.Set_Label_Font(|| Font.KorolevLight)
	.Set_Label_FontSize(|| 14)
	--.Set_Label_Color(||CastleYellow)
	--.Set_Color(|| Color.New(0,0,0,1))

_menu.lyt.leftSide.AddPanel(_p)
end


----vertical
do
local _p =
	LayoutPanels.Vertical:New()
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||400, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||410, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||4)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(0,0,0,0))
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, || Alignment.UpperLeft)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true)
	.SetWidgetHandle(Widgets.Image, Handles.Color, ||Color.New(0,0,0,0.7))

_menu.lyt.AddPanel(_p)
_menu.lyt.rightSide = _p
end


----vertical
do
local _p =
	LayoutPanels.Vertical:New()
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||400, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||410, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||4)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(0,0,0,0))
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, || Alignment.MiddleCenter)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||true)
	.SetWidgetHandle(Widgets.Image, Handles.Color, ||Color.New(0,0,0,0))

_menu.lyt.rightSide.AddPanel(_p)
_menu.lyt.rightSide.upperPanel = _p
end


----display double panel "map name"
do
local _p =
	dofile("LUI/Panels/LabeledPanel.lua")
	.Set_Height(|| 48)
	.Set_Width(|| 200)
	.Set_FlexibleWidth(|| -1)
	.Set_FlexibleHeight(|| -1)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	.Set_Label_Alignment(||Alignment.MiddleCenter)
	.Set_Label_Text(||"Orosi Valley")
	--.Set_Label_Font(|| Font.KorolevLight)
	.Set_Label_FontSize(|| 20)
	--.Set_Label_Color(||CastleYellow)
	.Set_Color(|| Color.New(0,0,0,0))

_menu.lyt.rightSide.upperPanel.AddPanel(_p)
end


----imaged button panel "map image"
do
local _p =
	dofile("LUI/Panels/ImagedButtonPanel.lua")
	.Set_Height(||200)
	.Set_Width(||220)
	.Set_FlexibleHeight(|| -1)
	.Set_FlexibleWidth(|| -1)
	.Set_Image_Width(||200)
	.Set_Image_Height(||200)
	.Set_Image_Sprite(||GetIcon(IconKeys.UI_Reset).Image64)
	.Set_Image_Color(|| Color.New(1,0,0,1))
	.Set_Alignment(||Alignment.MiddleCenter)
	.Set_Color(|| Color.New(0,0,0.8,1))

_menu.lyt.rightSide.upperPanel.AddPanel(_p)
end


----display double panel "map size"
do
local _p =
	dofile("LUI/Panels/DisplayDoublePanel.lua")
	--.Set_Height(|| 64)
	.Set_Width(|| 200)
	.Set_FlexibleWidth(|| -1)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	.Set_Label_Alignment(||Alignment.MiddleLeft)
	.Set_Label_Text(||"##mapmenu_size")
	.Set_Value_Text(||"Small")
	--.Set_Label_Font(|| Font.KorolevLight)
	--.Set_Label_FontSize(|| 20)
	--.Set_Label_Color(||CastleYellow)
	.Set_Color(|| Color.New(0,0,0,1))

_menu.lyt.rightSide.AddPanel(_p)
end


----display double panel "map mode"
do
local _p =
	dofile("LUI/Panels/DisplayDoublePanel.lua")
	--.Set_Height(|| 64)
	.Set_Width(|| 200)
	.Set_FlexibleWidth(|| -1)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	.Set_Label_Alignment(||Alignment.MiddleLeft)
	.Set_Label_Text(||"##mode")
	.Set_Value_Text(||"Conquest")
	--.Set_Label_Font(|| Font.KorolevLight)
	--.Set_Label_FontSize(|| 20)
	--.Set_Label_Color(||CastleYellow)
	.Set_Color(|| Color.New(0,0,0,1))

_menu.lyt.rightSide.AddPanel(_p)
end


----display double panel "map preset"
do
local _p =
	dofile("LUI/Panels/DisplayDoublePanel.lua")
	--.Set_Height(|| 64)
	.Set_Width(|| 200)
	.Set_FlexibleWidth(|| -1)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	.Set_Label_Alignment(||Alignment.MiddleLeft)
	.Set_Label_Text(||"##mode_preset")
	.Set_Value_Text(||"Versus")
	--.Set_Label_Font(|| Font.KorolevLight)
	--.Set_Label_FontSize(|| 20)
	--.Set_Label_Color(||CastleYellow)
	.Set_Color(|| Color.New(0,0,0,1))

_menu.lyt.rightSide.AddPanel(_p)
end


----display double panel "max players"
do
local _p =
	dofile("LUI/Panels/DisplayDoublePanel.lua")
	--.Set_Height(|| 64)
	.Set_Width(|| 200)
	.Set_FlexibleWidth(|| -1)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	.Set_Label_Alignment(||Alignment.MiddleLeft)
	.Set_Label_Text(||"##mapmenu_maxplayers")
	.Set_Value_Text(||"10")
	--.Set_Label_Font(|| Font.KorolevLight)
	--.Set_Label_FontSize(|| 20)
	--.Set_Label_Color(||CastleYellow)
	.Set_Color(|| Color.New(0,0,0,1))

_menu.lyt.rightSide.AddPanel(_p)
end

----labeled button "Start Game"
do
local _p =
	dofile("LUI/Panels/LabeledButtonPanel.lua")
	.Set_Height(|| 48)
	.Set_Width(|| 404)
	.Set_FlexibleWidth(|| -1)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	.Set_Label_Alignment(||Alignment.MiddleLeft)
	.Set_Label_Text(||"##mainmenu_multiplayer_startgame")
	.Set_Label_Font(|| Font.ProximaNovaRegular)
	.Set_Label_FontSize(|| 20)
	.Set_Label_Color(||CastleYellow)
	.Set_Color(|| Color.New(0,0,0,1))

_menu.lyt.rightSide.AddPanel(_p)
end


----labeled button "Leave"
do
local _p =
	dofile("LUI/Panels/LabeledButtonPanel.lua")
	.Set_Height(|| 48)
	.Set_Width(|| 404)
	.Set_FlexibleWidth(|| -1)
	--.Set_Padding(||Vector4.New(0,0,0,0))
	.Set_Label_Alignment(||Alignment.MiddleLeft)
	.Set_Label_Text(||"##mainmenu_multiplayer_leave")
	.Set_Label_Font(|| Font.ProximaNovaRegular)
	.Set_Label_FontSize(|| 20)
	.Set_Label_Color(||CastleYellow)
	.Set_Color(|| Color.New(0,0,0,1))
	.Set_Button_OnAction(|| _menu.CloseCurrentMenu())
	.Set_Button_Interactable(||true)

_menu.lyt.rightSide.AddPanel(_p)
end


---------
---flags
_menu.SetFlag(MenuFlag.OpenOnLoad)
_menu.SetFlag(MenuFlag.ReopenOnReload)
_menu.SetFlag(MenuFlag.OpensChildMenus)
---------


---------
---return
return _menu