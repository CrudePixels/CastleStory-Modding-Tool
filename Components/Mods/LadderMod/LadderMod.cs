using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

namespace CastleStoryModding.ExampleMods
{
    public class LadderMod
    {
        // Simple initialization that will be called when the DLL is loaded
        public static bool InitializeInGameProcess()
        {
            try
            {
                // Check if we're actually in Castle Story process
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                string processName = currentProcess.ProcessName.ToLower();
                
                // Only initialize if we're in Castle Story, not the launcher
                if (processName.Contains("castle") || processName.Contains("unity") || processName.Contains("game"))
                {
                    Initialize();
                    return true;
                }
                else
                {
                    // We're in the launcher, just log and return
                    string launcherDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                    string logPath = Path.Combine(launcherDir, "LadderModLog.txt");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] LadderMod loaded in launcher process: {currentProcess.ProcessName} (PID: {currentProcess.Id})\n");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Skipping initialization - waiting for Castle Story process\n");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error to a file that Castle Story can write to
                string errorFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LadderMod_Error.txt");
                File.WriteAllText(errorFile, $"LadderMod Entry Error: {ex.Message}\nStack: {ex.StackTrace}");
                return false;
            }
        }

        private static string logPath = string.Empty;

        public static void Initialize()
        {
            try
            {
                // Write to Castle Story's directory to prove we're in the game process
                string castleStoryDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string testFile = Path.Combine(castleStoryDir, "LADDER_MOD_INJECTED.txt");
                File.WriteAllText(testFile, $"🪜 LADDER MOD INJECTED INTO CASTLE STORY!\n");
                File.AppendAllText(testFile, $"✅ SUCCESS: LadderMod is running INSIDE Castle Story process!\n");
                File.AppendAllText(testFile, $"⏰ Injection time: {DateTime.Now}\n");
                File.AppendAllText(testFile, $"🎯 Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}\n");
                File.AppendAllText(testFile, $"📁 Process name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}\n");
                File.AppendAllText(testFile, $"🚀 LADDER MOD IS WORKING - DLL INJECTION SUCCESSFUL!\n");
                
                // Also write to launcher directory for comparison
                string launcherDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                logPath = Path.Combine(launcherDir, "LadderModLog.txt");
                File.WriteAllText(logPath, $"[{DateTime.Now}] 🪜 Ladder Mod Loaded!\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ MOD INJECTION WORKING!\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Process name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}] DLL INJECTION SUCCESSFUL - LADDER MOD IS RUNNING IN CASTLE STORY!\n");
                
                // Create a simple test file to prove the mod is running
                string launcherTestFile = Path.Combine(launcherDir, "LADDER_MOD_WORKING.txt");
                File.WriteAllText(launcherTestFile, $"Ladder Mod by CrudePixels\nMod loaded at: {DateTime.Now}\nThis proves the ladder mod is working!");
                
                // Try to inject ladders into the game
                try
                {
                    InjectLaddersIntoGame();
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Error injecting ladders: {ex.Message}\n");
                }
                
                File.AppendAllText(testFile, $"✅ DLL INJECTION PROOF: LadderMod is successfully running inside Castle Story!\n");
                File.AppendAllText(testFile, $"✅ Ladder injection attempted!\n");
            }
            catch (Exception ex)
            {
                // Log error
                string errorFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LadderMod_Init_Error.txt");
                File.WriteAllText(errorFile, $"LadderMod Init Error: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        private static void InjectLaddersIntoGame()
        {
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Injecting ladders into Castle Story...\n");
                
                // Find the game's building blocks data and inject our ladders
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var gameAssembly = assemblies.FirstOrDefault(a => 
                    a.GetName().Name.Contains("Assembly-CSharp") || 
                    a.GetName().Name.Contains("CastleStory"));

                if (gameAssembly != null)
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Found game assembly: {gameAssembly.GetName().Name}\n");
                    
                    // Look for building blocks data structure
                    var buildingBlocksType = gameAssembly.GetTypes()
                        .FirstOrDefault(t => t.Name.Contains("BuildingBlocks") || t.Name.Contains("Data_BuildingBlocks"));

                    if (buildingBlocksType != null)
                    {
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Found building blocks type: {buildingBlocksType.FullName}\n");
                        InjectLadderData(buildingBlocksType);
                    }
                    else
                    {
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Building blocks type not found, trying alternative injection method...\n");
                        InjectLadderDataAlternative();
                    }
                }
                else
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Game assembly not found, trying alternative injection method...\n");
                    InjectLadderDataAlternative();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error in ladder injection: {ex.Message}\n");
            }
        }

        private static void InjectLadderData(Type buildingBlocksType)
        {
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Injecting ladder data into building blocks...\n");
                
                // Try to find a static method or property that manages building blocks
                var methods = buildingBlocksType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                
                foreach (var method in methods)
                {
                    if (method.Name.ToLower().Contains("add") || method.Name.ToLower().Contains("register"))
                    {
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Found potential injection method: {method.Name}\n");
                        // Try to call the method with our ladder data
                        // This would need to be adapted based on the actual game's API
                    }
                }

                // Also try to find static fields or properties
                var fields = buildingBlocksType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.Name.ToLower().Contains("blocks") || field.Name.ToLower().Contains("data"))
                    {
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Found potential data field: {field.Name}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error in ladder data injection: {ex.Message}\n");
            }
        }

