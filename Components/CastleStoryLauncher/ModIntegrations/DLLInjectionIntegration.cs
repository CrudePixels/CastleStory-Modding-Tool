using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CastleStoryModdingTool.ModIntegrations
{
    public class DLLInjectionIntegration : IModIntegration
    {
        public string ModName { get; }
        public ModIntegrationType IntegrationType => ModIntegrationType.DLLInjection;
        
        private readonly string dllPath;
        private readonly string processName;
        
        public DLLInjectionIntegration(string modName, string dllPath, string processName = "Castle Story")
        {
            ModName = modName;
            this.dllPath = dllPath;
            this.processName = processName;
        }

        public bool CanApply(string gameDirectory)
        {
            return File.Exists(dllPath);
        }

        public bool Apply(string gameDirectory, string backupDirectory, string logFile)
        {
            try
            {
                File.AppendAllText(logFile, $"\nDLL injection scheduled for: {ModName}");
                File.AppendAllText(logFile, $"\nDLL path: {dllPath}");
                File.AppendAllText(logFile, $"\nTarget process: {processName}");
                
                // Store injection info for later use
                string injectionInfoFile = Path.Combine(gameDirectory, "Info", "Lua", "ModBackup", $"{ModName}_DLLInjection.json");
                Directory.CreateDirectory(Path.GetDirectoryName(injectionInfoFile)!);
                
                var injectionInfo = new
                {
                    ModName = ModName,
                    DLLPath = dllPath,
                    ProcessName = processName,
                    ScheduledAt = DateTime.Now
                };
                
                File.WriteAllText(injectionInfoFile, System.Text.Json.JsonSerializer.Serialize(injectionInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError scheduling DLL injection: {ex.Message}");
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
                File.AppendAllText(logFile, $"\nDLL injection will be removed on next game launch");
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError unapplying DLL injection: {ex.Message}");
                return false;
            }
        }

        public string GetDescription()
        {
            return $"DLL Injection Integration - {Path.GetFileName(dllPath)}";
        }
    }
}
