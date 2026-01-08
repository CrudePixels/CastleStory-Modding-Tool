function DoShowIntroDialog()

	local t = {}
	
	t.rawimg_header = {}
	t.rawimg_header.Tex = ||UIGame.GetModeIntroTexture()

	t.lbl_title = {}
	t.lbl_title.Text = ||GetLocalized(UIGame.GetModeName()):upper()

	t.lbl_subtitle = {}
	t.lbl_subtitle.Text = ||GetLocalized(UIGame.GetMapName(false)):upper()

	t.lbl_content = {}
	t.lbl_content.Text = ||GetLocalized("##mode_conquer_intro")

	t.btn_left = {}
	t.btn_left.Visible = ||true
	t.btn_left.OnAction = function() if not IsMultiplayer() then StartTimers() end Hooks.Invoke(UIGame.hk_CloseModalMenu) end
	t.btn_left.lbl_top = {}
	t.btn_left.lbl_top.Text = ||GetLocalized("##mode_conquer_intro_start"):upper()
	t.btn_left.lbl_bot = {}
	t.btn_left.lbl_bot.Text = ||GetLocalized("##mode_conquer_intro_start_info")

	t.btn_right = {}
	t.btn_right.Visible = ||false

	Hooks.Invoke(UIGame.hk_OpenModalMenu, {t})

	--[[
    local OnOk = function() if not IsMultiplayer() then StartTimers() end end

	--DisplayOkDialog(
		"##gamemenu_start_conquest",--titleText
		"##gamemenu_start_conquest_text",--contentText
		"##gamemenu_start_ready",--okText
		function() OnOk() end--okClosure
	)
	--]]
end

function ShowIntroDialog()
	Hooks.LateConnectOnce(GS_Modal.hk_OnLoaded, DoShowIntroDialog)
end

function ShowVictoryDialog()

    local F_onQuit =
		function()
			Hooks.Invoke(UIGame.hk_CloseModalMenu)
			Hooks.Invoke(hk_CallEndGame)
		end

	local t = {}
	
	t.rawimg_header = {}
	t.rawimg_header.Tex = ||UIGame.GetModeIntroTexture()

	t.lbl_title = {}
	t.lbl_title.Text = ||GetLocalized("##mode_conquer_victory"):upper()

	t.lbl_subtitle = {}
	t.lbl_subtitle.Text = ||GetLocalized(UIGame.GetModeName()):upper() .. " - " .. GetLocalized(UIGame.GetPresetName()):upper()

	t.lbl_content = {}
	t.lbl_content.Text = ||GetLocalized("##mode_conquer_victory_info")

	t.btn_left = {}
	t.btn_left.Visible = ||false

	t.btn_right = {}
	t.btn_right.Visible = ||true
	t.btn_right.OnAction = ||F_onQuit()
	t.btn_right.lbl_top = {}
	t.btn_right.lbl_top.Text = ||GetLocalized("##mode_conquer_victory_quit"):upper()
	t.btn_right.lbl_bot = {}
	t.btn_right.lbl_bot.Text = ||GetLocalized("##mode_conquer_victory_quit_info")

	Hooks.Invoke(UIGame.hk_OpenModalMenu, {t})

	--[[
      --DisplayOkDialog("##gamemenu_victory_conquest", "##gamemenu_victory_conquest_text", "##gamemenu_returntotitle", function() Hooks.Invoke(hk_CallEndGame) end)
	--]]
end

function ShowDefeatDialog(bool_allowSpectate)

    local F_onQuit =
		function()
			Hooks.Invoke(UIGame.hk_CloseModalMenu)
			Hooks.Invoke(hk_CallEndGame)
		end

    local F_onContinue =
		function()
			Hooks.Invoke(UIGame.hk_CloseModalMenu)
		end

	local t = {}
	
	t.rawimg_header = {}
	t.rawimg_header.Tex = ||UIGame.GetModeIntroTexture()

	t.lbl_title = {}
	t.lbl_title.Text = ||GetLocalized("##mode_conquer_defeat"):upper()

	t.lbl_subtitle = {}
	t.lbl_subtitle.Text = ||GetLocalized(UIGame.GetModeName()):upper() .. " - " .. GetLocalized(UIGame.GetPresetName()):upper()

	t.lbl_content = {}
	t.lbl_content.Text = ||GetLocalized("##mode_conquer_defeat_info")

	t.btn_left = {}
	t.btn_left.Visible = ||bool_allowSpectate
	t.btn_left.OnAction = ||F_onContinue()
	t.btn_left.lbl_top = {}
	t.btn_left.lbl_top.Text = ||GetLocalized("##mode_conquer_defeat_continue"):upper()
	t.btn_left.lbl_bot = {}
	t.btn_left.lbl_bot.Text = ||GetLocalized("##mode_conquer_defeat_continue_info")

	t.btn_right = {}
	t.btn_right.Visible = ||true
	t.btn_right.OnAction = ||F_onQuit()
	t.btn_right.lbl_top = {}
	t.btn_right.lbl_top.Text = ||GetLocalized("##mode_conquer_defeat_quit"):upper()
	t.btn_right.lbl_bot = {}
	t.btn_right.lbl_bot.Text = ||GetLocalized("##mode_conquer_defeat_quit_info")

	Hooks.Invoke(UIGame.hk_OpenModalMenu, {t})

	--[[
      if IsMultiplayer() then
        --DisplayDialog("##gamemenu_gameover_title", gameoverMessage, {"##gamemenu_returntotitle", "##gamemenu_continue_spectator"}, {function() Hooks.Invoke(hk_CallEndGame) end, function() end})
      else
        --DisplayOkDialog("##gamemenu_gameover_title", gameoverMessage, "##gamemenu_returntotitle", function() Hooks.Invoke(hk_CallEndGame) end)
      end
	--]]
end


function LoadScoreboardMenu()
	local m = dofile("Gamemodes/conquer/Menu/ConquerMenu.lua")
	m.Begin()
	return m
end


function LoadModalMenu()
	return dofile("LUI/Menus/Game/Menu_Modal.lua")
end

function LoadAllMenus()

	UIGame.SetQueueRefreshOptimization(true)
	UIGame.SetQueueTreeRefreshOptimization(true)

	GS_Scoreboard:LoadMenuFromClosure(LoadScoreboardMenu)
	GS_Scoreboard:SetVisible(true)

	GS_Modal:LoadMenuFromClosure(LoadModalMenu)
	GS_Modal:SetVisible(true)

end

do
function LoadTaskbarMenu()
	if GS_Taskbar.loaded then return end
	GS_Taskbar:LoadMenu()
end
Hooks.Connect(hk_OnLoadUI, LoadTaskbarMenu)
end

do
function UnloadAllMenus()
	GS_Scoreboard:UnloadMenu()
	GS_Modal:UnloadMenu()
	GS_Taskbar:UnloadMenu()

	UIGame.SetQueueRefreshOptimization(false)
	UIGame.SetQueueTreeRefreshOptimization(false)

	ResetClockSpeed()
end
Hooks.Connect(hk_OnLeaveGame, UnloadAllMenus)
end