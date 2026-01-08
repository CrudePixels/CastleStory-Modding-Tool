using System;
using System.IO;
using System.Text.Json;
using CastleStoryModdingTool.ModIntegrations;
using JasonsEnhancements;

namespace CastleStoryModdingTool.ModDefinitions
{
    public static class JasonsEnhancementsDefinition
    {
        public static IModIntegration CreateIntegration()
        {
            try
            {
                // Try to load from mod.json in the mod directory
                string modDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Components", "Mods", "JasonsEnhancements");
                string modJsonPath = Path.Combine(modDir, "mod.json");
                
                // Also try relative paths
                if (!File.Exists(modJsonPath))
                {
                    modJsonPath = Path.Combine("Components", "Mods", "JasonsEnhancements", "mod.json");
                }
                
                if (!File.Exists(modJsonPath))
                {
                    modJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Components", "Mods", "JasonsEnhancements", "mod.json");
                }
                
                // Default source path - relative to project root
                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                // Go up from bin/Debug or bin/Release to project root
                while (!string.IsNullOrEmpty(projectRoot) && !Directory.Exists(Path.Combine(projectRoot, "Components", "Mods")))
                {
                    projectRoot = Path.GetDirectoryName(projectRoot);
                }
                
                string sourcePath = Path.Combine(projectRoot ?? Directory.GetCurrentDirectory(), "Components", "Mods", "Castle Story");
                bool backupBeforeCopy = true;
                
                if (File.Exists(modJsonPath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(modJsonPath);
                        using JsonDocument doc = JsonDocument.Parse(jsonContent);
                        JsonElement root = doc.RootElement;
                        
                        // Get source path from settings
                        string sourcePathFromJson = sourcePath;
                        if (root.TryGetProperty("sourcePath", out JsonElement sourcePathElement))
                        {
                            sourcePathFromJson = sourcePathElement.GetString() ?? sourcePath;
                        }
                        else if (root.TryGetProperty("settings", out JsonElement settings))
                        {
                            if (settings.TryGetProperty("keyValues", out JsonElement keyValues))
                            {
                                foreach (JsonElement kv in keyValues.EnumerateArray())
                                {
                                    if (kv.TryGetProperty("key", out JsonElement key) && 
                                        key.GetString() == "sourcePath" &&
                                        kv.TryGetProperty("value", out JsonElement value))
                                    {
                                        sourcePathFromJson = value.GetString() ?? sourcePath;
                                    }
                                    if (kv.TryGetProperty("key", out JsonElement key2) && 
                                        key2.GetString() == "backupBeforeCopy" &&
                                        kv.TryGetProperty("value", out JsonElement value2))
                                    {
                                        backupBeforeCopy = value2.GetString()?.ToLower() == "true";
                                    }
                                }
                            }
                        }
                        
                        // Resolve relative paths to absolute
                        if (!Path.IsPathRooted(sourcePathFromJson))
                        {
                            // Relative path - resolve from project root
                            sourcePath = Path.Combine(projectRoot ?? Directory.GetCurrentDirectory(), sourcePathFromJson);
                        }
                        else
                        {
                            sourcePath = sourcePathFromJson;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Use defaults if JSON parsing fails
                        System.Diagnostics.Debug.WriteLine($"Error parsing mod.json: {ex.Message}");
                    }
                }
                
                return (IModIntegration)new FileCopyIntegration("Jason's Enhancements", sourcePath, backupBeforeCopy);
            }
            catch (Exception ex)
            {
                // Fallback to default path
                System.Diagnostics.Debug.WriteLine($"Error creating Jason's Enhancements integration: {ex.Message}");
                string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "Components", "Mods", "Castle Story");
                return (IModIntegration)new FileCopyIntegration("Jason's Enhancements", fallbackPath, true);
            }
        }
    }
}