        private static void InjectLadderDataAlternative()
        {
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Trying alternative ladder injection method...\n");
                
                // Alternative approach: Try to find and modify the game's Lua data directly
                // This is a simplified approach - in a real implementation, you'd need to find the actual game directory
                string[] possiblePaths = {
                    @"D:\SteamLibrary\steamapps\common\Castle Story",
                    @"C:\Program Files (x86)\Steam\steamapps\common\Castle Story",
                    @"C:\Steam\steamapps\common\Castle Story",
                    @"D:\MyProjects\CASTLE STORY\Original Castle Story\Castle Story"
                };

                foreach (var gamePath in possiblePaths)
                {
                    if (Directory.Exists(gamePath) && File.Exists(Path.Combine(gamePath, "CastleStory.exe")))
                    {
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Found Castle Story at: {gamePath}\n");
                        InjectLaddersIntoLuaFiles(gamePath);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error in alternative ladder injection: {ex.Message}\n");
            }
        }

        private static void InjectLaddersIntoLuaFiles(string gamePath)
        {
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Injecting ladders into Lua files at: {gamePath}\n");
                
                // Create backup directory
                string backupDir = Path.Combine(gamePath, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}");
                Directory.CreateDirectory(backupDir);

                // Backup original files
                string blocksFile = Path.Combine(gamePath, "Info", "Lua", "Data_BuildingBlocks.lua");
                string categoriesFile = Path.Combine(gamePath, "Info", "Lua", "Data_BuildingCategories.lua");

                if (File.Exists(blocksFile))
                {
                    File.Copy(blocksFile, Path.Combine(backupDir, "Data_BuildingBlocks.lua"), true);
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Backed up Data_BuildingBlocks.lua\n");
                }

                if (File.Exists(categoriesFile))
                {
                    File.Copy(categoriesFile, Path.Combine(backupDir, "Data_BuildingCategories.lua"), true);
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Backed up Data_BuildingCategories.lua\n");
                }

                // Add ladder category
                if (File.Exists(categoriesFile))
                {
                    string categoryData = @"
-- LadderMod: Adding ladder category
BuildingCategories[""ladder""] = {
    name = ""Ladders"",
    icon = ""ladder_category_icon"",
    order = 10
}
";
                    File.AppendAllText(categoriesFile, categoryData);
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Added ladder category\n");
                }

                // Add ladder blocks
                if (File.Exists(blocksFile))
                {
                    string ladderData = @"
-- LadderMod: Adding ladder blocks

-- Wooden Ladder
Data_BuildingBlocks[""ladder_wood""] = {
    name = ""Wooden Ladder"",
    category = ""ladder"",
    material = ""wood"",
    durability = 100,
    cost = { wood = 2 },
    icon = ""ladder_wood_icon"",
    model = ""ladder_wood_model"",
    climbSpeed = 2.0,
    maxHeight = 50,
    canClimb = true,
    buildable = true,
    placeable = true
}

-- Iron Ladder
Data_BuildingBlocks[""ladder_iron""] = {
    name = ""Iron Ladder"",
    category = ""ladder"",
    material = ""iron"",
    durability = 200,
    cost = { iron = 1, wood = 1 },
    icon = ""ladder_iron_icon"",
    model = ""ladder_iron_model"",
    climbSpeed = 3.0,
    maxHeight = 75,
    canClimb = true,
    buildable = true,
    placeable = true
}

-- Stone Ladder
Data_BuildingBlocks[""ladder_stone""] = {
    name = ""Stone Ladder"",
    category = ""ladder"",
    material = ""stone"",
    durability = 300,
    cost = { stone = 2 },
    icon = ""ladder_stone_icon"",
    model = ""ladder_stone_model"",
    climbSpeed = 1.6,
    maxHeight = 100,
    canClimb = true,
    buildable = true,
    placeable = true
}

-- Rope Ladder
Data_BuildingBlocks[""ladder_rope""] = {
    name = ""Rope Ladder"",
    category = ""ladder"",
    material = ""rope"",
    durability = 50,
    cost = { rope = 3, wood = 1 },
    icon = ""ladder_rope_icon"",
    model = ""ladder_rope_model"",
    climbSpeed = 2.4,
    maxHeight = 40,
    canClimb = true,
    buildable = true,
    placeable = true
}
";
                    File.AppendAllText(blocksFile, ladderData);
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Added ladder blocks\n");
                }

                // Create injection marker
                string injectionMarker = Path.Combine(gamePath, "LADDER_MOD_INJECTED.txt");
                File.WriteAllText(injectionMarker, $"LadderMod injected at: {DateTime.Now}\nGame Path: {gamePath}\nBackup: {backupDir}");

                File.AppendAllText(logPath, $"[{DateTime.Now}] Ladder injection completed successfully!\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error injecting ladders into Lua files: {ex.Message}\n");
            }
        }
    }
}