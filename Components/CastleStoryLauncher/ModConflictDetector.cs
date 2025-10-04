using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CastleStoryLauncher
{
    public class ModConflict
    {
        public string Type { get; set; } = string.Empty; // FileConflict, MemoryConflict, DependencyConflict
        public string Severity { get; set; } = string.Empty; // High, Medium, Low
        public List<string> AffectedMods { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public List<string> ConflictingResources { get; set; } = new List<string>();
    }

    public class ModDependency
    {
        public string ModName { get; set; } = string.Empty;
        public List<string> Dependencies { get; set; } = new List<string>();
        public List<string> OptionalDependencies { get; set; } = new List<string>();
        public List<string> Conflicts { get; set; } = new List<string>();
        public string Version { get; set; } = "1.0.0";
    }

    public class ModMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ModifiedFiles { get; set; } = new List<string>();
        public List<string> MemoryPatches { get; set; } = new List<string>();
        public ModDependency? Dependencies { get; set; }
        public int Priority { get; set; } = 100; // Lower number = higher priority
    }

    public class ModConflictDetector
    {
        private readonly Dictionary<string, ModMetadata> loadedMods;
        private readonly List<ModConflict> detectedConflicts;
        private readonly string modsDirectory;

        public ModConflictDetector(string modsDirectory)
        {
            this.modsDirectory = modsDirectory;
            loadedMods = new Dictionary<string, ModMetadata>();
            detectedConflicts = new List<ModConflict>();
        }

        public bool LoadModMetadata(string modPath)
        {
            try
            {
                string metadataFile = Path.Combine(modPath, "mod.json");
                if (!File.Exists(metadataFile))
                    return false;

                string json = File.ReadAllText(metadataFile);
                var metadata = JsonSerializer.Deserialize<ModMetadata>(json);

                if (metadata != null && !string.IsNullOrEmpty(metadata.Name))
                {
                    loadedMods[metadata.Name] = metadata;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void LoadAllMods()
        {
            if (!Directory.Exists(modsDirectory))
                return;

            var modDirs = Directory.GetDirectories(modsDirectory);
            foreach (var modDir in modDirs)
            {
                LoadModMetadata(modDir);
            }
        }

        public List<ModConflict> DetectAllConflicts()
        {
            detectedConflicts.Clear();

            DetectFileConflicts();
            DetectMemoryConflicts();
            DetectDependencyConflicts();
            DetectLoadOrderConflicts();

            return new List<ModConflict>(detectedConflicts);
        }

        private void DetectFileConflicts()
        {
            var fileModifications = new Dictionary<string, List<string>>();

            foreach (var mod in loadedMods)
            {
                foreach (var file in mod.Value.ModifiedFiles)
                {
                    if (!fileModifications.ContainsKey(file))
                    {
                        fileModifications[file] = new List<string>();
                    }
                    fileModifications[file].Add(mod.Key);
                }
            }

            foreach (var file in fileModifications.Where(f => f.Value.Count > 1))
            {
                detectedConflicts.Add(new ModConflict
                {
                    Type = "FileConflict",
                    Severity = "High",
                    AffectedMods = new List<string>(file.Value),
                    Description = $"Multiple mods modify the same file: {file.Key}",
                    Resolution = "Only one mod can modify this file at a time. Disable conflicting mods or apply them in a specific order.",
                    ConflictingResources = new List<string> { file.Key }
                });
            }
        }

        private void DetectMemoryConflicts()
        {
            var memoryPatches = new Dictionary<string, List<string>>();

            foreach (var mod in loadedMods)
            {
                foreach (var patch in mod.Value.MemoryPatches)
                {
                    if (!memoryPatches.ContainsKey(patch))
                    {
                        memoryPatches[patch] = new List<string>();
                    }
                    memoryPatches[patch].Add(mod.Key);
                }
            }

            foreach (var patch in memoryPatches.Where(p => p.Value.Count > 1))
            {
                detectedConflicts.Add(new ModConflict
                {
                    Type = "MemoryConflict",
                    Severity = "Medium",
                    AffectedMods = new List<string>(patch.Value),
                    Description = $"Multiple mods patch the same memory location: {patch.Key}",
                    Resolution = "Memory patches may conflict. Test carefully or use only one mod.",
                    ConflictingResources = new List<string> { patch.Key }
                });
            }
        }

        private void DetectDependencyConflicts()
        {
            foreach (var mod in loadedMods)
            {
                if (mod.Value.Dependencies == null)
                    continue;

                // Check for missing dependencies
                foreach (var dependency in mod.Value.Dependencies.Dependencies)
                {
                    if (!loadedMods.ContainsKey(dependency))
                    {
                        detectedConflicts.Add(new ModConflict
                        {
                            Type = "DependencyConflict",
                            Severity = "High",
                            AffectedMods = new List<string> { mod.Key },
                            Description = $"Mod '{mod.Key}' requires '{dependency}' which is not loaded",
                            Resolution = $"Install and enable the '{dependency}' mod",
                            ConflictingResources = new List<string> { dependency }
                        });
                    }
                }

                // Check for declared conflicts
                foreach (var conflict in mod.Value.Dependencies.Conflicts)
                {
                    if (loadedMods.ContainsKey(conflict))
                    {
                        detectedConflicts.Add(new ModConflict
                        {
                            Type = "DependencyConflict",
                            Severity = "High",
                            AffectedMods = new List<string> { mod.Key, conflict },
                            Description = $"Mod '{mod.Key}' is incompatible with '{conflict}'",
                            Resolution = $"Disable either '{mod.Key}' or '{conflict}'",
                            ConflictingResources = new List<string> { mod.Key, conflict }
                        });
                    }
                }

                // Check for version conflicts
                foreach (var dependency in mod.Value.Dependencies.Dependencies)
                {
                    if (loadedMods.ContainsKey(dependency))
                    {
                        // Simplified version check
                        var requiredVersion = mod.Value.Dependencies.Version;
                        var actualVersion = loadedMods[dependency].Version;

                        if (!string.IsNullOrEmpty(requiredVersion) && requiredVersion != actualVersion)
                        {
                            detectedConflicts.Add(new ModConflict
                            {
                                Type = "DependencyConflict",
                                Severity = "Medium",
                                AffectedMods = new List<string> { mod.Key, dependency },
                                Description = $"Mod '{mod.Key}' requires '{dependency}' version {requiredVersion}, but version {actualVersion} is loaded",
                                Resolution = "Update the dependency mod to the required version",
                                ConflictingResources = new List<string> { dependency }
                            });
                        }
                    }
                }
            }
        }

        private void DetectLoadOrderConflicts()
        {
            // Check if mods with dependencies are loaded in the correct order
            var sortedMods = GetRecommendedLoadOrder();
            var currentOrder = loadedMods.Keys.ToList();

            foreach (var mod in loadedMods)
            {
                if (mod.Value.Dependencies == null)
                    continue;

                foreach (var dependency in mod.Value.Dependencies.Dependencies)
                {
                    if (loadedMods.ContainsKey(dependency))
                    {
                        int modIndex = currentOrder.IndexOf(mod.Key);
                        int depIndex = currentOrder.IndexOf(dependency);

                        if (modIndex < depIndex)
                        {
                            detectedConflicts.Add(new ModConflict
                            {
                                Type = "LoadOrderConflict",
                                Severity = "Low",
                                AffectedMods = new List<string> { mod.Key, dependency },
                                Description = $"Mod '{mod.Key}' is loaded before its dependency '{dependency}'",
                                Resolution = "Reorder mods so dependencies are loaded first",
                                ConflictingResources = new List<string> { mod.Key, dependency }
                            });
                        }
                    }
                }
            }
        }

        public List<string> GetRecommendedLoadOrder()
        {
            var sorted = new List<string>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            void Visit(string modName)
            {
                if (visited.Contains(modName))
                    return;

                if (visiting.Contains(modName))
                {
                    // Circular dependency detected
                    return;
                }

                visiting.Add(modName);

                if (loadedMods.ContainsKey(modName) && loadedMods[modName].Dependencies != null)
                {
                    foreach (var dependency in loadedMods[modName].Dependencies.Dependencies)
                    {
                        if (loadedMods.ContainsKey(dependency))
                        {
                            Visit(dependency);
                        }
                    }
                }

                visiting.Remove(modName);
                visited.Add(modName);
                sorted.Add(modName);
            }

            // Sort by priority first
            var prioritySorted = loadedMods.OrderBy(m => m.Value.Priority).Select(m => m.Key).ToList();

            foreach (var modName in prioritySorted)
            {
                Visit(modName);
            }

            return sorted;
        }

        public List<ModConflict> GetConflictsBySeverity(string severity)
        {
            return detectedConflicts
                .Where(c => c.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<ModConflict> GetConflictsForMod(string modName)
        {
            return detectedConflicts
                .Where(c => c.AffectedMods.Contains(modName))
                .ToList();
        }

        public bool HasConflicts()
        {
            return detectedConflicts.Count > 0;
        }

        public bool HasCriticalConflicts()
        {
            return detectedConflicts.Any(c => c.Severity.Equals("High", StringComparison.OrdinalIgnoreCase));
        }

        public Dictionary<string, int> GetConflictStatistics()
        {
            var stats = new Dictionary<string, int>();

            stats["Total Conflicts"] = detectedConflicts.Count;
            stats["High Severity"] = detectedConflicts.Count(c => c.Severity == "High");
            stats["Medium Severity"] = detectedConflicts.Count(c => c.Severity == "Medium");
            stats["Low Severity"] = detectedConflicts.Count(c => c.Severity == "Low");
            stats["File Conflicts"] = detectedConflicts.Count(c => c.Type == "FileConflict");
            stats["Memory Conflicts"] = detectedConflicts.Count(c => c.Type == "MemoryConflict");
            stats["Dependency Conflicts"] = detectedConflicts.Count(c => c.Type == "DependencyConflict");
            stats["Load Order Conflicts"] = detectedConflicts.Count(c => c.Type == "LoadOrderConflict");

            return stats;
        }

        public void GenerateConflictReport(string reportPath)
        {
            try
            {
                using (var writer = new StreamWriter(reportPath))
                {
                    writer.WriteLine("=== Mod Conflict Detection Report ===");
                    writer.WriteLine($"Generated: {DateTime.Now}");
                    writer.WriteLine($"Mods Loaded: {loadedMods.Count}");
                    writer.WriteLine($"Conflicts Found: {detectedConflicts.Count}");
                    writer.WriteLine();

                    var stats = GetConflictStatistics();
                    writer.WriteLine("=== Statistics ===");
                    foreach (var stat in stats)
                    {
                        writer.WriteLine($"{stat.Key}: {stat.Value}");
                    }
                    writer.WriteLine();

                    writer.WriteLine("=== High Severity Conflicts ===");
                    foreach (var conflict in GetConflictsBySeverity("High"))
                    {
                        WriteConflict(writer, conflict);
                    }

                    writer.WriteLine("=== Medium Severity Conflicts ===");
                    foreach (var conflict in GetConflictsBySeverity("Medium"))
                    {
                        WriteConflict(writer, conflict);
                    }

                    writer.WriteLine("=== Low Severity Conflicts ===");
                    foreach (var conflict in GetConflictsBySeverity("Low"))
                    {
                        WriteConflict(writer, conflict);
                    }

                    writer.WriteLine("=== Recommended Load Order ===");
                    var recommendedOrder = GetRecommendedLoadOrder();
                    for (int i = 0; i < recommendedOrder.Count; i++)
                    {
                        writer.WriteLine($"{i + 1}. {recommendedOrder[i]}");
                    }
                }
            }
            catch
            {
                // Ignore errors in report generation
            }
        }

        private void WriteConflict(StreamWriter writer, ModConflict conflict)
        {
            writer.WriteLine($"- [{conflict.Type}] {conflict.Description}");
            writer.WriteLine($"  Affected Mods: {string.Join(", ", conflict.AffectedMods)}");
            writer.WriteLine($"  Resolution: {conflict.Resolution}");
            if (conflict.ConflictingResources.Count > 0)
            {
                writer.WriteLine($"  Resources: {string.Join(", ", conflict.ConflictingResources)}");
            }
            writer.WriteLine();
        }

        public bool ExportConflictsToJson(string outputPath)
        {
            try
            {
                var data = new
                {
                    GeneratedAt = DateTime.Now,
                    ModsLoaded = loadedMods.Count,
                    ConflictsFound = detectedConflicts.Count,
                    Statistics = GetConflictStatistics(),
                    Conflicts = detectedConflicts,
                    RecommendedLoadOrder = GetRecommendedLoadOrder()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(outputPath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetModsWithoutConflicts()
        {
            var modsWithConflicts = detectedConflicts
                .SelectMany(c => c.AffectedMods)
                .Distinct()
                .ToList();

            return loadedMods.Keys
                .Where(mod => !modsWithConflicts.Contains(mod))
                .ToList();
        }
    }
}

