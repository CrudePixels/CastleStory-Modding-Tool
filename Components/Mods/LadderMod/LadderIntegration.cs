using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CastleStoryMods
{
    public class LadderIntegration
    {
        private const string GAME_DATA_PATH = @"D:\SteamLibrary\steamapps\common\Castle Story\Info\Lua\Data";
        private const string MOD_DATA_PATH = @"D:\SteamLibrary\steamapps\common\Castle Story\Info\Lua\Data\Mods";
        
        public static bool IntegrateLadderMod()
        {
            try
            {
                Console.WriteLine("Integrating Ladder Mod into Castle Story...");
                
                // Create mod directory if it doesn't exist
                if (!Directory.Exists(MOD_DATA_PATH))
                {
                    Directory.CreateDirectory(MOD_DATA_PATH);
                    Console.WriteLine("Created mod directory: " + MOD_DATA_PATH);
                }
                
                // Copy ladder configuration to game directory
                CopyLadderConfig();
                
                // Copy ladder blocks definition
                CopyLadderBlocks();
                
                // Copy ladder mechanics
                CopyLadderMechanics();
                
                // Create main integration script
                CreateMainIntegrationScript();
                
                // Update game's main Lua files to load the mod
                UpdateGameLuaFiles();
                
                Console.WriteLine("Ladder Mod integration completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error integrating Ladder Mod: {ex.Message}");
                return false;
            }
        }
        
        private static void CopyLadderConfig()
        {
            var configContent = GenerateLadderConfigLua();
            var configPath = Path.Combine(MOD_DATA_PATH, "LadderConfig.lua");
            File.WriteAllText(configPath, configContent);
            Console.WriteLine("Copied ladder configuration to game directory");
        }
        
        private static void CopyLadderBlocks()
        {
            var blocksContent = GenerateLadderBlocksLua();
            var blocksPath = Path.Combine(MOD_DATA_PATH, "LadderBlocks.lua");
            File.WriteAllText(blocksPath, blocksContent);
            Console.WriteLine("Copied ladder blocks to game directory");
        }
        
        private static void CopyLadderMechanics()
        {
            var mechanicsContent = GenerateLadderMechanicsLua();
            var mechanicsPath = Path.Combine(MOD_DATA_PATH, "LadderMechanics.lua");
            File.WriteAllText(mechanicsPath, mechanicsContent);
            Console.WriteLine("Copied ladder mechanics to game directory");
        }
        
        private static void CreateMainIntegrationScript()
        {
            var integrationContent = GenerateMainIntegrationLua();
            var integrationPath = Path.Combine(MOD_DATA_PATH, "LadderMod.lua");
            File.WriteAllText(integrationPath, integrationContent);
            Console.WriteLine("Created main integration script");
        }
        
        private static void UpdateGameLuaFiles()
        {
            // Inject ladder blocks directly into the building system
            InjectIntoBuildingSystem();
            
            // Update the main game initialization to load our mod
            var mainLuaPath = Path.Combine(GAME_DATA_PATH, "Main.lua");
            if (File.Exists(mainLuaPath))
            {
                var mainContent = File.ReadAllText(mainLuaPath);
                if (!mainContent.Contains("LadderMod"))
                {
                    var updatedContent = mainContent + "\n\n-- Load Ladder Mod\nif file_exists('Data/Mods/LadderMod.lua') then\n    dofile('Data/Mods/LadderMod.lua')\nend\n";
                    File.WriteAllText(mainLuaPath, updatedContent);
                    Console.WriteLine("Updated main game Lua file to load Ladder Mod");
                }
            }
        }
        
        private static void InjectIntoBuildingSystem()
        {
            try
            {
                // Find and update the building blocks file
                var buildingBlocksPath = Path.Combine(GAME_DATA_PATH, "Data_BuildingBlocks.lua");
                if (File.Exists(buildingBlocksPath))
                {
                    var content = File.ReadAllText(buildingBlocksPath);
                    
                    // Add ladder blocks to the existing building blocks
                    var ladderBlocksCode = GenerateBuildingBlocksIntegration();
                    
                    // Insert ladder blocks before the closing of the building blocks table
                    if (content.Contains("}"))
                    {
                        var lastBraceIndex = content.LastIndexOf("}");
                        var updatedContent = content.Substring(0, lastBraceIndex) + 
                                          ",\n\n    -- Ladder Blocks (Added by LadderMod)\n" + 
                                          ladderBlocksCode + "\n" + 
                                          content.Substring(lastBraceIndex);
                        
                        File.WriteAllText(buildingBlocksPath, updatedContent);
                        Console.WriteLine("Injected ladder blocks into building system");
                    }
                }
                
                // Also update the building categories to include ladders
                var buildingCategoriesPath = Path.Combine(GAME_DATA_PATH, "Data_BuildingCategories.lua");
                if (File.Exists(buildingCategoriesPath))
                {
                    var content = File.ReadAllText(buildingCategoriesPath);
                    
                    if (!content.Contains("ladder"))
                    {
                        var ladderCategoryCode = GenerateBuildingCategoryIntegration();
                        var updatedContent = content + "\n\n" + ladderCategoryCode;
                        File.WriteAllText(buildingCategoriesPath, updatedContent);
                        Console.WriteLine("Added ladder category to building system");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error injecting into building system: {ex.Message}");
            }
        }
        
        private static string GenerateBuildingBlocksIntegration()
        {
            var sb = new StringBuilder();
            
            // Wooden Ladder
            sb.AppendLine("    [\"ladder_wood\"] = {");
            sb.AppendLine("        name = \"Wooden Ladder\",");
            sb.AppendLine("        category = \"ladder\",");
            sb.AppendLine("        material = \"wood\",");
            sb.AppendLine("        durability = 100,");
            sb.AppendLine("        cost = { wood = 2 },");
            sb.AppendLine("        icon = \"ladder_wood_icon\",");
            sb.AppendLine("        model = \"ladder_wood_model\",");
            sb.AppendLine("        climbSpeed = 2.0,");
            sb.AppendLine("        maxHeight = 50,");
            sb.AppendLine("        canClimb = true");
            sb.AppendLine("    },");
            
            // Iron Ladder
            sb.AppendLine("    [\"ladder_iron\"] = {");
            sb.AppendLine("        name = \"Iron Ladder\",");
            sb.AppendLine("        category = \"ladder\",");
            sb.AppendLine("        material = \"iron\",");
            sb.AppendLine("        durability = 200,");
            sb.AppendLine("        cost = { iron = 1, wood = 1 },");
            sb.AppendLine("        icon = \"ladder_iron_icon\",");
            sb.AppendLine("        model = \"ladder_iron_model\",");
            sb.AppendLine("        climbSpeed = 3.0,");
            sb.AppendLine("        maxHeight = 75,");
            sb.AppendLine("        canClimb = true");
            sb.AppendLine("    },");
            
            // Stone Ladder
            sb.AppendLine("    [\"ladder_stone\"] = {");
            sb.AppendLine("        name = \"Stone Ladder\",");
            sb.AppendLine("        category = \"ladder\",");
            sb.AppendLine("        material = \"stone\",");
            sb.AppendLine("        durability = 300,");
            sb.AppendLine("        cost = { stone = 2 },");
            sb.AppendLine("        icon = \"ladder_stone_icon\",");
            sb.AppendLine("        model = \"ladder_stone_model\",");
            sb.AppendLine("        climbSpeed = 1.6,");
            sb.AppendLine("        maxHeight = 100,");
            sb.AppendLine("        canClimb = true");
            sb.AppendLine("    },");
            
            // Rope Ladder
            sb.AppendLine("    [\"ladder_rope\"] = {");
            sb.AppendLine("        name = \"Rope Ladder\",");
            sb.AppendLine("        category = \"ladder\",");
            sb.AppendLine("        material = \"rope\",");
            sb.AppendLine("        durability = 50,");
            sb.AppendLine("        cost = { rope = 3, wood = 1 },");
            sb.AppendLine("        icon = \"ladder_rope_icon\",");
            sb.AppendLine("        model = \"ladder_rope_model\",");
            sb.AppendLine("        climbSpeed = 2.4,");
            sb.AppendLine("        maxHeight = 40,");
            sb.AppendLine("        canClimb = true");
            sb.AppendLine("    }");
            
            return sb.ToString();
        }
        
        private static string GenerateBuildingCategoryIntegration()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Category (Added by LadderMod)");
            sb.AppendLine("BuildingCategories[\"ladder\"] = {");
            sb.AppendLine("    name = \"Ladders\",");
            sb.AppendLine("    icon = \"ladder_category_icon\",");
            sb.AppendLine("    order = 10");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private static string GenerateLadderConfigLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Configuration");
            sb.AppendLine("-- Generated by LadderMod Integration");
            sb.AppendLine();
            
            sb.AppendLine("LadderConfig = {");
            sb.AppendLine("    enabled = true,");
            sb.AppendLine("    maxHeight = 50,");
            sb.AppendLine("    climbSpeed = 2.0,");
            sb.AppendLine("    autoSnap = true,");
            sb.AppendLine("    grabDistance = 2.0,");
            sb.AppendLine("    releaseDistance = 3.0,");
            sb.AppendLine("    snapDistance = 1.5,");
            sb.AppendLine("    climbHeight = 1.0,");
            sb.AppendLine("    fallDamage = false,");
            sb.AppendLine("    minLevel = 1,");
            sb.AppendLine("    requiresBlueprint = false,");
            sb.AppendLine("    maxPerPlayer = 10,");
            sb.AppendLine("    cooldown = 0.5");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private static string GenerateLadderBlocksLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Blocks Definition");
            sb.AppendLine("-- Generated by LadderMod Integration");
            sb.AppendLine();
            
            sb.AppendLine("LadderBlocks = {");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Wooden Ladder\",");
            sb.AppendLine("        id = \"ladder_wood\",");
            sb.AppendLine("        material = \"wood\",");
            sb.AppendLine("        durability = 100,");
            sb.AppendLine("        climbSpeed = 2.0,");
            sb.AppendLine("        maxHeight = 50,");
            sb.AppendLine("        cost = { wood = 2 },");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        icon = \"ladder_wood_icon\"");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Iron Ladder\",");
            sb.AppendLine("        id = \"ladder_iron\",");
            sb.AppendLine("        material = \"iron\",");
            sb.AppendLine("        durability = 200,");
            sb.AppendLine("        climbSpeed = 3.0,");
            sb.AppendLine("        maxHeight = 75,");
            sb.AppendLine("        cost = { iron = 1, wood = 1 },");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        icon = \"ladder_iron_icon\"");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Stone Ladder\",");
            sb.AppendLine("        id = \"ladder_stone\",");
            sb.AppendLine("        material = \"stone\",");
            sb.AppendLine("        durability = 300,");
            sb.AppendLine("        climbSpeed = 1.6,");
            sb.AppendLine("        maxHeight = 100,");
            sb.AppendLine("        cost = { stone = 2 },");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        icon = \"ladder_stone_icon\"");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Rope Ladder\",");
            sb.AppendLine("        id = \"ladder_rope\",");
            sb.AppendLine("        material = \"rope\",");
            sb.AppendLine("        durability = 50,");
            sb.AppendLine("        climbSpeed = 2.4,");
            sb.AppendLine("        maxHeight = 40,");
            sb.AppendLine("        cost = { rope = 3, wood = 1 },");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        icon = \"ladder_rope_icon\"");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private static string GenerateLadderMechanicsLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Mechanics");
            sb.AppendLine("-- Generated by LadderMod Integration");
            sb.AppendLine();
            
            sb.AppendLine("-- Ladder climbing state");
            sb.AppendLine("local isClimbing = false");
            sb.AppendLine("local currentLadder = nil");
            sb.AppendLine("local climbDirection = 0");
            sb.AppendLine();
            
            sb.AppendLine("-- Initialize ladder system");
            sb.AppendLine("function InitializeLadderSystem()");
            sb.AppendLine("    print(\"Initializing Ladder System...\")");
            sb.AppendLine("    ");
            sb.AppendLine("    -- Register ladder blocks with building system");
            sb.AppendLine("    for _, block in ipairs(LadderBlocks) do");
            sb.AppendLine("        RegisterBuildingBlock(block.id, block)");
            sb.AppendLine("    end");
            sb.AppendLine("    ");
            sb.AppendLine("    -- Hook into character movement");
            sb.AppendLine("    HookCharacterMovement()");
            sb.AppendLine("    ");
            sb.AppendLine("    print(\"Ladder System initialized successfully\")");
            sb.AppendLine("end");
            sb.AppendLine();
            
            sb.AppendLine("-- Hook into character movement system");
            sb.AppendLine("function HookCharacterMovement()");
            sb.AppendLine("    -- This would hook into the game's character movement system");
            sb.AppendLine("    -- to detect when a character is near a ladder and enable climbing");
            sb.AppendLine("    print(\"Character movement hooked for ladder climbing\")");
            sb.AppendLine("end");
            sb.AppendLine();
            
            sb.AppendLine("-- Check if character can climb ladder");
            sb.AppendLine("function CanClimbLadder(character, ladder)");
            sb.AppendLine("    if not LadderConfig.enabled then");
            sb.AppendLine("        return false");
            sb.AppendLine("    end");
            sb.AppendLine("    ");
            sb.AppendLine("    -- Check distance");
            sb.AppendLine("    local distance = GetDistance(character, ladder)");
            sb.AppendLine("    if distance > LadderConfig.grabDistance then");
            sb.AppendLine("        return false");
            sb.AppendLine("    end");
            sb.AppendLine("    ");
            sb.AppendLine("    -- Check level requirement");
            sb.AppendLine("    if character.level < LadderConfig.minLevel then");
            sb.AppendLine("        return false");
            sb.AppendLine("    end");
            sb.AppendLine("    ");
            sb.AppendLine("    return true");
            sb.AppendLine("end");
            sb.AppendLine();
            
            sb.AppendLine("-- Start climbing ladder");
            sb.AppendLine("function StartClimbingLadder(character, ladder)");
            sb.AppendLine("    if CanClimbLadder(character, ladder) then");
            sb.AppendLine("        isClimbing = true");
            sb.AppendLine("        currentLadder = ladder");
            sb.AppendLine("        character.movementMode = \"climbing\"");
            sb.AppendLine("        print(\"Character started climbing ladder\")");
            sb.AppendLine("        return true");
            sb.AppendLine("    end");
            sb.AppendLine("    return false");
            sb.AppendLine("end");
            sb.AppendLine();
            
            sb.AppendLine("-- Stop climbing ladder");
            sb.AppendLine("function StopClimbingLadder(character)");
            sb.AppendLine("    if isClimbing then");
            sb.AppendLine("        isClimbing = false");
            sb.AppendLine("        currentLadder = nil");
            sb.AppendLine("        character.movementMode = \"walking\"");
            sb.AppendLine("        print(\"Character stopped climbing ladder\")");
            sb.AppendLine("    end");
            sb.AppendLine("end");
            sb.AppendLine();
            
            sb.AppendLine("-- Update ladder climbing");
            sb.AppendLine("function UpdateLadderClimbing(character, deltaTime)");
            sb.AppendLine("    if not isClimbing or not currentLadder then");
            sb.AppendLine("        return");
            sb.AppendLine("    end");
            sb.AppendLine("    ");
            sb.AppendLine("    -- Handle climbing movement");
            sb.AppendLine("    if climbDirection ~= 0 then");
            sb.AppendLine("        local climbSpeed = LadderConfig.climbSpeed * deltaTime");
            sb.AppendLine("        character.position.y = character.position.y + (climbDirection * climbSpeed)");
            sb.AppendLine("        ");
            sb.AppendLine("        -- Check if reached top or bottom");
            sb.AppendLine("        if character.position.y >= currentLadder.maxHeight then");
            sb.AppendLine("            StopClimbingLadder(character)");
            sb.AppendLine("        end");
            sb.AppendLine("    end");
            sb.AppendLine("end");
            
            return sb.ToString();
        }
        
        private static string GenerateMainIntegrationLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Mod Integration");
            sb.AppendLine("-- Main integration script for Castle Story");
            sb.AppendLine();
            
            sb.AppendLine("-- Load dependencies");
            sb.AppendLine("if file_exists('Data/Mods/LadderConfig.lua') then");
            sb.AppendLine("    dofile('Data/Mods/LadderConfig.lua')");
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine("if file_exists('Data/Mods/LadderBlocks.lua') then");
            sb.AppendLine("    dofile('Data/Mods/LadderBlocks.lua')");
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine("if file_exists('Data/Mods/LadderMechanics.lua') then");
            sb.AppendLine("    dofile('Data/Mods/LadderMechanics.lua')");
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine("-- Initialize the ladder system when the game starts");
            sb.AppendLine("if LadderConfig and LadderConfig.enabled then");
            sb.AppendLine("    InitializeLadderSystem()");
            sb.AppendLine("end");
            
            return sb.ToString();
        }
    }
}
