-- Test Castle Story Gamemode Configuration
-- This file tests the Lua Editor's ability to parse and edit game settings

--These all get overritten by presets if those are loaded with the map
--If you want to change values, change them there and/or make your own preset
sv_Settings = {
	--Raid Management
	playerAttackInterval = 600,
	firstWaveDurationBonus = 300,
	maximumEnemyLevel = 10,
	initialEnemyLevel = 0,
	levelClockInterval = 480,
	neutralAttackInterval = 60,
	startingCorruptCrystals = 2,
	forcePlayerFirefliesToPlayerCrystal = true,
  corruptronCap = 50,
  baseCorruptronOffense = 6,
  baseCorruptronDefense = 6,
  offenseIncreasePerLevel = 3,
  defenseIncreasePerLevel = 3,
  randomCorruptronCapture = false,
	--Resources
	bricktronCap = 100,
	startingWorkersCount = 10,
	startingKnightCount = 2,
	startingArcherCount = 2,
	--Global Settings
  fireflyCostMultiplier = 0.2, --0.5
	canDigGround = true,
	playerRelations = 2, --0 == allied, 1 == neutral, 2 == enemy
	--Time of Day
	startingTimeOfDay = 7,
	daynightCycleSetting = 0, --0 == daytime/nightime, 1 == only daytime, 2 == only nighttime
	daytimeFactor = 1.4,
	nighttimeFactor = 0.6,
  pauseTimeOfDay = false,
  moonlight = nil,
  ambientColor = nil
  --Before using presets to override this config file, make sur they are properly networked. Lights out related settings do work properly though
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
		Cost = 12
	},
	Minitron = {
		Ref = fy_Minitron,
    Occupation = Occupations.Minitron,
		Cost = 1.5
	},
	Magitron = {
		Ref = fy_Magitron,
    Occupation = Occupations.Magitron,
		Cost = 18
	}
}

Registry = {
  currentLevel = sv_Settings.initialEnemyLevel,
  currentFibonacci = 1, --double check usage and remove
  previousFibonacci = 0, --double check usage and remove
  currentExperiencePoints = 0, --double check usage and remove
  timers = {
    timeForPlayerAttack = nil,
    timeBetweenLevels = nil,
    timeForNeutralAttack = nil
  }
}

InterestPoints = {}
