using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CastleStoryModdingTool.ModDefinitions;
using CastleStoryModdingTool.ModIntegrations;

namespace CastleStoryModdingTool
{
    public class ModManager
    {
        private readonly Dictionary<string, IModIntegration> availableMods;
        private readonly Dictionary<string, ModIntegrationResult> appliedMods;

        public ModManager()
        {
            availableMods = new Dictionary<string, IModIntegration>();
            appliedMods = new Dictionary<string, ModIntegrationResult>();
            InitializeMods();
        }

        private void InitializeMods()
        {
            // Register available mods
            availableMods["LadderMod"] = LadderModDefinition.CreateIntegration();
            availableMods["MultiplayerMod"] = MultiplayerModDefinition.CreateIntegration();
        }

        public List<string> GetAvailableMods()
        {
            return availableMods.Keys.ToList();
        }

        public List<string> GetAppliedMods()
        {
            return appliedMods.Keys.ToList();
        }

        public ModIntegrationResult ApplyMod(string modName, string gameDirectory, string logFile)
        {
            try
            {
                if (!availableMods.ContainsKey(modName))
                {
                    return new ModIntegrationResult
                    {
                        Success = false,
                        Message = $"Mod '{modName}' not found",
                        IntegrationType = ModIntegrationType.FileModification
                    };
                }

                var mod = availableMods[modName];
                File.AppendAllText(logFile, $"\nApplying mod: {modName} using {mod.IntegrationType}");

                // Create backup directory
                string backupDir = Path.Combine(gameDirectory, "Info", "Lua", $"ModBackup_{DateTime.Now:yyyyMMdd_HHmmss}");
                Directory.CreateDirectory(backupDir);

                // Check if mod can be applied
                if (!mod.CanApply(gameDirectory))
                {
                    return new ModIntegrationResult
                    {
                        Success = false,
                        Message = $"Mod '{modName}' cannot be applied to this game directory",
                        IntegrationType = mod.IntegrationType
                    };
                }

                // Apply the mod
                bool success = mod.Apply(gameDirectory, backupDir, logFile);
                
                var result = new ModIntegrationResult
                {
                    Success = success,
                    Message = success ? $"Successfully applied {modName}" : $"Failed to apply {modName}",
                    IntegrationType = mod.IntegrationType
                };

                if (success)
                {
                    appliedMods[modName] = result;
                }

                return result;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError applying mod {modName}: {ex.Message}");
                return new ModIntegrationResult
                {
                    Success = false,
                    Message = $"Error applying {modName}: {ex.Message}",
                    IntegrationType = ModIntegrationType.FileModification
                };
            }
        }

        public ModIntegrationResult UnapplyMod(string modName, string gameDirectory, string logFile)
        {
            try
            {
                if (!appliedMods.ContainsKey(modName))
                {
                    return new ModIntegrationResult
                    {
                        Success = false,
                        Message = $"Mod '{modName}' is not currently applied",
                        IntegrationType = ModIntegrationType.FileModification
                    };
                }

                var mod = availableMods[modName];
                File.AppendAllText(logFile, $"\nUnapplying mod: {modName}");

                bool success = mod.Unapply(gameDirectory, logFile);
                
                var result = new ModIntegrationResult
                {
                    Success = success,
                    Message = success ? $"Successfully unapplied {modName}" : $"Failed to unapply {modName}",
                    IntegrationType = mod.IntegrationType
                };

                if (success)
                {
                    appliedMods.Remove(modName);
                }

                return result;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError unapplying mod {modName}: {ex.Message}");
                return new ModIntegrationResult
                {
                    Success = false,
                    Message = $"Error unapplying {modName}: {ex.Message}",
                    IntegrationType = ModIntegrationType.FileModification
                };
            }
        }

        public void ApplyMemoryPatches(Process gameProcess, string logFile)
        {
            try
            {
                File.AppendAllText(logFile, $"\nApplying memory patches to process: {gameProcess.ProcessName} (PID: {gameProcess.Id})");
                
                // Apply memory patches for multiplayer mod
                if (appliedMods.ContainsKey("MultiplayerMod"))
                {
                    // For now, just log that memory patching is needed
                    // The actual memory patching will be handled by the existing system
                    File.AppendAllText(logFile, $"\nMemory patching scheduled for MultiplayerMod");
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError applying memory patches: {ex.Message}");
            }
        }

        public void ApplyDLLInjections(Process gameProcess, string logFile)
        {
            try
            {
                File.AppendAllText(logFile, $"\nApplying DLL injections to process: {gameProcess.ProcessName} (PID: {gameProcess.Id})");
                
                // Find and apply DLL injections
                foreach (var mod in appliedMods)
                {
                    if (mod.Value.IntegrationType == ModIntegrationType.DLLInjection)
                    {
                        File.AppendAllText(logFile, $"\nDLL injection for: {mod.Key}");
                        var modIntegration = availableMods[mod.Key];
                        // DLL injection is handled by the integration's Apply method
                        // The actual injection happens when the game process starts
                        File.AppendAllText(logFile, $"\nDLL injection ready for: {mod.Key}");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError applying DLL injections: {ex.Message}");
            }
        }

        public void ApplyAssetReplacements(Process gameProcess, string logFile)
        {
            try
            {
                File.AppendAllText(logFile, $"\nApplying asset replacements for process: {gameProcess.ProcessName} (PID: {gameProcess.Id})");
                
                // Find and apply asset replacements
                foreach (var mod in appliedMods)
                {
                    if (mod.Value.IntegrationType == ModIntegrationType.AssetReplacement)
                    {
                        File.AppendAllText(logFile, $"\nAsset replacement for: {mod.Key}");
                        var modIntegration = availableMods[mod.Key];
                        // Asset replacement is handled by the integration's Apply method
                        // Assets are replaced directly in the game directory
                        File.AppendAllText(logFile, $"\nAsset replacement ready for: {mod.Key}");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError applying asset replacements: {ex.Message}");
            }
        }

        public string GetModDescription(string modName)
        {
            if (availableMods.ContainsKey(modName))
            {
                return availableMods[modName].GetDescription();
            }
            return "Mod not found";
        }

        public ModIntegrationType GetModIntegrationType(string modName)
        {
            if (availableMods.ContainsKey(modName))
            {
                return availableMods[modName].IntegrationType;
            }
            return ModIntegrationType.FileModification;
        }
    }
}
