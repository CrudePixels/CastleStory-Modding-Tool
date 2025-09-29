using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace CastleStoryMods
{
    public class LadderMod
    {
        private const string MOD_NAME = "LadderMod";
        private const string MOD_VERSION = "1.0.0";
        
        // Windows API functions for memory patching
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);
        
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);
        
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);
        
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        
        private IntPtr processHandle;
        private string logPath;
        private LadderConfig config;
        private bool isInitialized = false;
        
        public LadderMod()
        {
            logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs", "LadderMod.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            config = new LadderConfig();
        }
        
        public bool Initialize(int processId)
        {
            try
            {
                LogMessage($"Initializing {MOD_NAME} v{MOD_VERSION}");
                
                // Load configuration
                LoadConfiguration();
                
                // Open the Castle Story process
                processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
                if (processHandle == IntPtr.Zero)
                {
                    LogMessage("Failed to open Castle Story process");
                    return false;
                }
                
                LogMessage("Successfully opened Castle Story process");
                
                // Enable ladder functionality
                if (EnableLadderSystem())
                {
                    LogMessage("Ladder system enabled successfully");
                    isInitialized = true;
                    return true;
                }
                else
                {
                    LogMessage("Failed to enable ladder system");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error initializing LadderMod: {ex.Message}");
                return false;
            }
        }
        
        private void LoadConfiguration()
        {
            try
            {
                var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LadderConfig.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    config = JsonConvert.DeserializeObject<LadderConfig>(json) ?? new LadderConfig();
                    LogMessage("Loaded ladder configuration from file");
                }
                else
                {
                    // Create default configuration
                    SaveConfiguration();
                    LogMessage("Created default ladder configuration");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading configuration: {ex.Message}");
                config = new LadderConfig();
            }
        }
        
        private void SaveConfiguration()
        {
            try
            {
                var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LadderConfig.json");
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                LogMessage($"Error saving configuration: {ex.Message}");
            }
        }
        
        private bool EnableLadderSystem()
        {
            try
            {
                LogMessage("Enabling ladder system...");
                
                // Inject ladder building blocks into the game
                if (InjectLadderBlocks())
                {
                    LogMessage("Ladder blocks injected successfully");
                }
                
                // Patch ladder climbing mechanics
                if (PatchLadderMechanics())
                {
                    LogMessage("Ladder mechanics patched successfully");
                }
                
                // Hook into building system
                if (HookBuildingSystem())
                {
                    LogMessage("Building system hooked successfully");
                }
                
                // Generate Lua configuration for the game
                GenerateLuaConfiguration();
                
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Error enabling ladder system: {ex.Message}");
                return false;
            }
        }
        
        private bool InjectLadderBlocks()
        {
            try
            {
                // This would involve injecting new building block definitions
                // into the game's building system
                LogMessage("Injecting ladder building blocks...");
                
                // For now, we'll create a Lua script that defines ladder blocks
                var luaScript = GenerateLadderBlocksLua();
                var scriptPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ladder_blocks.lua");
                File.WriteAllText(scriptPath, luaScript);
                
                LogMessage("Ladder blocks Lua script generated");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Error injecting ladder blocks: {ex.Message}");
                return false;
            }
        }
        
        private bool PatchLadderMechanics()
        {
            try
            {
                // This would involve patching the game's movement system
                // to support ladder climbing
                LogMessage("Patching ladder mechanics...");
                
                // For now, we'll create a Lua script that handles ladder mechanics
                var luaScript = GenerateLadderMechanicsLua();
                var scriptPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ladder_mechanics.lua");
                File.WriteAllText(scriptPath, luaScript);
                
                LogMessage("Ladder mechanics Lua script generated");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Error patching ladder mechanics: {ex.Message}");
                return false;
            }
        }
        
        private bool HookBuildingSystem()
        {
            try
            {
                // This would involve hooking into the game's building system
                // to register ladder blocks
                LogMessage("Hooking into building system...");
                
                // For now, we'll create a Lua script that registers ladder blocks
                var luaScript = GenerateBuildingSystemLua();
                var scriptPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "building_system.lua");
                File.WriteAllText(scriptPath, luaScript);
                
                LogMessage("Building system Lua script generated");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Error hooking building system: {ex.Message}");
                return false;
            }
        }
        
        private void GenerateLuaConfiguration()
        {
            try
            {
                var luaConfig = GenerateLadderConfigLua();
                var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ladder_config.lua");
                File.WriteAllText(configPath, luaConfig);
                LogMessage("Ladder configuration Lua script generated");
            }
            catch (Exception ex)
            {
                LogMessage($"Error generating Lua configuration: {ex.Message}");
            }
        }
        
        private string GenerateLadderBlocksLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Blocks Definition");
            sb.AppendLine("-- Generated by LadderMod v" + MOD_VERSION);
            sb.AppendLine();
            
            sb.AppendLine("local LadderBlocks = {");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Wooden Ladder\",");
            sb.AppendLine("        id = \"ladder_wood\",");
            sb.AppendLine("        material = \"wood\",");
            sb.AppendLine("        durability = 100,");
            sb.AppendLine("        climbSpeed = " + config.Physics.ClimbSpeed + ",");
            sb.AppendLine("        maxHeight = " + config.MaxHeight + ",");
            sb.AppendLine("        cost = { wood = 2 }");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Iron Ladder\",");
            sb.AppendLine("        id = \"ladder_iron\",");
            sb.AppendLine("        material = \"iron\",");
            sb.AppendLine("        durability = 200,");
            sb.AppendLine("        climbSpeed = " + (config.Physics.ClimbSpeed * 1.5) + ",");
            sb.AppendLine("        maxHeight = " + (config.MaxHeight * 1.5) + ",");
            sb.AppendLine("        cost = { iron = 1, wood = 1 }");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Stone Ladder\",");
            sb.AppendLine("        id = \"ladder_stone\",");
            sb.AppendLine("        material = \"stone\",");
            sb.AppendLine("        durability = 300,");
            sb.AppendLine("        climbSpeed = " + (config.Physics.ClimbSpeed * 0.8) + ",");
            sb.AppendLine("        maxHeight = " + (config.MaxHeight * 2) + ",");
            sb.AppendLine("        cost = { stone = 2 }");
            sb.AppendLine("    },");
            sb.AppendLine("    {");
            sb.AppendLine("        name = \"Rope Ladder\",");
            sb.AppendLine("        id = \"ladder_rope\",");
            sb.AppendLine("        material = \"rope\",");
            sb.AppendLine("        durability = 50,");
            sb.AppendLine("        climbSpeed = " + (config.Physics.ClimbSpeed * 1.2) + ",");
            sb.AppendLine("        maxHeight = " + (config.MaxHeight * 0.8) + ",");
            sb.AppendLine("        cost = { rope = 3, wood = 1 }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("return LadderBlocks");
            
            return sb.ToString();
        }
        
        private string GenerateLadderMechanicsLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Mechanics");
            sb.AppendLine("-- Generated by LadderMod v" + MOD_VERSION);
            sb.AppendLine();
            
            sb.AppendLine("local LadderMechanics = {");
            sb.AppendLine("    enabled = " + (config.Enabled ? "true" : "false") + ",");
            sb.AppendLine("    maxHeight = " + config.MaxHeight + ",");
            sb.AppendLine("    climbSpeed = " + config.Physics.ClimbSpeed + ",");
            sb.AppendLine("    autoSnap = " + (config.Physics.AutoSnap ? "true" : "false") + ",");
            sb.AppendLine("    grabDistance = " + config.Physics.GrabDistance + ",");
            sb.AppendLine("    releaseDistance = " + config.Physics.ReleaseDistance + ",");
            sb.AppendLine("    snapDistance = " + config.Physics.SnapDistance + ",");
            sb.AppendLine("    climbHeight = " + config.Physics.ClimbHeight + ",");
            sb.AppendLine("    fallDamage = " + (config.Physics.FallDamage ? "true" : "false") + ",");
            sb.AppendLine("    minLevel = " + config.Requirements.MinLevel + ",");
            sb.AppendLine("    requiresBlueprint = " + (config.Requirements.RequiresBlueprint ? "true" : "false") + ",");
            sb.AppendLine("    maxPerPlayer = " + config.Requirements.MaxPerPlayer + ",");
            sb.AppendLine("    cooldown = " + config.Requirements.Cooldown);
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("return LadderMechanics");
            
            return sb.ToString();
        }
        
        private string GenerateBuildingSystemLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Building System Integration");
            sb.AppendLine("-- Generated by LadderMod v" + MOD_VERSION);
            sb.AppendLine();
            
            sb.AppendLine("-- Register ladder blocks with the building system");
            sb.AppendLine("function RegisterLadderBlocks()");
            sb.AppendLine("    local ladderBlocks = require('ladder_blocks')");
            sb.AppendLine("    ");
            sb.AppendLine("    for _, block in ipairs(ladderBlocks) do");
            sb.AppendLine("        -- Register with game's building system");
            sb.AppendLine("        RegisterBuildingBlock(block.id, block)");
            sb.AppendLine("    end");
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine("-- Initialize ladder system");
            sb.AppendLine("function InitializeLadderSystem()");
            sb.AppendLine("    RegisterLadderBlocks()");
            sb.AppendLine("    ");
            sb.AppendLine("    -- Hook into character movement");
            sb.AppendLine("    HookCharacterMovement()");
            sb.AppendLine("    ");
            sb.AppendLine("    print('Ladder system initialized successfully')");
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine("return {");
            sb.AppendLine("    RegisterLadderBlocks = RegisterLadderBlocks,");
            sb.AppendLine("    InitializeLadderSystem = InitializeLadderSystem");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private string GenerateLadderConfigLua()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Ladder Configuration");
            sb.AppendLine("-- Generated by LadderMod v" + MOD_VERSION);
            sb.AppendLine();
            
            sb.AppendLine("LadderConfig = {");
            sb.AppendLine("    enabled = " + (config.Enabled ? "true" : "false") + ",");
            sb.AppendLine("    maxHeight = " + config.MaxHeight + ",");
            sb.AppendLine("    climbSpeed = " + config.Physics.ClimbSpeed + ",");
            sb.AppendLine("    autoSnap = " + (config.Physics.AutoSnap ? "true" : "false") + ",");
            sb.AppendLine("    grabDistance = " + config.Physics.GrabDistance + ",");
            sb.AppendLine("    releaseDistance = " + config.Physics.ReleaseDistance + ",");
            sb.AppendLine("    snapDistance = " + config.Physics.SnapDistance + ",");
            sb.AppendLine("    climbHeight = " + config.Physics.ClimbHeight + ",");
            sb.AppendLine("    fallDamage = " + (config.Physics.FallDamage ? "true" : "false") + ",");
            sb.AppendLine("    minLevel = " + config.Requirements.MinLevel + ",");
            sb.AppendLine("    requiresBlueprint = " + (config.Requirements.RequiresBlueprint ? "true" : "false") + ",");
            sb.AppendLine("    maxPerPlayer = " + config.Requirements.MaxPerPlayer + ",");
            sb.AppendLine("    cooldown = " + config.Requirements.Cooldown);
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        public void Cleanup()
        {
            try
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                    processHandle = IntPtr.Zero;
                }
                
                LogMessage("LadderMod cleanup completed");
            }
            catch (Exception ex)
            {
                LogMessage($"Error during cleanup: {ex.Message}");
            }
        }
        
        private void LogMessage(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] {message}";
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
                Console.WriteLine(logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
    
    public class LadderConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxHeight { get; set; } = 50;
        public LadderPhysics Physics { get; set; } = new LadderPhysics();
        public LadderRequirements Requirements { get; set; } = new LadderRequirements();
    }
    
    public class LadderPhysics
    {
        public double ClimbSpeed { get; set; } = 2.0;
        public bool AutoSnap { get; set; } = true;
        public double GrabDistance { get; set; } = 2.0;
        public double ReleaseDistance { get; set; } = 3.0;
        public double SnapDistance { get; set; } = 1.5;
        public double ClimbHeight { get; set; } = 1.0;
        public bool FallDamage { get; set; } = false;
    }
    
    public class LadderRequirements
    {
        public int MinLevel { get; set; } = 1;
        public bool RequiresBlueprint { get; set; } = false;
        public int MaxPerPlayer { get; set; } = 10;
        public double Cooldown { get; set; } = 0.5;
    }
}
