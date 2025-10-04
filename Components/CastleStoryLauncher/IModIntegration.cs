using System;
using System.Collections.Generic;

namespace CastleStoryModdingTool
{
    public enum ModIntegrationType
    {
        FileModification,    // Modify game files directly
        MemoryPatching,      // Patch memory at runtime
        DLLInjection,        // Inject DLL into game process
        LuaInjection,        // Inject Lua code
        AssetReplacement     // Replace game assets
    }

    public interface IModIntegration
    {
        string ModName { get; }
        ModIntegrationType IntegrationType { get; }
        bool CanApply(string gameDirectory);
        bool Apply(string gameDirectory, string backupDirectory, string logFile);
        bool CanUnapply(string gameDirectory);
        bool Unapply(string gameDirectory, string logFile);
        string GetDescription();
    }

    public class ModIntegrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ModIntegrationType IntegrationType { get; set; }
        public List<string> ModifiedFiles { get; set; } = new List<string>();
    }
}
