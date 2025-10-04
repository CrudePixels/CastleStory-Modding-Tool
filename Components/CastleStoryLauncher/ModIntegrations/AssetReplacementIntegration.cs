using System;
using System.Collections.Generic;
using System.IO;

namespace CastleStoryModdingTool.ModIntegrations
{
    public class AssetReplacementIntegration : IModIntegration
    {
        public string ModName { get; }
        public ModIntegrationType IntegrationType => ModIntegrationType.AssetReplacement;
        
        private readonly List<AssetReplacement> replacements;
        
        public AssetReplacementIntegration(string modName, List<AssetReplacement> replacements)
        {
            ModName = modName;
            this.replacements = replacements;
        }

        public bool CanApply(string gameDirectory)
        {
            foreach (var replacement in replacements)
            {
                string targetPath = Path.Combine(gameDirectory, replacement.TargetPath);
                if (!File.Exists(replacement.SourcePath))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Apply(string gameDirectory, string backupDirectory, string logFile)
        {
            try
            {
                foreach (var replacement in replacements)
                {
                    string targetPath = Path.Combine(gameDirectory, replacement.TargetPath);
                    string backupPath = Path.Combine(backupDirectory, Path.GetFileName(replacement.TargetPath));
                    
                    // Create directory if needed
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                    
                    // Backup original asset
                    if (File.Exists(targetPath))
                    {
                        File.Copy(targetPath, backupPath, true);
                        File.AppendAllText(logFile, $"\nBacked up asset: {targetPath}");
                    }
                    
                    // Replace asset
                    File.Copy(replacement.SourcePath, targetPath, true);
                    File.AppendAllText(logFile, $"\nReplaced asset: {targetPath} with {replacement.SourcePath}");
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError replacing assets: {ex.Message}");
                return false;
            }
        }

        public bool CanUnapply(string gameDirectory)
        {
            return true;
        }

        public bool Unapply(string gameDirectory, string logFile)
        {
            try
            {
                foreach (var replacement in replacements)
                {
                    string targetPath = Path.Combine(gameDirectory, replacement.TargetPath);
                    string backupPath = Path.Combine(gameDirectory, "Info", "Lua", "ModBackup", Path.GetFileName(replacement.TargetPath));
                    
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, targetPath, true);
                        File.AppendAllText(logFile, $"\nRestored asset: {targetPath}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError unapplying asset replacement: {ex.Message}");
                return false;
            }
        }

        public string GetDescription()
        {
            return $"Asset Replacement Integration - {replacements.Count} asset(s)";
        }
    }

    public class AssetReplacement
    {
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
