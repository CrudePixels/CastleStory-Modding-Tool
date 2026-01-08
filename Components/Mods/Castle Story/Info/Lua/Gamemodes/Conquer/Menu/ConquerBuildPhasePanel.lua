require("LUI2.lua")


---panel
local _panel =
	LayoutPanels.Horizontal:New()

	
---build phase?
local _VisiblePanel =
	function()
		if(not(_panel.data))then return false end
		if(_panel.data.lv_hasCorrupteds:GetValue())then return false end
		return not(_panel.data.lv_buildPhaseDone:GetValue())
	end

---panel CTD
_panel
	.SetWidgetHandle(Widgets.LayoutElement, Handles.Visible, ||_VisiblePanel())
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.MinHeight, ||48, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredWidth, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.PreferredHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleWidth, ||1, true)
	.SetWidgetHandle(Widgets.LayoutElement, Handles.FlexibleHeight, ||-1, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Spacing, ||0, true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Padding, ||Vector4.New(6,6,6,6), true)
	.SetWidgetHandle(Widgets.LayoutGroup, Handles.Alignment, ||Alignment.MiddleRight, true)
	.SetWidgetHandle(Widgets.Image, Handles.Enabled, ||false, true)
	

---timer panel
_panel.AddTimerPanel =
	function()
	
		local _TimeRemaining =
			function()
				return _panel.data.lv_buildPhaseRemainingTime:GetValue()
			end
		
		---name
		do
		local _p =
			Panels.Label:New()
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.AutoMinWidth, ||true, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.MinHeight, ||24, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.Pivot, ||Vector2.New(0,0.5), true)
			.SetWidgetHandle(Widgets.Label, Handles.FontSize, ||14, true)
			.SetWidgetHandle(Widgets.Label, Handles.Alignment, ||Alignment.LowerLeft, true)
			.SetWidgetHandle(Widgets.Label, Handles.Color, ||Color.New(1,1,1,1), true)
			.SetWidgetHandle(Widgets.Label, Handles.Text, ||"Build Phase")
	
			---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close

		_panel.AddPanel(_p)
		_panel.nameLbl = _p
		end

		---timer
		do
		local _p =
			Panels.Label:New()
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.MinWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.MinHeight, ||24, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredWidth, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.PreferredHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleWidth, ||1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.FlexibleHeight, ||-1, true)
			.SetWidgetHandle(Widgets.LabelLayoutElement, Handles.Pivot, ||Vector2.New(0,0.5), true)
			.SetWidgetHandle(Widgets.Label, Handles.FontSize, ||14, true)
			.SetWidgetHandle(Widgets.Label, Handles.Alignment, ||Alignment.LowerRight, true)
			.SetWidgetHandle(Widgets.Label, Handles.Color, ||Color.New(1,1,1,1), true)
			.SetWidgetHandle(Widgets.Label, Handles.Text, ||_TimeRemaining())
	
			---TODO: revise below to tr_onSlowUpdate:RemoveCallback on _panel.Close
			_panel.data.tr_onSlowUpdate:AddCallback(function() if(_p)then _p.Refresh() end end)

		_panel.AddPanel(_p)
		_panel.timerLbl = _p
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
_panel.Build =
	function()
		_panel.AddTimerPanel()
		return _panel
	end




----------------------
---return
return _panel