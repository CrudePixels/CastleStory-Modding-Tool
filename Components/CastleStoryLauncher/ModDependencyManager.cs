using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CastleStoryLauncher
{
    // Using existing ModDependency and ModMetadata classes from ModConflictDetector.cs

    public class DependencyConflict
    {
        public string Type { get; set; } = ""; // "Missing", "Version", "Conflict"
        public string ModId { get; set; } = "";
        public string Message { get; set; } = "";
        public string Severity { get; set; } = "Error"; // "Error", "Warning", "Info"
    }

    public class ModDependencyManager
    {
        private readonly Dictionary<string, ModMetadata> modMetadata = new Dictionary<string, ModMetadata>();
        private readonly string modsDirectory;

        public ModDependencyManager(string modsDirectory)
        {
            this.modsDirectory = modsDirectory;
            LoadModMetadata();
        }

        public List<ModMetadata> GetAllMods()
        {
            return modMetadata.Values.ToList();
        }

        public ModMetadata? GetModMetadata(string modId)
        {
            return modMetadata.TryGetValue(modId, out var metadata) ? metadata : null;
        }

        public List<DependencyConflict> ValidateModDependencies(List<string> enabledModIds)
        {
            var conflicts = new List<DependencyConflict>();
            var installedMods = new HashSet<string>(modMetadata.Keys);

            foreach (var modId in enabledModIds)
            {
                if (!modMetadata.TryGetValue(modId, out var mod))
                    continue;

                // Check dependencies
                if (mod.Dependencies != null)
                {
                    foreach (var dependency in mod.Dependencies.Dependencies)
                    {
                        if (!installedMods.Contains(dependency))
                        {
                            conflicts.Add(new DependencyConflict
                            {
                                Type = "Missing",
                                ModId = modId,
                                Message = $"Missing required dependency: {dependency}",
                                Severity = "Error"
                            });
                        }
                    }

                    // Check optional dependencies
                    foreach (var dependency in mod.Dependencies.OptionalDependencies)
                    {
                        if (!installedMods.Contains(dependency))
                        {
                            conflicts.Add(new DependencyConflict
                            {
                                Type = "Missing",
                                ModId = modId,
                                Message = $"Missing optional dependency: {dependency}",
                                Severity = "Warning"
                            });
                        }
                    }

                    // Check conflicts
                    foreach (var conflictModId in mod.Dependencies.Conflicts)
                    {
                        if (enabledModIds.Contains(conflictModId))
                        {
                            conflicts.Add(new DependencyConflict
                            {
                                Type = "Conflict",
                                ModId = modId,
                                Message = $"Mod conflicts with: {conflictModId}",
                                Severity = "Error"
                            });
                        }
                    }
                }
            }

            return conflicts;
        }

        public List<string> ResolveLoadOrder(List<string> enabledModIds)
        {
            var loadOrder = new List<string>();
            var processed = new HashSet<string>();
            var processing = new HashSet<string>();

            foreach (var modId in enabledModIds)
            {
                if (!processed.Contains(modId))
                {
                    ResolveModDependencies(modId, enabledModIds, loadOrder, processed, processing);
                }
            }

            return loadOrder;
        }

        private void ResolveModDependencies(string modId, List<string> enabledModIds, 
            List<string> loadOrder, HashSet<string> processed, HashSet<string> processing)
        {
            if (processing.Contains(modId))
            {
                throw new InvalidOperationException($"Circular dependency detected involving mod: {modId}");
            }

            if (processed.Contains(modId))
                return;

            processing.Add(modId);

            if (modMetadata.TryGetValue(modId, out var mod) && mod.Dependencies != null)
            {
                // Process dependencies first
                foreach (var dependency in mod.Dependencies.Dependencies)
                {
                    if (enabledModIds.Contains(dependency))
                    {
                        ResolveModDependencies(dependency, enabledModIds, loadOrder, processed, processing);
                    }
                }
            }

            processing.Remove(modId);
            processed.Add(modId);
            loadOrder.Add(modId);
        }

        public List<string> GetRequiredMods(string modId)
        {
            var required = new List<string>();
            var processed = new HashSet<string>();

            GetRequiredModsRecursive(modId, required, processed);
            return required;
        }

        private void GetRequiredModsRecursive(string modId, List<string> required, HashSet<string> processed)
        {
            if (processed.Contains(modId))
                return;

            processed.Add(modId);

            if (modMetadata.TryGetValue(modId, out var mod) && mod.Dependencies != null)
            {
                foreach (var dependency in mod.Dependencies.Dependencies)
                {
                    if (!required.Contains(dependency))
                    {
                        required.Add(dependency);
                        GetRequiredModsRecursive(dependency, required, processed);
                    }
                }
            }
        }

        public List<string> GetModsByAuthor(string author)
        {
            return modMetadata.Values
                .Where(m => m.Author.Equals(author, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.Name)
                .ToList();
        }

        public List<string> GetAuthors()
        {
            return modMetadata.Values
                .Select(m => m.Author)
                .Distinct()
                .OrderBy(a => a)
                .ToList();
        }

        private void LoadModMetadata()
        {
            if (!Directory.Exists(modsDirectory))
                return;

            var modDirectories = Directory.GetDirectories(modsDirectory);
            
            foreach (var modDir in modDirectories)
            {
                var modJsonPath = Path.Combine(modDir, "mod.json");
                if (File.Exists(modJsonPath))
                {
                    try
                    {
                        var jsonContent = File.ReadAllText(modJsonPath);
                        var modData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
                        
                if (modData != null && modData.ContainsKey("name"))
                {
                    var metadata = ParseModMetadata(modData, modDir);
                    if (metadata != null)
                    {
                        modMetadata[metadata.Name] = metadata;
                    }
                }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading mod metadata from {modJsonPath}: {ex.Message}");
                    }
                }
            }
        }

        private ModMetadata? ParseModMetadata(Dictionary<string, object> modData, string modDir)
        {
            try
            {
                var metadata = new ModMetadata
                {
                    Name = GetStringValue(modData, "name") ?? Path.GetFileName(modDir),
                    Version = GetStringValue(modData, "version") ?? "1.0.0",
                    Author = GetStringValue(modData, "author") ?? "Unknown",
                    Description = GetStringValue(modData, "description") ?? ""
                };

                // Parse dependencies
                var dependencies = new ModDependency
                {
                    ModName = metadata.Name
                };

                // Parse required dependencies
                if (modData.TryGetValue("dependencies", out var depsObj) && depsObj is JsonElement depsElement)
                {
                    if (depsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var depElement in depsElement.EnumerateArray())
                        {
                            if (depElement.ValueKind == JsonValueKind.String)
                            {
                                var depId = depElement.GetString();
                                if (!string.IsNullOrEmpty(depId))
                                {
                                    dependencies.Dependencies.Add(depId);
                                }
                            }
                        }
                    }
                }

                // Parse optional dependencies
                if (modData.TryGetValue("optionalDependencies", out var optDepsObj) && optDepsObj is JsonElement optDepsElement)
                {
                    if (optDepsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var depElement in optDepsElement.EnumerateArray())
                        {
                            if (depElement.ValueKind == JsonValueKind.String)
                            {
                                var depId = depElement.GetString();
                                if (!string.IsNullOrEmpty(depId))
                                {
                                    dependencies.OptionalDependencies.Add(depId);
                                }
                            }
                        }
                    }
                }

                // Parse conflicts
                if (modData.TryGetValue("conflicts", out var conflictsObj) && conflictsObj is JsonElement conflictsElement)
                {
                    if (conflictsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var conflictElement in conflictsElement.EnumerateArray())
                        {
                            if (conflictElement.ValueKind == JsonValueKind.String)
                            {
                                var conflictId = conflictElement.GetString();
                                if (!string.IsNullOrEmpty(conflictId))
                                {
                                    dependencies.Conflicts.Add(conflictId);
                                }
                            }
                        }
                    }
                }

                metadata.Dependencies = dependencies;
                return metadata;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing mod metadata: {ex.Message}");
                return null;
            }
        }

        private string? GetStringValue(Dictionary<string, object> data, string key)
        {
            if (data.TryGetValue(key, out var value))
            {
                return value?.ToString();
            }
            return null;
        }

        private bool IsVersionCompatible(string installedVersion, string requiredVersion)
        {
            try
            {
                // Simple version comparison - can be enhanced with semantic versioning
                if (string.IsNullOrEmpty(requiredVersion) || requiredVersion == "*")
                    return true;

                var installed = ParseVersion(installedVersion);
                var required = ParseVersion(requiredVersion);

                return CompareVersions(installed, required) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private int[] ParseVersion(string version)
        {
            return version.Split('.')
                .Select(v => int.TryParse(v, out var num) ? num : 0)
                .ToArray();
        }

        private int CompareVersions(int[] version1, int[] version2)
        {
            var maxLength = Math.Max(version1.Length, version2.Length);
            
            for (int i = 0; i < maxLength; i++)
            {
                var v1 = i < version1.Length ? version1[i] : 0;
                var v2 = i < version2.Length ? version2[i] : 0;
                
                if (v1 < v2) return -1;
                if (v1 > v2) return 1;
            }
            
            return 0;
        }

        public void RefreshModMetadata()
        {
            modMetadata.Clear();
            LoadModMetadata();
        }
    }
}
