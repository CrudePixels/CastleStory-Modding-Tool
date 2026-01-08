
Spawner = {}

function Spawner.At(key, position, faction, occupation)
	local newguy = key:New(faction, position)
	if occupation ~= nil then
		newguy:SetOccupation(occupation, true)
	end
	return newguy
end

function Spawner.Around(key, position, faction, occupation)
	return Spawner.At(key, Random.VoxelAround(position), faction, occupation)
end