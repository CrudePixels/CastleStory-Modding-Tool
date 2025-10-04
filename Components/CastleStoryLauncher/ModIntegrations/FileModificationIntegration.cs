using System;
using System.Collections.Generic;
using System.IO;

namespace CastleStoryModdingTool.ModIntegrations
{
    public class FileModificationIntegration : IModIntegration
    {
        public string ModName { get; }
        public ModIntegrationType IntegrationType => ModIntegrationType.FileModification;
        
        private readonly List<FileModification> modifications;
        
        public FileModificationIntegration(string modName, List<FileModification> modifications)
        {
            ModName = modName;
            this.modifications = modifications;
        }

        public bool CanApply(string gameDirectory)
        {
            foreach (var mod in modifications)
            {
                string fullPath = Path.Combine(gameDirectory, mod.RelativePath);
                bool fileExists = File.Exists(fullPath);
                bool canCreate = mod.CreateIfNotExists;
                
                // Debug logging
                string debugLog = Path.Combine(gameDirectory, "Info", "Lua", "ModDebug.txt");
                File.AppendAllText(debugLog, $"\n[DEBUG] Checking file: {fullPath}");
                File.AppendAllText(debugLog, $"\n[DEBUG] File exists: {fileExists}");
                File.AppendAllText(debugLog, $"\n[DEBUG] Can create: {canCreate}");
                
                if (!fileExists && !canCreate)
                {
                    File.AppendAllText(debugLog, $"\n[DEBUG] FAILING: File doesn't exist and can't create");
                    return false;
                }
            }
            return true;
        }

        public bool Apply(string gameDirectory, string backupDirectory, string logFile)
        {
            try
            {
                foreach (var mod in modifications)
                {
                    string fullPath = Path.Combine(gameDirectory, mod.RelativePath);
                    string backupPath = Path.Combine(backupDirectory, Path.GetFileName(mod.RelativePath));
                    
                    // Create directory if needed
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                    
                    // Backup original file
                    if (File.Exists(fullPath))
                    {
                        File.Copy(fullPath, backupPath, true);
                        File.AppendAllText(logFile, $"\nBacked up: {fullPath}");
                    }
                    
                    // Apply modification
                    switch (mod.Type)
                    {
                        case FileModificationType.Append:
                            File.AppendAllText(fullPath, mod.Content);
                            break;
                        case FileModificationType.Replace:
                            File.WriteAllText(fullPath, mod.Content);
                            break;
                        case FileModificationType.InsertBefore:
                            string originalContent = File.Exists(fullPath) ? File.ReadAllText(fullPath) : "";
                            string newContent = originalContent.Replace(mod.InsertMarker, mod.Content + "\n" + mod.InsertMarker);
                            File.WriteAllText(fullPath, newContent);
                            break;
                        case FileModificationType.InsertAfter:
                            string originalContent2 = File.Exists(fullPath) ? File.ReadAllText(fullPath) : "";
                            string newContent2 = originalContent2.Replace(mod.InsertMarker, mod.InsertMarker + "\n" + mod.Content);
                            File.WriteAllText(fullPath, newContent2);
                            break;
                    }
                    
                    File.AppendAllText(logFile, $"\nApplied {mod.Type} to: {fullPath}");
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError applying file modifications: {ex.Message}");
                return false;
            }
        }

        public bool CanUnapply(string gameDirectory)
        {
            return true; // Can always unapply file modifications
        }

        public bool Unapply(string gameDirectory, string logFile)
        {
            try
            {
                foreach (var mod in modifications)
                {
                    string fullPath = Path.Combine(gameDirectory, mod.RelativePath);
                    string backupPath = Path.Combine(gameDirectory, "Info", "Lua", "ModBackup", Path.GetFileName(mod.RelativePath));
                    
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, fullPath, true);
                        File.AppendAllText(logFile, $"\nRestored: {fullPath}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError unapplying file modifications: {ex.Message}");
                return false;
            }
        }

        public string GetDescription()
        {
            return $"File Modification Integration - {modifications.Count} file(s) to modify";
        }
    }

    public class FileModification
    {
        public string RelativePath { get; set; } = string.Empty;
        public FileModificationType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public string InsertMarker { get; set; } = string.Empty;
        public bool CreateIfNotExists { get; set; } = false;
    }

    public enum FileModificationType
    {
        Append,
        Replace,
        InsertBefore,
        InsertAfter
    }
}
