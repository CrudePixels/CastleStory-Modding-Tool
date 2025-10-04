using System;
using System.Collections.Generic;
using System.IO;

namespace CastleStoryModdingTool.ModIntegrations
{
    public class LuaInjectionIntegration : IModIntegration
    {
        public string ModName { get; }
        public ModIntegrationType IntegrationType => ModIntegrationType.LuaInjection;
        
        private readonly List<LuaInjection> injections;
        
        public LuaInjectionIntegration(string modName, List<LuaInjection> injections)
        {
            ModName = modName;
            this.injections = injections;
        }

        public bool CanApply(string gameDirectory)
        {
            foreach (var injection in injections)
            {
                string targetFile = Path.Combine(gameDirectory, injection.TargetFile);
                if (!File.Exists(targetFile) && !injection.CreateIfNotExists)
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
                foreach (var injection in injections)
                {
                    string targetFile = Path.Combine(gameDirectory, injection.TargetFile);
                    string backupFile = Path.Combine(backupDirectory, Path.GetFileName(injection.TargetFile));
                    
                    // Create directory if needed
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFile)!);
                    
                    // Backup original file
                    if (File.Exists(targetFile))
                    {
                        File.Copy(targetFile, backupFile, true);
                        File.AppendAllText(logFile, $"\nBacked up: {targetFile}");
                    }
                    
                    // Apply Lua injection
                    switch (injection.Type)
                    {
                        case LuaInjectionType.Append:
                            File.AppendAllText(targetFile, injection.LuaCode);
                            break;
                        case LuaInjectionType.InsertBefore:
                            string originalContent = File.Exists(targetFile) ? File.ReadAllText(targetFile) : "";
                            string newContent = originalContent.Replace(injection.InsertMarker, injection.LuaCode + "\n" + injection.InsertMarker);
                            File.WriteAllText(targetFile, newContent);
                            break;
                        case LuaInjectionType.InsertAfter:
                            string originalContent2 = File.Exists(targetFile) ? File.ReadAllText(targetFile) : "";
                            string newContent2 = originalContent2.Replace(injection.InsertMarker, injection.InsertMarker + "\n" + injection.LuaCode);
                            File.WriteAllText(targetFile, newContent2);
                            break;
                        case LuaInjectionType.Replace:
                            File.WriteAllText(targetFile, injection.LuaCode);
                            break;
                    }
                    
                    File.AppendAllText(logFile, $"\nApplied Lua injection to: {targetFile}");
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError applying Lua injection: {ex.Message}");
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
                foreach (var injection in injections)
                {
                    string targetFile = Path.Combine(gameDirectory, injection.TargetFile);
                    string backupFile = Path.Combine(gameDirectory, "Info", "Lua", "ModBackup", Path.GetFileName(injection.TargetFile));
                    
                    if (File.Exists(backupFile))
                    {
                        File.Copy(backupFile, targetFile, true);
                        File.AppendAllText(logFile, $"\nRestored: {targetFile}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError unapplying Lua injection: {ex.Message}");
                return false;
            }
        }

        public string GetDescription()
        {
            return $"Lua Injection Integration - {injections.Count} injection(s)";
        }
    }

    public class LuaInjection
    {
        public string TargetFile { get; set; } = string.Empty;
        public LuaInjectionType Type { get; set; }
        public string LuaCode { get; set; } = string.Empty;
        public string InsertMarker { get; set; } = string.Empty;
        public bool CreateIfNotExists { get; set; } = false;
    }

    public enum LuaInjectionType
    {
        Append,
        InsertBefore,
        InsertAfter,
        Replace
    }
}
