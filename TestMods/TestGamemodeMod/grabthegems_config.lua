-- Grab the Gems Gamemode Configuration
-- This file demonstrates the enhanced grab the gems gamemode editor

sv_Settings =
{
	startingWorkersCount = 12,
	bricktronCap = 12,
	playerRelations = 2, --0 == allied, 1 == neutral, 2 == enemy
	startingTimeOfDay = 7,
	daytimeFactor = 1.4,
	nighttimeFactor = 0.6,
	pauseTimeOfDay = false,
	moonlight = nil,
	ambientColor = nil,
	pumpkinSpawnTime = 480,
	firstPumpkinSpawnTime = 720,
	pumpkinsToWin = 10,
	pumpkinsPerWave = 5,
}

Registry =
{
	gameTimer = Timer.New(0),
	gameTimerOffset = 0,
	gemSpawnTimer = Timer.New(0),
	gemSpawnTimerDelay = 0,
}

Characters = {
	Bricktron = {
		Ref = fy_Bricktron,
		Cost = 1
	},
	Corruptron = {
		Ref = fy_Corruptron,
        Occupation = Occupations.Corruptron,
		Cost = 3
	},
	Biftron = {
		Ref = fy_Biftron,
        Occupation = Occupations.Biftron,
		Cost = 8
	},
	Minitron = {
		Ref = fy_Minitron,
        Occupation = Occupations.Minitron,
		Cost = 2
	},
	Magitron = {
		Ref = fy_Magitron,
        Occupation = Occupations.Magitron,
		Cost = 15
	}
}
