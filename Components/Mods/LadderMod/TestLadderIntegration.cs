using System;
using System.IO;

namespace CastleStoryMods
{
    class TestLadderIntegration
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Castle Story Ladder Mod Integration Test");
            Console.WriteLine("========================================");
            
            try
            {
                // Test the integration
                bool success = LadderIntegration.IntegrateLadderMod();
                
                if (success)
                {
                    Console.WriteLine("\n✅ Ladder Mod integration completed successfully!");
                    Console.WriteLine("\nThe following files have been created:");
                    Console.WriteLine("- Data/Mods/LadderConfig.lua");
                    Console.WriteLine("- Data/Mods/LadderBlocks.lua");
                    Console.WriteLine("- Data/Mods/LadderMechanics.lua");
                    Console.WriteLine("- Data/Mods/LadderMod.lua");
                    Console.WriteLine("\nThe ladder system should now be available in Castle Story!");
                }
                else
                {
                    Console.WriteLine("\n❌ Ladder Mod integration failed!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
