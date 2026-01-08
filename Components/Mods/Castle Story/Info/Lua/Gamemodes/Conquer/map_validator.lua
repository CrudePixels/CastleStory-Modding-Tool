local _results = { true }
local _TryInvalidate = function(_flag) if(not(_flag))then _results[1] = false end end

do
	local _name = "Player Starting Point"
	local _required = 2
	local _current = #Markers.GetGroup2(_name)
	local _flag = _current >= _required

	_TryInvalidate(_flag)
	table.insert(_results, {_flag, string.format("({0}/{1}) {2}", _current, _required, _name)})
end

return _results