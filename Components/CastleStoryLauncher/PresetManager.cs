using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CastleStoryLauncher
{
    public class GamemodePreset
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GamemodeType { get; set; } = string.Empty; // sandbox, invasion, conquest
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Author { get; set; } = "User";
    }

    public class PresetManager
    {
        private readonly string presetsDirectory;
        private readonly List<GamemodePreset> presets;

        public PresetManager(string gameDirectory)
        {
            presetsDirectory = Path.Combine(gameDirectory, "Info", "Presets");
            presets = new List<GamemodePreset>();
            EnsurePresetsDirectoryExists();
            LoadPresets();
            CreateDefaultPresets();
        }

        private void EnsurePresetsDirectoryExists()
        {
            if (!Directory.Exists(presetsDirectory))
            {
                Directory.CreateDirectory(presetsDirectory);
            }
        }

        private void CreateDefaultPresets()
        {
            if (presets.Count == 0)
            {
                // Easy Mode Presets
                presets.Add(new GamemodePreset
                {
                    Name = "Easy Survival",
                    Description = "Relaxed difficulty for beginners",
                    GamemodeType = "invasion",
                    Settings = new Dictionary<string, object>
                    {
                        { "startingResources", 1000 },
                        { "enemyDifficulty", 0.5 },
                        { "waveDelay", 600 },
                        { "buildPhaseTime", 300 }
                    }
                });

                presets.Add(new GamemodePreset
                {
                    Name = "Normal Survival",
                    Description = "Balanced gameplay",
                    GamemodeType = "invasion",
                    Settings = new Dictionary<string, object>
                    {
                        { "startingResources", 500 },
                        { "enemyDifficulty", 1.0 },
                        { "waveDelay", 300 },
                        { "buildPhaseTime", 180 }
                    }
                });

                presets.Add(new GamemodePreset
                {
                    Name = "Hard Survival",
                    Description = "Challenging gameplay for veterans",
                    GamemodeType = "invasion",
                    Settings = new Dictionary<string, object>
                    {
                        { "startingResources", 250 },
                        { "enemyDifficulty", 1.5 },
                        { "waveDelay", 180 },
                        { "buildPhaseTime", 120 }
                    }
                });

                presets.Add(new GamemodePreset
                {
                    Name = "Creative Sandbox",
                    Description = "Unlimited resources for building",
                    GamemodeType = "sandbox",
                    Settings = new Dictionary<string, object>
                    {
                        { "unlimitedResources", true },
                        { "noEnemies", true },
                        { "instantBuild", true },
                        { "godMode", true }
                    }
                });

                presets.Add(new GamemodePreset
                {
                    Name = "Competitive Conquest",
                    Description = "Balanced team vs team gameplay",
                    GamemodeType = "conquest",
                    Settings = new Dictionary<string, object>
                    {
                        { "startingResources", 750 },
                        { "captureTime", 60 },
                        { "respawnTime", 30 },
                        { "maxPlayers", 16 }
                    }
                });

                SaveAllPresets();
            }
        }

        public List<GamemodePreset> GetPresets()
        {
            return new List<GamemodePreset>(presets);
        }

        public List<GamemodePreset> GetPresetsByGamemode(string gamemodeType)
        {
            return presets.FindAll(p => p.GamemodeType.Equals(gamemodeType, StringComparison.OrdinalIgnoreCase));
        }

        public GamemodePreset? GetPresetByName(string name)
        {
            return presets.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool SavePreset(GamemodePreset preset)
        {
            try
            {
                var existingPreset = presets.Find(p => p.Name.Equals(preset.Name, StringComparison.OrdinalIgnoreCase));
                if (existingPreset != null)
                {
                    presets.Remove(existingPreset);
                }

                presets.Add(preset);

                string filePath = Path.Combine(presetsDirectory, $"{preset.Name}.preset.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(preset, options);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeletePreset(string name)
        {
            try
            {
                var preset = presets.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (preset != null)
                {
                    presets.Remove(preset);
                    string filePath = Path.Combine(presetsDirectory, $"{name}.preset.json");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ExportPreset(string name, string exportPath)
        {
            try
            {
                var preset = GetPresetByName(name);
                if (preset != null)
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(preset, options);
                    File.WriteAllText(exportPath, json);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ImportPreset(string importPath)
        {
            try
            {
                string json = File.ReadAllText(importPath);
                var preset = JsonSerializer.Deserialize<GamemodePreset>(json);
                if (preset != null)
                {
                    return SavePreset(preset);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void LoadPresets()
        {
            try
            {
                if (Directory.Exists(presetsDirectory))
                {
                    var presetFiles = Directory.GetFiles(presetsDirectory, "*.preset.json");
                    foreach (var file in presetFiles)
                    {
                        try
                        {
                            string json = File.ReadAllText(file);
                            var preset = JsonSerializer.Deserialize<GamemodePreset>(json);
                            if (preset != null)
                            {
                                presets.Add(preset);
                            }
                        }
                        catch
                        {
                            // Skip invalid preset files
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors during preset loading
            }
        }

        private void SaveAllPresets()
        {
            foreach (var preset in presets)
            {
                SavePreset(preset);
            }
        }

        public GamemodePreset CreateCustomPreset(string name, string description, string gamemodeType)
        {
            return new GamemodePreset
            {
                Name = name,
                Description = description,
                GamemodeType = gamemodeType,
                Settings = new Dictionary<string, object>(),
                CreatedDate = DateTime.Now,
                Author = Environment.UserName
            };
        }

        public bool ApplyPresetToConfig(GamemodePreset preset, string configFilePath)
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                    return false;
                }

                // Read the config file
                string content = File.ReadAllText(configFilePath);

                // Create backup
                string backupPath = configFilePath + ".backup";
                File.Copy(configFilePath, backupPath, true);

                // Apply preset settings to the config file
                foreach (var setting in preset.Settings)
                {
                    // Simple find-replace for Lua config files
                    // This is a basic implementation and may need enhancement
                    string pattern = $"{setting.Key} = ";
                    if (content.Contains(pattern))
                    {
                        // Find and replace the value
                        int start = content.IndexOf(pattern) + pattern.Length;
                        int end = content.IndexOf('\n', start);
                        if (end > start)
                        {
                            string oldValue = content.Substring(start, end - start).Trim();
                            string newValue = setting.Value.ToString() ?? "";
                            content = content.Replace(pattern + oldValue, pattern + newValue);
                        }
                    }
                }

                // Write the modified config
                File.WriteAllText(configFilePath, content);
                return true;
            }
            catch
            {
                // Restore from backup if something went wrong
                try
                {
                    string backupPath = configFilePath + ".backup";
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, configFilePath, true);
                    }
                }
                catch
                {
                    // Ignore backup restoration errors
                }
                return false;
            }
        }
    }
}

