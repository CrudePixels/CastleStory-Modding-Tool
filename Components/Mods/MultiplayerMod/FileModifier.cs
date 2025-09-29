using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MultiplayerMod
{
    /// <summary>
    /// File-based modification approach for Castle Story multiplayer limits
    /// This modifies game configuration files instead of runtime memory patching
    /// </summary>
    public class FileModifier
    {
        private string gameDirectory;
        private string modDirectory;
        
        public FileModifier(string gameDir, string modDir)
        {
            gameDirectory = gameDir;
            modDirectory = modDir;
        }
        
        /// <summary>
        /// Apply multiplayer modifications by modifying game configuration files
        /// </summary>
        public bool ApplyModifications(int maxPlayers = 32, int maxTeams = 16)
        {
            try
            {
                Console.WriteLine("=== Castle Story File Modifier ===");
                Console.WriteLine($"Game Directory: {gameDirectory}");
                Console.WriteLine($"Mod Directory: {modDirectory}");
                Console.WriteLine($"Max Players: {maxPlayers}");
                Console.WriteLine($"Max Teams: {maxTeams}");
                
                // Create backup of original files
                CreateBackups();
                
                // Modify game configuration files
                bool success = true;
                success &= ModifyGameConfig(maxPlayers, maxTeams);
                success &= ModifyLuaFiles(maxPlayers, maxTeams);
                success &= ModifyUnityConfig(maxPlayers, maxTeams);
                success &= CreateModConfig(maxPlayers, maxTeams);
                
                if (success)
                {
                    Console.WriteLine("✅ All modifications applied successfully!");
                    LogModification("File modifications completed", $"MaxPlayers: {maxPlayers}, MaxTeams: {maxTeams}");
                }
                else
                {
                    Console.WriteLine("❌ Some modifications failed!");
                    LogModification("File modifications failed", "Check logs for details");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying modifications: {ex.Message}");
                LogModification("Error", ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Create backups of original files before modification
        /// </summary>
        private void CreateBackups()
        {
            try
            {
                var backupDir = Path.Combine(modDirectory, "backups");
                Directory.CreateDirectory(backupDir);
                
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupSubDir = Path.Combine(backupDir, $"backup_{timestamp}");
                Directory.CreateDirectory(backupSubDir);
                
                // Backup common game config files
                var filesToBackup = new[]
                {
                    "CastleStory_Data/Resources/GameConfig.asset",
                    "CastleStory_Data/Resources/GameSettings.asset",
                    "CastleStory_Data/StreamingAssets/game_config.lua",
                    "CastleStory_Data/StreamingAssets/multiplayer_config.lua",
                    "CastleStory_Data/StreamingAssets/settings.lua"
                };
                
                foreach (var file in filesToBackup)
                {
                    var sourcePath = Path.Combine(gameDirectory, file);
                    if (File.Exists(sourcePath))
                    {
                        var destPath = Path.Combine(backupSubDir, Path.GetFileName(file));
                        File.Copy(sourcePath, destPath, true);
                        Console.WriteLine($"📁 Backed up: {file}");
                    }
                }
                
                Console.WriteLine($"✅ Backups created in: {backupSubDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Warning: Could not create backups: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Modify Unity game configuration files
        /// </summary>
        private bool ModifyGameConfig(int maxPlayers, int maxTeams)
        {
            try
            {
                var configPath = Path.Combine(gameDirectory, "CastleStory_Data", "Resources", "GameConfig.asset");
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("⚠️ GameConfig.asset not found, creating new one...");
                    CreateGameConfigAsset(configPath, maxPlayers, maxTeams);
                    return true;
                }
                
                var content = File.ReadAllText(configPath);
                var modified = false;
                
                // Modify max players
                content = Regex.Replace(content, 
                    @"m_MaxPlayers:\s*(\d+)", 
                    $"m_MaxPlayers: {maxPlayers}", 
                    RegexOptions.IgnoreCase);
                
                // Modify max teams
                content = Regex.Replace(content, 
                    @"m_MaxTeams:\s*(\d+)", 
                    $"m_MaxTeams: {maxTeams}", 
                    RegexOptions.IgnoreCase);
                
                // Add multiplayer settings if they don't exist
                if (!content.Contains("m_MaxPlayers:"))
                {
                    content = content.Replace("--- !u!114 &11400000",
                        $"m_MaxPlayers: {maxPlayers}\n  m_MaxTeams: {maxTeams}\n--- !u!114 &11400000");
                    modified = true;
                }
                
                if (modified || content != File.ReadAllText(configPath))
                {
                    File.WriteAllText(configPath, content);
                    Console.WriteLine($"✅ Modified GameConfig.asset: MaxPlayers={maxPlayers}, MaxTeams={maxTeams}");
                    return true;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error modifying GameConfig.asset: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Create a new GameConfig.asset file
        /// </summary>
        private void CreateGameConfigAsset(string path, int maxPlayers, int maxTeams)
        {
            var content = $@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 0000000000000000000000000000000000, type: 3}}
  m_Name: GameConfig
  m_MaxPlayers: {maxPlayers}
  m_MaxTeams: {maxTeams}
  m_EnableMultiplayer: 1
  m_EnableMapSync: 1
  m_EnableGamemodeSync: 1
  m_DefaultPort: 7777
  m_EnableHostMigration: 1
  m_EnableFileIntegrityCheck: 1
";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }
        
        /// <summary>
        /// Modify Lua configuration files
        /// </summary>
        private bool ModifyLuaFiles(int maxPlayers, int maxTeams)
        {
            try
            {
                var luaFiles = new[]
                {
                    "CastleStory_Data/StreamingAssets/game_config.lua",
                    "CastleStory_Data/StreamingAssets/multiplayer_config.lua",
                    "CastleStory_Data/StreamingAssets/settings.lua"
                };
                
                bool success = true;
                foreach (var file in luaFiles)
                {
                    success &= ModifyLuaFile(file, maxPlayers, maxTeams);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error modifying Lua files: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Modify a specific Lua file
        /// </summary>
        private bool ModifyLuaFile(string relativePath, int maxPlayers, int maxTeams)
        {
            try
            {
                var fullPath = Path.Combine(gameDirectory, relativePath);
                
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"⚠️ {relativePath} not found, creating new one...");
                    CreateLuaConfig(fullPath, maxPlayers, maxTeams);
                    return true;
                }
                
                var content = File.ReadAllText(fullPath);
                var originalContent = content;
                
                // Modify max players
                content = Regex.Replace(content, 
                    @"maxPlayers\s*=\s*\d+", 
                    $"maxPlayers = {maxPlayers}", 
                    RegexOptions.IgnoreCase);
                
                // Modify max teams
                content = Regex.Replace(content, 
                    @"maxTeams\s*=\s*\d+", 
                    $"maxTeams = {maxTeams}", 
                    RegexOptions.IgnoreCase);
                
                // Add settings if they don't exist
                if (!content.Contains("maxPlayers"))
                {
                    var settingsBlock = $@"
-- Multiplayer Mod Settings
maxPlayers = {maxPlayers}
maxTeams = {maxTeams}
enableMapSync = true
enableGamemodeSync = true
defaultPort = 7777
enableHostMigration = true
enableFileIntegrityCheck = true
";
                    content += settingsBlock;
                }
                
                if (content != originalContent)
                {
                    File.WriteAllText(fullPath, content);
                    Console.WriteLine($"✅ Modified {relativePath}: MaxPlayers={maxPlayers}, MaxTeams={maxTeams}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error modifying {relativePath}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Create a new Lua configuration file
        /// </summary>
        private void CreateLuaConfig(string path, int maxPlayers, int maxTeams)
        {
            var content = $@"-- Castle Story Multiplayer Configuration
-- Generated by MultiplayerMod on {DateTime.Now}

-- Player Limits
maxPlayers = {maxPlayers}
maxTeams = {maxTeams}

-- Multiplayer Features
enableMapSync = true
enableGamemodeSync = true
defaultPort = 7777
enableHostMigration = true
enableFileIntegrityCheck = true

-- Game Settings
gameMode = ""multiplayer""
difficulty = ""normal""
mapSize = ""large""

-- Network Settings
timeout = 30
heartbeatInterval = 5
maxPing = 200

-- Debug Settings
debugMode = false
logLevel = ""info""
";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }
        
        /// <summary>
        /// Modify Unity configuration files
        /// </summary>
        private bool ModifyUnityConfig(int maxPlayers, int maxTeams)
        {
            try
            {
                var configPath = Path.Combine(gameDirectory, "CastleStory_Data", "Resources", "GameSettings.asset");
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("⚠️ GameSettings.asset not found, creating new one...");
                    CreateGameSettingsAsset(configPath, maxPlayers, maxTeams);
                    return true;
                }
                
                var content = File.ReadAllText(configPath);
                var modified = false;
                
                // Modify multiplayer settings
                content = Regex.Replace(content, 
                    @"m_MultiplayerEnabled:\s*(\d+)", 
                    "m_MultiplayerEnabled: 1", 
                    RegexOptions.IgnoreCase);
                
                content = Regex.Replace(content, 
                    @"m_MaxPlayers:\s*(\d+)", 
                    $"m_MaxPlayers: {maxPlayers}", 
                    RegexOptions.IgnoreCase);
                
                content = Regex.Replace(content, 
                    @"m_MaxTeams:\s*(\d+)", 
                    $"m_MaxTeams: {maxTeams}", 
                    RegexOptions.IgnoreCase);
                
                if (modified || content != File.ReadAllText(configPath))
                {
                    File.WriteAllText(configPath, content);
                    Console.WriteLine($"✅ Modified GameSettings.asset: MaxPlayers={maxPlayers}, MaxTeams={maxTeams}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error modifying GameSettings.asset: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Create a new GameSettings.asset file
        /// </summary>
        private void CreateGameSettingsAsset(string path, int maxPlayers, int maxTeams)
        {
            var content = $@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 0000000000000000000000000000000000, type: 3}}
  m_Name: GameSettings
  m_MultiplayerEnabled: 1
  m_MaxPlayers: {maxPlayers}
  m_MaxTeams: {maxTeams}
  m_EnableMapSync: 1
  m_EnableGamemodeSync: 1
  m_DefaultPort: 7777
  m_EnableHostMigration: 1
  m_EnableFileIntegrityCheck: 1
  m_NetworkTimeout: 30
  m_HeartbeatInterval: 5
  m_MaxPing: 200
";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }
        
        /// <summary>
        /// Create mod configuration file
        /// </summary>
        private bool CreateModConfig(int maxPlayers, int maxTeams)
        {
            try
            {
                var configPath = Path.Combine(modDirectory, "multiplayer_config.json");
                var config = new
                {
                    modName = "MultiplayerMod",
                    version = "1.0.0",
                    appliedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    settings = new
                    {
                        maxPlayers = maxPlayers,
                        maxTeams = maxTeams,
                        enableMapSync = true,
                        enableGamemodeSync = true,
                        defaultPort = 7777,
                        enableHostMigration = true,
                        enableFileIntegrityCheck = true
                    },
                    modifiedFiles = new[]
                    {
                        "CastleStory_Data/Resources/GameConfig.asset",
                        "CastleStory_Data/Resources/GameSettings.asset",
                        "CastleStory_Data/StreamingAssets/game_config.lua",
                        "CastleStory_Data/StreamingAssets/multiplayer_config.lua",
                        "CastleStory_Data/StreamingAssets/settings.lua"
                    }
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(configPath, json);
                Console.WriteLine($"✅ Created mod configuration: {configPath}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating mod config: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Log modification activity
        /// </summary>
        private void LogModification(string action, string details)
        {
            try
            {
                var logPath = Path.Combine(modDirectory, "modification_log.txt");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}: {details}\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }
        
        /// <summary>
        /// Restore original files from backup
        /// </summary>
        public bool RestoreFromBackup()
        {
            try
            {
                var backupDir = Path.Combine(modDirectory, "backups");
                if (!Directory.Exists(backupDir))
                {
                    Console.WriteLine("❌ No backup directory found!");
                    return false;
                }
                
                var backupDirs = Directory.GetDirectories(backupDir);
                if (backupDirs.Length == 0)
                {
                    Console.WriteLine("❌ No backups found!");
                    return false;
                }
                
                // Get the most recent backup
                var latestBackup = backupDirs.OrderByDescending(d => d).First();
                Console.WriteLine($"📁 Restoring from backup: {Path.GetFileName(latestBackup)}");
                
                var files = Directory.GetFiles(latestBackup);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var destPath = Path.Combine(gameDirectory, "CastleStory_Data", "Resources", fileName);
                    
                    if (fileName.EndsWith(".lua"))
                    {
                        destPath = Path.Combine(gameDirectory, "CastleStory_Data", "StreamingAssets", fileName);
                    }
                    
                    if (File.Exists(destPath))
                    {
                        File.Copy(file, destPath, true);
                        Console.WriteLine($"✅ Restored: {fileName}");
                    }
                }
                
                Console.WriteLine("✅ Restore completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error restoring from backup: {ex.Message}");
                return false;
            }
        }
    }
}
