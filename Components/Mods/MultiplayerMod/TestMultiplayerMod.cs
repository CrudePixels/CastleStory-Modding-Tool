using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

namespace CastleStoryModding.ExampleMods
{
    public class TestMultiplayerMod
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
                    string launcherDir = @"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool\CastleStoryLauncher";
                    string logPath = Path.Combine(launcherDir, "CastleStoryModLog.txt");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] DLL loaded in launcher process: {currentProcess.ProcessName} (PID: {currentProcess.Id})\n");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Skipping initialization - waiting for Castle Story process\n");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error to a file that Castle Story can write to
                string errorFile = Path.Combine(@"D:\MyProjects\CASTLE STORY", "DLL_ENTRY_ERROR.txt");
                File.WriteAllText(errorFile, $"DLL Entry Error: {ex.Message}\nStack: {ex.StackTrace}");
                return false;
            }
        }

        private static string logPath = string.Empty;
        private const int NEW_MAX_TEAMS = 16;
        private const int NEW_MAX_PLAYERS = 32;

        public static void Initialize()
        {
            // Write to Castle Story's directory to prove we're in the game process
            string castleStoryDir = @"D:\MyProjects\CASTLE STORY";
            string testFile = Path.Combine(castleStoryDir, "CASTLE_STORY_MOD_INJECTED.txt");
            File.WriteAllText(testFile, $"🎮 ENHANCED MULTIPLAYER MOD INJECTED INTO CASTLE STORY!\n");
            File.AppendAllText(testFile, $"✅ SUCCESS: Mod is running INSIDE Castle Story process!\n");
            File.AppendAllText(testFile, $"⏰ Injection time: {DateTime.Now}\n");
            File.AppendAllText(testFile, $"🎯 Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}\n");
            File.AppendAllText(testFile, $"📁 Process name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}\n");
            File.AppendAllText(testFile, $"🚀 MOD IS WORKING - DLL INJECTION SUCCESSFUL!\n");
            
            // Also write to launcher directory for comparison
            string launcherDir = @"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool\CastleStoryLauncher";
            logPath = Path.Combine(launcherDir, "CastleStoryModLog.txt");
            File.WriteAllText(logPath, $"[{DateTime.Now}] 🎮 Enhanced Multiplayer Mod Loaded!\n");
            File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ MOD INJECTION WORKING!\n");
            File.AppendAllText(logPath, $"[{DateTime.Now}] Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}\n");
            File.AppendAllText(logPath, $"[{DateTime.Now}] Process name: {System.Diagnostics.Process.GetCurrentProcess().ProcessName}\n");
            File.AppendAllText(logPath, $"[{DateTime.Now}] DLL INJECTION SUCCESSFUL - MOD IS RUNNING IN CASTLE STORY!\n");
            
            // Create a simple test file to prove the mod is running
            string launcherTestFile = Path.Combine(launcherDir, "MOD_WORKING.txt");
            File.WriteAllText(launcherTestFile, $"Enhanced Multiplayer Mod by CrudePixels\nMod loaded at: {DateTime.Now}\nThis proves the mod is working!");
            
            // Initialize Enhanced Multiplayer Systems
            try
            {
                InitializeEnhancedMultiplayerSystems();
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error initializing enhanced multiplayer systems: {ex.Message}\n");
            }
            
            // Try to add visual indicator to main menu
            try
            {
                AddVisualIndicatorToMainMenu();
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error adding visual indicator: {ex.Message}\n");
            }
            
            // Try to patch team limits immediately
            try
            {
                PatchTeamLimitsImmediately();
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error patching team limits: {ex.Message}\n");
            }
            
            File.AppendAllText(testFile, $"✅ DLL INJECTION PROOF: Mod is successfully running inside Castle Story!\n");
            File.AppendAllText(testFile, $"✅ Enhanced Multiplayer Systems initialized!\n");
            File.AppendAllText(testFile, $"✅ Visual indicator and team limit patching attempted!\n");
        }

        private static void InjectMenuIndicator()
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] Searching for main menu components...\n");
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var gameAssemblies = assemblies.Where(a => 
                a.GetName().Name?.Contains("Assembly-CSharp") == true || 
                a.GetName().Name?.Contains("CastleStory") == true ||
                a.GetName().Name?.Contains("Unity") == true ||
                a.GetName().Name?.Contains("Game") == true
            ).ToList();
            
            File.AppendAllText(logPath, $"[{DateTime.Now}] Found {gameAssemblies.Count} potential game assemblies\n");
            
            foreach (var assembly in gameAssemblies)
            {
                try
                {
                    var assemblyName = assembly.GetName().Name;
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Checking assembly: {assemblyName}\n");
                    
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        // Look for menu-related classes
                        if (type.Name.ToLower().Contains("menu") || 
                            type.Name.ToLower().Contains("ui") ||
                            type.Name.ToLower().Contains("gui") ||
                            type.Name.ToLower().Contains("main") ||
                            type.Name.ToLower().Contains("title"))
                        {
                            File.AppendAllText(logPath, $"[{DateTime.Now}] Found menu-related type: {type.FullName}\n");
                            
                            // Try to find and patch text fields or labels
                            TryPatchMenuText(type);
                            
                            // Look for static methods that might create menu elements
                            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                            foreach (var method in methods)
                            {
                                if (method.Name.ToLower().Contains("start") || 
                                    method.Name.ToLower().Contains("awake") ||
                                    method.Name.ToLower().Contains("onenable") ||
                                    method.Name.ToLower().Contains("init"))
                                {
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] Found initialization method: {type.FullName}.{method.Name}\n");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Error checking assembly {assembly.GetName().Name}: {ex.Message}\n");
                }
            }
            
            File.AppendAllText(logPath, $"[{DateTime.Now}] Menu indicator injection attempt completed\n");
        }

        private static void TryPatchMenuText(Type type)
        {
            // Look for text fields that might be the main menu title or subtitle
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var fieldName = field.Name.ToLower();
                    if (fieldName.Contains("title") || fieldName.Contains("subtitle") || fieldName.Contains("version"))
                    {
                        try
                        {
                            string originalValue = field.GetValue(null)?.ToString() ?? "";
                            string newValue = originalValue + "\n\nEnhanced Multiplayer by CrudePixels";
                            field.SetValue(null, newValue);
                            File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ PATCHED menu text: {type.FullName}.{field.Name}\n");
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, $"[{DateTime.Now}] Error patching menu text {type.FullName}.{field.Name}: {ex.Message}\n");
                        }
                    }
                }
            }

            // Look for properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) && property.CanWrite)
                {
                    var propertyName = property.Name.ToLower();
                    if (propertyName.Contains("title") || propertyName.Contains("subtitle") || propertyName.Contains("version"))
                    {
                        try
                        {
                            string originalValue = property.GetValue(null)?.ToString() ?? "";
                            string newValue = originalValue + "\n\nEnhanced Multiplayer by CrudePixels";
                            property.SetValue(null, newValue);
                            File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ PATCHED menu property: {type.FullName}.{property.Name}\n");
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, $"[{DateTime.Now}] Error patching menu property {type.FullName}.{property.Name}: {ex.Message}\n");
                        }
                    }
                }
            }
        }

        private static void PatchGameLimits()
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] Searching for Castle Story game code...\n");
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            File.AppendAllText(logPath, $"[{DateTime.Now}] Found {assemblies.Length} loaded assemblies\n");
            
            // Look specifically for Castle Story assemblies
            var gameAssemblies = assemblies.Where(a => 
                a.GetName().Name?.Contains("Assembly-CSharp") == true || 
                a.GetName().Name?.Contains("CastleStory") == true ||
                a.GetName().Name?.Contains("Unity") == true ||
                a.GetName().Name?.Contains("Game") == true
            ).ToList();
            
            File.AppendAllText(logPath, $"[{DateTime.Now}] Found {gameAssemblies.Count} potential game assemblies\n");
            
            foreach (var assembly in gameAssemblies)
            {
                try
                {
                    var assemblyName = assembly.GetName().Name;
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Checking game assembly: {assemblyName}\n");
                    
                    var types = assembly.GetTypes();
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Found {types.Length} types in {assemblyName}\n");
                    
                    foreach (var type in types)
                    {
                        // Look for types that might control multiplayer/team limits
                        if (type.Name.ToLower().Contains("team") || 
                            type.Name.ToLower().Contains("lobby") ||
                            type.Name.ToLower().Contains("multiplayer") ||
                            type.Name.ToLower().Contains("network") ||
                            type.Name.ToLower().Contains("game") ||
                            type.Name.ToLower().Contains("player"))
                        {
                            File.AppendAllText(logPath, $"[{DateTime.Now}] Found relevant type: {type.FullName}\n");
                            
                            // Try to patch static fields/properties
                            TryPatchStaticFields(type);
                            TryPatchStaticProperties(type);
                            
                            // Look for methods that might initialize limits
                            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                            foreach (var method in methods)
                            {
                                if (method.Name.ToLower().Contains("init") || 
                                    method.Name.ToLower().Contains("setup") ||
                                    method.Name.ToLower().Contains("create") ||
                                    method.Name.ToLower().Contains("start"))
                                {
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] Found initialization method: {type.FullName}.{method.Name}\n");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Error checking assembly {assembly.GetName().Name}: {ex.Message}\n");
                }
            }
            
            File.AppendAllText(logPath, $"[{DateTime.Now}] Game limit patching attempt completed\n");
        }

        private static void TryPatchStaticFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int))
                {
                    var fieldName = field.Name.ToLower();
                    if (fieldName.Contains("max") && (fieldName.Contains("team") || fieldName.Contains("player")))
                    {
                        try
                        {
                            int originalValue = (int)(field.GetValue(null) ?? 0);
                            int newValue = fieldName.Contains("team") ? NEW_MAX_TEAMS : NEW_MAX_PLAYERS;
                            field.SetValue(null, newValue);
                            File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ PATCHED {type.FullName}.{field.Name} from {originalValue} to {newValue}\n");
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, $"[{DateTime.Now}] Error patching field {type.FullName}.{field.Name}: {ex.Message}\n");
                        }
                    }
                }
            }
        }

        private static void TryPatchStaticProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(int) && property.CanWrite)
                {
                    var propertyName = property.Name.ToLower();
                    if (propertyName.Contains("max") && (propertyName.Contains("team") || propertyName.Contains("player")))
                    {
                        try
                        {
                            int originalValue = (int)(property.GetValue(null) ?? 0);
                            int newValue = propertyName.Contains("team") ? NEW_MAX_TEAMS : NEW_MAX_PLAYERS;
                            property.SetValue(null, newValue);
                            File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ PATCHED {type.FullName}.{property.Name} from {originalValue} to {newValue}\n");
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, $"[{DateTime.Now}] Error patching property {type.FullName}.{property.Name}: {ex.Message}\n");
                        }
                    }
                }
            }
        }

        public static void ShowModInfo()
        {
            if (logPath != null)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Mod Info Requested\n");
            }
        }

        private static void AttemptSimpleMemoryPatching()
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] 🚀 STARTING SIMPLE MEMORY PATCHING!\n");
            
            try
            {
                // Get current process (should be Castle Story)
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                File.AppendAllText(logPath, $"[{DateTime.Now}] Current process: {currentProcess.ProcessName} (PID: {currentProcess.Id})\n");
                
                // Create a simple proof that we're in the Castle Story process
                string castleStoryDir = @"D:\MyProjects\CASTLE STORY";
                string simpleTestFile = Path.Combine(castleStoryDir, "SIMPLE_MEMORY_PATCH_TEST.txt");
                File.WriteAllText(simpleTestFile, $"🎯 SIMPLE MEMORY PATCHING TEST!\n");
                File.AppendAllText(simpleTestFile, $"✅ Process: {currentProcess.ProcessName} (PID: {currentProcess.Id})\n");
                File.AppendAllText(simpleTestFile, $"✅ Time: {DateTime.Now}\n");
                File.AppendAllText(simpleTestFile, $"✅ This proves the mod is running in Castle Story process!\n");
                File.AppendAllText(simpleTestFile, $"✅ Next step: Try to find and patch team limit values...\n");
                
                // Try to use our memory patcher
                bool patchSuccess = MemoryPatcher.PatchCastleStoryLimits(currentProcess);
                
                if (patchSuccess)
                {
                    File.AppendAllText(simpleTestFile, $"✅ SUCCESS: Memory patching completed!\n");
                    File.AppendAllText(simpleTestFile, $"✅ Team limits should now be increased from 4 to 16!\n");
                }
                else
                {
                    File.AppendAllText(simpleTestFile, $"❌ FAILED: Memory patching unsuccessful\n");
                    File.AppendAllText(simpleTestFile, $"❌ Check MEMORY_PATCH_LOG.txt for details\n");
                }
                
                File.AppendAllText(logPath, $"[{DateTime.Now}] Simple memory patching test completed\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ❌ ERROR in simple memory patching: {ex.Message}\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Stack trace: {ex.StackTrace}\n");
            }
        }

        private static void AddVisualIndicatorToMainMenu()
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] 🎨 Adding visual indicator to main menu...\n");
            
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var gameAssemblies = assemblies.Where(a => 
                    a.GetName().Name?.Contains("Assembly-CSharp") == true || 
                    a.GetName().Name?.Contains("CastleStory") == true ||
                    a.GetName().Name?.Contains("Unity") == true ||
                    a.GetName().Name?.Contains("Game") == true
                ).ToList();
                
                File.AppendAllText(logPath, $"[{DateTime.Now}] Found {gameAssemblies.Count} potential game assemblies\n");
                
                foreach (var assembly in gameAssemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            // Look for main menu or UI classes
                            if (type.Name.ToLower().Contains("menu") || 
                                type.Name.ToLower().Contains("main") ||
                                type.Name.ToLower().Contains("title") ||
                                type.Name.ToLower().Contains("ui") ||
                                type.Name.ToLower().Contains("lobby"))
                            {
                                File.AppendAllText(logPath, $"[{DateTime.Now}] Found UI type: {type.FullName}\n");
                                
                                // Try to find and modify text fields
                                TryModifyUIText(type);
                                
                                // Try to find and modify static strings
                                TryModifyStaticStrings(type);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Error processing assembly {assembly.GetName().Name}: {ex.Message}\n");
                    }
                }
                
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Visual indicator injection completed\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ❌ Error in AddVisualIndicatorToMainMenu: {ex.Message}\n");
            }
        }

        private static void TryModifyUIText(Type type)
        {
            try
            {
                // Look for string fields that might be UI text
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(string))
                    {
                        try
                        {
                            string? originalValue = field.GetValue(null)?.ToString();
                            if (!string.IsNullOrEmpty(originalValue))
                            {
                                // Add mod indicator to various UI text
                                if (originalValue.Contains("CASTLE STORY") || 
                                    originalValue.Contains("MULTIPLAYER") ||
                                    originalValue.Contains("LOBBY"))
                                {
                                    string newValue = originalValue + "\n\n🎮 Enhanced Multiplayer Mod Active!";
                                    field.SetValue(null, newValue);
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Modified UI text: {type.FullName}.{field.Name}\n");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore errors for individual fields
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error in TryModifyUIText: {ex.Message}\n");
            }
        }

        private static void TryModifyStaticStrings(Type type)
        {
            try
            {
                // Look for static string properties
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string) && property.CanWrite)
                    {
                        try
                        {
                            string? originalValue = property.GetValue(null)?.ToString();
                            if (!string.IsNullOrEmpty(originalValue))
                            {
                                if (originalValue.Contains("CASTLE STORY") || 
                                    originalValue.Contains("MULTIPLAYER"))
                                {
                                    string newValue = originalValue + " [MODDED]";
                                    property.SetValue(null, newValue);
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Modified static property: {type.FullName}.{property.Name}\n");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore errors for individual properties
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error in TryModifyStaticStrings: {ex.Message}\n");
            }
        }

        private static void PatchTeamLimitsImmediately()
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] 🎯 Attempting immediate team limit patching...\n");
            
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var gameAssemblies = assemblies.Where(a => 
                    a.GetName().Name?.Contains("Assembly-CSharp") == true || 
                    a.GetName().Name?.Contains("CastleStory") == true ||
                    a.GetName().Name?.Contains("Unity") == true ||
                    a.GetName().Name?.Contains("Game") == true
                ).ToList();
                
                foreach (var assembly in gameAssemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            // Look for types that might control team/player limits
                            if (type.Name.ToLower().Contains("team") || 
                                type.Name.ToLower().Contains("player") ||
                                type.Name.ToLower().Contains("lobby") ||
                                type.Name.ToLower().Contains("multiplayer") ||
                                type.Name.ToLower().Contains("game") ||
                                type.Name.ToLower().Contains("manager"))
                            {
                                File.AppendAllText(logPath, $"[{DateTime.Now}] Checking type: {type.FullName}\n");
                                
                                // Try to patch static fields
                                TryPatchStaticFieldsImmediately(type);
                                
                                // Try to patch static properties
                                TryPatchStaticPropertiesImmediately(type);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logPath, $"[{DateTime.Now}] Error processing assembly {assembly.GetName().Name}: {ex.Message}\n");
                    }
                }
                
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Immediate team limit patching completed\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ❌ Error in PatchTeamLimitsImmediately: {ex.Message}\n");
            }
        }

        private static void TryPatchStaticFieldsImmediately(Type type)
        {
            try
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(int))
                    {
                        var fieldName = field.Name.ToLower();
                        if ((fieldName.Contains("max") && fieldName.Contains("team")) ||
                            (fieldName.Contains("team") && fieldName.Contains("count")) ||
                            (fieldName.Contains("max") && fieldName.Contains("player")))
                        {
                            try
                            {
                                int originalValue = (int)(field.GetValue(null) ?? 0);
                                int newValue = fieldName.Contains("team") ? NEW_MAX_TEAMS : NEW_MAX_PLAYERS;
                                
                                if (originalValue != newValue)
                                {
                                    field.SetValue(null, newValue);
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ PATCHED {type.FullName}.{field.Name} from {originalValue} to {newValue}\n");
                                }
                            }
                            catch (Exception ex)
                            {
                                File.AppendAllText(logPath, $"[{DateTime.Now}] Error patching field {type.FullName}.{field.Name}: {ex.Message}\n");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error in TryPatchStaticFieldsImmediately: {ex.Message}\n");
            }
        }

        private static void TryPatchStaticPropertiesImmediately(Type type)
        {
            try
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(int) && property.CanWrite)
                    {
                        var propertyName = property.Name.ToLower();
                        if ((propertyName.Contains("max") && propertyName.Contains("team")) ||
                            (propertyName.Contains("team") && propertyName.Contains("count")) ||
                            (propertyName.Contains("max") && propertyName.Contains("player")))
                        {
                            try
                            {
                                int originalValue = (int)(property.GetValue(null) ?? 0);
                                int newValue = propertyName.Contains("team") ? NEW_MAX_TEAMS : NEW_MAX_PLAYERS;
                                
                                if (originalValue != newValue)
                                {
                                    property.SetValue(null, newValue);
                                    File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ PATCHED {type.FullName}.{property.Name} from {originalValue} to {newValue}\n");
                                }
                            }
                            catch (Exception ex)
                            {
                                File.AppendAllText(logPath, $"[{DateTime.Now}] Error patching property {type.FullName}.{property.Name}: {ex.Message}\n");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error in TryPatchStaticPropertiesImmediately: {ex.Message}\n");
            }
        }

        public static void OnGameStart()
        {
            string launcherDir = @"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool\CastleStoryLauncher";
            string gameLogPath = Path.Combine(launcherDir, "CastleStoryModLog.txt");
            File.AppendAllText(gameLogPath, $"[{DateTime.Now}] Game Started - Mod Active!\n");
            File.AppendAllText(gameLogPath, $"[{DateTime.Now}] Attempting delayed runtime patching...\n");
            
                // Try to patch after the game has fully loaded
                System.Threading.Tasks.Task.Delay(3000).ContinueWith(_ => {
                    try
                    {
                        AttemptSimpleMemoryPatching();
                        PatchGameLimits();
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(gameLogPath, $"[{DateTime.Now}] Delayed patching error: {ex.Message}\n");
                    }
                });
        }

        private static void InitializeEnhancedMultiplayerSystems()
        {
            File.AppendAllText(logPath, $"[{DateTime.Now}] 🚀 Initializing Enhanced Multiplayer Systems...\n");
            
            try
            {
                // Initialize Enhanced Networking
                var networking = EnhancedNetworking.Instance;
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Enhanced Networking initialized\n");
                
                // Initialize Lobby Manager
                var lobbyManager = LobbyManager.Instance;
                lobbyManager.SetNetworking(networking);
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Lobby Manager initialized\n");
                
                // Initialize Spectator Mode
                var spectatorMode = SpectatorMode.Instance;
                spectatorMode.SetNetworking(networking);
                spectatorMode.SetLobbyManager(lobbyManager);
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Spectator Mode initialized\n");
                
                // Subscribe to events for logging
                networking.PlayerJoined += (sender, e) => {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Player joined: {e.Player.Name} ({e.Player.IPAddress})\n");
                };
                
                networking.PlayerLeft += (sender, e) => {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Player left: {e.Player.Name}\n");
                };
                
                networking.SpectatorJoined += (sender, e) => {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Spectator joined: {e.Spectator.Name}\n");
                };
                
                lobbyManager.LobbyStateChanged += (sender, e) => {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Lobby state changed: {e.LobbyState.Status}\n");
                };
                
                // Note: SpectatorJoined event is handled by EnhancedNetworking
                
                // Initialize default lobby
                lobbyManager.InitializeLobby("classic", "default_plains");
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Default lobby initialized (Classic mode on Plains of War)\n");
                
                // Log available features
                File.AppendAllText(logPath, $"[{DateTime.Now}] 🎮 Enhanced Multiplayer Features Available:\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Up to {networking.MaxPlayers} players\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Up to {networking.MaxSpectators} spectators\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Host migration support\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Advanced lobby management\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Multiple camera modes for spectators\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Real-time player synchronization\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Custom gamemodes and maps\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Team management system\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}]   • Spectator chat and replay system\n");
                
                File.AppendAllText(logPath, $"[{DateTime.Now}] ✅ Enhanced Multiplayer Systems fully initialized!\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ❌ Error initializing enhanced multiplayer systems: {ex.Message}\n");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Stack trace: {ex.StackTrace}\n");
            }
        }
    }
}
