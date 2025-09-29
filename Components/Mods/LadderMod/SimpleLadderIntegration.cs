using System;
using System.IO;
using System.Text;

namespace CastleStoryMods
{
    public class SimpleLadderIntegration
    {
        private static string GAME_DATA_PATH = @"D:\SteamLibrary\steamapps\common\Castle Story\Info\Lua\Data";
        
        public static bool IntegrateLadders()
        {
            try
            {
                Console.WriteLine("Integrating ladders into Castle Story...");
                
                // Try to find Castle Story installation
                var possiblePaths = new[]
                {
                    @"D:\SteamLibrary\steamapps\common\Castle Story\Info\Lua\Data",
                    @"C:\Program Files (x86)\Steam\steamapps\common\Castle Story\Info\Lua\Data",
                    @"C:\Steam\steamapps\common\Castle Story\Info\Lua\Data",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "Castle Story", "Info", "Lua", "Data"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "Castle Story", "Info", "Lua", "Data")
                };
                
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        GAME_DATA_PATH = path;
                        Console.WriteLine($"Found Castle Story at: {path}");
                        break;
                    }
                }
                
                // Check if Castle Story is installed
                if (!Directory.Exists(GAME_DATA_PATH))
                {
                    Console.WriteLine($"Castle Story data directory not found. Tried paths:");
                    foreach (var path in possiblePaths)
                    {
                        Console.WriteLine($"  - {path}");
                    }
                    Console.WriteLine("\nPlease update the GAME_DATA_PATH in SimpleLadderIntegration.cs");
                    return false;
                }
                
                // Find the building blocks file
                var buildingBlocksPath = Path.Combine(GAME_DATA_PATH, "Data_BuildingBlocks.lua");
                if (!File.Exists(buildingBlocksPath))
                {
                    Console.WriteLine($"Building blocks file not found at: {buildingBlocksPath}");
                    return false;
                }
                
                // Read the current building blocks
                var content = File.ReadAllText(buildingBlocksPath);
                
                // Check if ladders are already added
                if (content.Contains("ladder_wood"))
                {
                    Console.WriteLine("Ladders already integrated!");
                    return true;
                }
                
                // Add ladder blocks to the building system
                var ladderCode = GenerateLadderBlocksCode();
                
                // Insert before the last closing brace
                var lastBraceIndex = content.LastIndexOf("}");
                if (lastBraceIndex > 0)
                {
                    var updatedContent = content.Substring(0, lastBraceIndex) + 
                                      ",\n\n    -- Ladder Blocks (Added by LadderMod)\n" + 
                                      ladderCode + "\n" + 
                                      content.Substring(lastBraceIndex);
                    
                    // Create backup
                    File.Copy(buildingBlocksPath, buildingBlocksPath + ".backup", true);
                    
                    // Write updated content
                    File.WriteAllText(buildingBlocksPath, updatedContent);
                    
                    Console.WriteLine("✅ Ladders successfully integrated into Castle Story!");
                    Console.WriteLine("📁 Backup created: Data_BuildingBlocks.lua.backup");
                    Console.WriteLine("🎮 Start Castle Story to see the ladders in the building menu!");
                    
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Could not find proper location to insert ladder blocks");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error integrating ladders: {ex.Message}");
                return false;
            }
        }
        
        private static string GenerateLadderBlocksCode()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("    [\"ladder_wood\"] = {");
            sb.AppendLine("        name = \"Wooden Ladder\",");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        material = \"wood\",");
            sb.AppendLine("        durability = 100,");
            sb.AppendLine("        cost = { wood = 2 },");
            sb.AppendLine("        icon = \"ladder_wood_icon\",");
            sb.AppendLine("        model = \"ladder_wood_model\",");
            sb.AppendLine("        canClimb = true,");
            sb.AppendLine("        climbSpeed = 2.0");
            sb.AppendLine("    },");
            
            sb.AppendLine("    [\"ladder_iron\"] = {");
            sb.AppendLine("        name = \"Iron Ladder\",");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        material = \"iron\",");
            sb.AppendLine("        durability = 200,");
            sb.AppendLine("        cost = { iron = 1, wood = 1 },");
            sb.AppendLine("        icon = \"ladder_iron_icon\",");
            sb.AppendLine("        model = \"ladder_iron_model\",");
            sb.AppendLine("        canClimb = true,");
            sb.AppendLine("        climbSpeed = 3.0");
            sb.AppendLine("    },");
            
            sb.AppendLine("    [\"ladder_stone\"] = {");
            sb.AppendLine("        name = \"Stone Ladder\",");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        material = \"stone\",");
            sb.AppendLine("        durability = 300,");
            sb.AppendLine("        cost = { stone = 2 },");
            sb.AppendLine("        icon = \"ladder_stone_icon\",");
            sb.AppendLine("        model = \"ladder_stone_model\",");
            sb.AppendLine("        canClimb = true,");
            sb.AppendLine("        climbSpeed = 1.6");
            sb.AppendLine("    },");
            
            sb.AppendLine("    [\"ladder_rope\"] = {");
            sb.AppendLine("        name = \"Rope Ladder\",");
            sb.AppendLine("        category = \"building\",");
            sb.AppendLine("        material = \"rope\",");
            sb.AppendLine("        durability = 50,");
            sb.AppendLine("        cost = { rope = 3, wood = 1 },");
            sb.AppendLine("        icon = \"ladder_rope_icon\",");
            sb.AppendLine("        model = \"ladder_rope_model\",");
            sb.AppendLine("        canClimb = true,");
            sb.AppendLine("        climbSpeed = 2.4");
            sb.AppendLine("    }");
            
            return sb.ToString();
        }
        
        public static void Main(string[] args)
        {
            Console.WriteLine("Castle Story Ladder Integration Tool");
            Console.WriteLine("====================================");
            Console.WriteLine();
            
            bool success = IntegrateLadders();
            
            if (success)
            {
                Console.WriteLine();
                Console.WriteLine("🎉 Integration completed successfully!");
                Console.WriteLine("📝 The following ladders have been added:");
                Console.WriteLine("   • Wooden Ladder (2 wood)");
                Console.WriteLine("   • Iron Ladder (1 iron + 1 wood)");
                Console.WriteLine("   • Stone Ladder (2 stone)");
                Console.WriteLine("   • Rope Ladder (3 rope + 1 wood)");
                Console.WriteLine();
                Console.WriteLine("🎮 Start Castle Story and check the building menu!");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("❌ Integration failed!");
                Console.WriteLine("💡 Make sure Castle Story is installed and the path is correct.");
            }
            
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
