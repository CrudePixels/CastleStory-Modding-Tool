using System.Collections.Generic;
using CastleStoryModdingTool.ModIntegrations;

namespace CastleStoryModdingTool.ModDefinitions
{
    public static class MultiplayerModDefinition
    {
        public static IModIntegration CreateIntegration()
        {
            var patches = new List<MemoryPatch>
            {
                new MemoryPatch
                {
                    Description = "Increase team limit to 16",
                    ProcessName = "Castle Story",
                    SearchPattern = new byte[] { 0x04, 0x00, 0x00, 0x00 }, // Original team limit
                    ReplacementPattern = new byte[] { 0x10, 0x00, 0x00, 0x00 } // New team limit (16)
                },
                new MemoryPatch
                {
                    Description = "Increase player limit to 16",
                    ProcessName = "Castle Story", 
                    SearchPattern = new byte[] { 0x04, 0x00, 0x00, 0x00 }, // Original player limit
                    ReplacementPattern = new byte[] { 0x10, 0x00, 0x00, 0x00 } // New player limit (16)
                }
            };

            return new MemoryPatchingIntegration("MultiplayerMod", patches);
        }
    }
}
