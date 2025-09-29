-- Test Bricktron Names File for Castle Story
-- This file tests the Lua Editor's name editing capabilities

local bricktronNames = {
    -- Worker Names
    workers = {
        "Builder Bob",
        "Digger Dave",
        "Miner Mike",
        "Crafty Carl",
        "Handy Harry",
        "Tool Tom",
        "Work Will",
        "Labor Luke",
        "Task Tony",
        "Job Jack",
        "Build Bill",
        "Construct Chris",
        "Create Kate",
        "Make Mark",
        "Assemble Amy",
        "Fabricate Frank",
        "Manufacture Mary",
        "Produce Pete",
        "Develop Dan",
        "Design Diana"
    },
    
    -- Knight Names
    knights = {
        "Sir Brave",
        "Knight Kyle",
        "Guardian Greg",
        "Protector Paul",
        "Defender Dave",
        "Shield Sam",
        "Sword Steve",
        "Blade Ben",
        "Warrior Will",
        "Fighter Frank",
        "Champion Chris",
        "Hero Henry",
        "Valiant Victor",
        "Noble Nick",
        "Royal Ryan",
        "Elite Eric",
        "Veteran Vince",
        "Commander Carl",
        "Captain Kate",
        "General George"
    },
    
    -- Archer Names
    archers = {
        "Arrow Andy",
        "Bow Ben",
        "Shot Sam",
        "Aim Amy",
        "Target Tom",
        "Sniper Steve",
        "Hunter Harry",
        "Ranger Ryan",
        "Scout Scott",
        "Spotter Sarah",
        "Marksman Mike",
        "Sharpshooter Sam",
        "Eagle Eye Eric",
        "Hawk Harry",
        "Falcon Frank",
        "Raven Ryan",
        "Crow Chris",
        "Owl Oliver",
        "Vulture Vince",
        "Buzzard Bob"
    },
    
    -- Special Names
    special = {
        "Legend Larry",
        "Mythic Mike",
        "Epic Eric",
        "Legendary Luke",
        "Fabled Frank",
        "Famous Fred",
        "Renowned Ryan",
        "Celebrated Chris",
        "Honored Harry",
        "Distinguished Dan",
        "Esteemed Eric",
        "Prestigious Pete",
        "Illustrious Ian",
        "Glorious Greg",
        "Magnificent Mike",
        "Majestic Mary",
        "Regal Ryan",
        "Royal Rick",
        "Noble Nick",
        "Aristocratic Amy"
    },
    
    -- Funny Names
    funny = {
        "Silly Sam",
        "Goofy Greg",
        "Wacky Will",
        "Crazy Chris",
        "Zany Zack",
        "Loony Luke",
        "Nutty Nick",
        "Kooky Kate",
        "Wacky Wendy",
        "Silly Sarah",
        "Goofy Gary",
        "Funny Frank",
        "Hilarious Harry",
        "Comical Chris",
        "Amusing Amy",
        "Entertaining Eric",
        "Humorous Henry",
        "Jovial Jack",
        "Merry Mike",
        "Cheerful Charlie"
    },
    
    -- Fantasy Names
    fantasy = {
        "Aragorn",
        "Legolas",
        "Gimli",
        "Gandalf",
        "Frodo",
        "Samwise",
        "Merry",
        "Pippin",
        "Boromir",
        "Faramir",
        "Eowyn",
        "Arwen",
        "Galadriel",
        "Elrond",
        "Thranduil",
        "Bilbo",
        "Thorin",
        "Balin",
        "Dwalin",
        "Fili"
    },
    
    -- Medieval Names
    medieval = {
        "William",
        "Richard",
        "Henry",
        "Edward",
        "John",
        "Robert",
        "Thomas",
        "James",
        "Charles",
        "George",
        "Arthur",
        "Lancelot",
        "Percival",
        "Galahad",
        "Bors",
        "Gawain",
        "Tristan",
        "Isolde",
        "Guinevere",
        "Morgan"
    }
}

-- Name generation rules
local nameRules = {
    maxLength = 20,
    minLength = 3,
    allowNumbers = false,
    allowSpecialChars = false,
    caseSensitive = false,
    uniqueNames = true,
    randomize = true,
    prefixChance = 0.1,
    suffixChance = 0.1
}

-- Prefixes and suffixes
local prefixes = {
    "Sir",
    "Lady",
    "Master",
    "Mistress",
    "Captain",
    "Lieutenant",
    "Sergeant",
    "Private",
    "Commander",
    "General"
}

local suffixes = {
    "the Brave",
    "the Bold",
    "the Wise",
    "the Strong",
    "the Swift",
    "the Clever",
    "the Noble",
    "the Honorable",
    "the Valiant",
    "the Mighty"
}

return {
    names = bricktronNames,
    rules = nameRules,
    prefixes = prefixes,
    suffixes = suffixes
}
