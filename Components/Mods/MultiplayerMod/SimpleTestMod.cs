using System;
using System.IO;

namespace CastleStoryModding.ExampleMods
{
    public class SimpleTestMod
    {
        public static void Initialize()
        {
            // Create a simple test file to prove the mod is running
            string testFile = @"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool\CastleStoryLauncher\SIMPLE_MOD_TEST.txt";
            File.WriteAllText(testFile, $"Simple Test Mod Loaded at: {DateTime.Now}\nThis proves mod loading works!");
        }
        
        public static void OnGameStart()
        {
            string testFile = @"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool\CastleStoryLauncher\SIMPLE_MOD_TEST.txt";
            File.AppendAllText(testFile, $"\nGame Started at: {DateTime.Now}");
        }
    }
}
