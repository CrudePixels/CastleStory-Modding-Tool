using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CastleStoryLauncher
{
    public class MemoryPatchConfig
    {
        public class PatchPattern
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public byte[] SearchPattern { get; set; } = Array.Empty<byte>();
            public byte[] ReplacementPattern { get; set; } = Array.Empty<byte>();
            public bool Enabled { get; set; }
            public string Category { get; set; } = string.Empty;
        }

        public class PatchCategory
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<PatchPattern> Patterns { get; set; } = new List<PatchPattern>();
        }

        public List<PatchCategory> Categories { get; set; } = new List<PatchCategory>();
        public bool EnableAllPatches { get; set; } = true;
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info";

        public static MemoryPatchConfig LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<MemoryPatchConfig>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading memory patch config: {ex.Message}");
            }

            return CreateDefaultConfig();
        }

        public static MemoryPatchConfig CreateDefaultConfig()
        {
            var config = new MemoryPatchConfig();

            // Player Limits Category
            var playerCategory = new PatchCategory
            {
                Name = "Player Limits",
                Description = "Configure maximum player counts and team limits"
            };

            playerCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Players",
                Description = "Increase maximum player count from 4 to 32",
                SearchPattern = new byte[] { 0x04, 0x00, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0x20, 0x00, 0x00, 0x00 },
                Enabled = true,
                Category = "Player Limits"
            });

            playerCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Teams",
                Description = "Increase maximum team count from 2 to 8",
                SearchPattern = new byte[] { 0x02, 0x00, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0x08, 0x00, 0x00, 0x00 },
                Enabled = true,
                Category = "Player Limits"
            });

            config.Categories.Add(playerCategory);

            // Bricktron Limits Category
            var bricktronCategory = new PatchCategory
            {
                Name = "Bricktron Limits",
                Description = "Configure maximum bricktron counts and unit limits"
            };

            bricktronCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Bricktron",
                Description = "Increase maximum bricktron count from 15 to 100",
                SearchPattern = new byte[] { 0x0F, 0x00, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0x64, 0x00, 0x00, 0x00 },
                Enabled = true,
                Category = "Bricktron Limits"
            });

            bricktronCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Workers",
                Description = "Increase maximum worker count from 10 to 50",
                SearchPattern = new byte[] { 0x0A, 0x00, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0x32, 0x00, 0x00, 0x00 },
                Enabled = true,
                Category = "Bricktron Limits"
            });

            config.Categories.Add(bricktronCategory);

            // Resource Limits Category
            var resourceCategory = new PatchCategory
            {
                Name = "Resource Limits",
                Description = "Configure maximum resource counts and storage limits"
            };

            resourceCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Resources",
                Description = "Increase maximum resource count from 1000 to 5000",
                SearchPattern = new byte[] { 0xE8, 0x03, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0x88, 0x13, 0x00, 0x00 },
                Enabled = true,
                Category = "Resource Limits"
            });

            resourceCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Storage",
                Description = "Increase maximum storage capacity from 500 to 2000",
                SearchPattern = new byte[] { 0xF4, 0x01, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0xD0, 0x07, 0x00, 0x00 },
                Enabled = true,
                Category = "Resource Limits"
            });

            config.Categories.Add(resourceCategory);

            // Gameplay Limits Category
            var gameplayCategory = new PatchCategory
            {
                Name = "Gameplay Limits",
                Description = "Configure gameplay mechanics and limits"
            };

            gameplayCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Building Height",
                Description = "Increase maximum building height from 50 to 200",
                SearchPattern = new byte[] { 0x32, 0x00, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0xC8, 0x00, 0x00, 0x00 },
                Enabled = true,
                Category = "Gameplay Limits"
            });

            gameplayCategory.Patterns.Add(new PatchPattern
            {
                Name = "Max Map Size",
                Description = "Increase maximum map size from 256 to 512",
                SearchPattern = new byte[] { 0x00, 0x01, 0x00, 0x00 },
                ReplacementPattern = new byte[] { 0x00, 0x02, 0x00, 0x00 },
                Enabled = false,
                Category = "Gameplay Limits"
            });

            config.Categories.Add(gameplayCategory);

            return config;
        }

        public void SaveToFile(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving memory patch config: {ex.Message}");
            }
        }

        public List<PatchPattern> GetEnabledPatterns()
        {
            var enabledPatterns = new List<PatchPattern>();
            
            foreach (var category in Categories)
            {
                foreach (var pattern in category.Patterns)
                {
                    if (pattern.Enabled && EnableAllPatches)
                    {
                        enabledPatterns.Add(pattern);
                    }
                }
            }
            
            return enabledPatterns;
        }

        public List<PatchPattern> GetPatternsByCategory(string categoryName)
        {
            var patterns = new List<PatchPattern>();
            
            foreach (var category in Categories)
            {
                if (category.Name == categoryName)
                {
                    patterns.AddRange(category.Patterns);
                }
            }
            
            return patterns;
        }

        public void EnablePattern(string patternName, bool enabled)
        {
            foreach (var category in Categories)
            {
                foreach (var pattern in category.Patterns)
                {
                    if (pattern.Name == patternName)
                    {
                        pattern.Enabled = enabled;
                        return;
                    }
                }
            }
        }

        public void EnableCategory(string categoryName, bool enabled)
        {
            foreach (var category in Categories)
            {
                if (category.Name == categoryName)
                {
                    foreach (var pattern in category.Patterns)
                    {
                        pattern.Enabled = enabled;
                    }
                    return;
                }
            }
        }
    }
}
