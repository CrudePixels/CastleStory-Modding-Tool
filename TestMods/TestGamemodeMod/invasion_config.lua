-- Invasion Gamemode Configuration
-- This file demonstrates the enhanced invasion gamemode editor

sv_Settings = 
{
	--Wave Intervals
	displayTimeBeforeWave = true,
	firstWaveDurationBonus = 240.0,
	waveDuration = 480.0,
	--Wave Difficulty
	firstWaveBudget = 2,
	additionalWaveBudget = 2,
	waveBudgetMultiplier = 1,
	waveBudgetRandom = 0,
	waveLevelUpCount = 20,
	maximumCorruptronLevel = 3,
	victoryWaveNumber = 61,
	--Wave Trigger Condition
	canManualTriggerWaves = true,
	--Resources
	bricktronCap = 15,
	startingWorkersCount = 3,
	startingKnightCount = 2,
	startingArcherCount = 1,
	--Global Settings
	canDigGround = false,
	--Time of Day
	startingTimeOfDay = 7,
	daynightCycleSetting = 0, --0 == daytime/nightime, 1 == only daytime, 2 == only nighttime
	daytimeFactor = 1.4,
	nighttimeFactor = 0.6,
	timeToFireflyAbsorption = 60.0,
    pauseTimeOfDay = false,
    moonlight = nil,
    ambientColor = nil
}

Registry = 
{
	waveNumber = 1, 
	waveSurvived = 0,
	waveTimer = nil,
	deadCorruptronCount = 0,
	waveBudget = 0,
	CorruptronCharacterLevel = 1;
}

Characters = {
	Bricktron = {
		Ref = fy_Bricktron,
		Cost = 1
	},
	Corruptron = {
		Ref = fy_Corruptron,
        Occupation = Occupations.Corruptron,
		Cost = 5
	},
	Biftron = {
		Ref = fy_Biftron,
        Occupation = Occupations.Biftron,
		Cost = 20
	},
	Minitron = {
		Ref = fy_Minitron,
        Occupation = Occupations.Minitron,
		Cost = 3
	},
	Magitron = {
		Ref = fy_Magitron,
        Occupation = Occupations.Magitron,
		Cost = 30
	}
}

CorruptedFaction = nil
