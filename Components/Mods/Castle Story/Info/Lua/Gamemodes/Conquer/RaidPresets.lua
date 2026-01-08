
RaidPresets = 
{
	Level1 = 
	{
		Raid1 = 
		{
			{2, Characters.Minitron}
		},
		Raid2 = 
		{
			{2, Characters.Minitron}
		},
		Raid3 = 
		{
			{1, Characters.Corruptron}
		},
		Raid4 = 
		{
			{1, Characters.Corruptron}
		},
		Raid5 = 
		{
			{2, Characters.Corruptron}
		},
		Raid6 = 
		{
			{1, Characters.Biftron}
		},
		Raid7 = 
		{
			{1, Characters.Magitron}
		}
	},

  GetAtLevel = function(desiredLevel)
    local affordable = {}
    local affordableName = {}
    local currentLevel
    local levelName

    if desiredLevel == 1 then  currentLevel = RaidPresets.Level1 ; levelName = "Level1"
    elseif desiredLevel == 2 then  currentLevel = RaidPresets.Level2 ; levelName = "Level2"
    elseif desiredLevel == 3 then  currentLevel = RaidPresets.Level3 ; levelName = "Level3"
    else currentLevel = RaidPresets.Level4 ; levelName = "Level4"
    end

    for i, set in pairs(currentLevel) do
      local cost = 0
      for j, pair in pairs(set) do
        cost = cost + (pair[1] * pair[2].Cost)
      end
      if true then
        table.insert(affordable, set)
        table.insert(affordableName, i)
      end
    end
      
    if #affordable == 0 then
      return nil, nil, nil
    end

    local chosenNumber = math.random(#affordable)
    return affordable[ chosenNumber], levelName, affordableName[ chosenNumber]

  end,

	GetUnderBudget = function(budget)
		local affordable = {}
		local affordableName = {}
		local affordableCost = {}
		local currentLevel = RaidPresets.Level1
		local levelName = "Level1"

		for i, set in pairs(currentLevel) do
			local cost = 0
			for j, pair in pairs(set) do
				cost = cost + (pair[1] * pair[2].Cost)
			end
			if cost <= budget then
				table.insert(affordable, set)
				table.insert(affordableName, i)
				table.insert(affordableCost, cost)
			end
		end
    
		if #affordable == 0 then
			return nil
		end

		local chosenNumber = math.random(#affordable)
		return affordable[chosenNumber], levelName, affordableName[chosenNumber], affordableCost[chosenNumber]
    
	end
}
