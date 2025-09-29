using System;
using System.Diagnostics;
using System.IO;

namespace MultiplayerMod
{
    /// <summary>
    /// Main entry point for the file-based multiplayer mod
    /// This approach modifies game configuration files instead of runtime memory
    /// </summary>
    public class FileBasedMod
    {
        private static string logPath;
        
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== Castle Story Multiplayer Mod (File-Based) ===");
                Console.WriteLine("This mod modifies game configuration files to increase multiplayer limits");
                Console.WriteLine();
                
                // Parse command line arguments
                var maxPlayers = 32;
                var maxTeams = 16;
                var gameDirectory = "";
                var modDirectory = "";
                
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case "--maxplayers":
                        case "-p":
                            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int players))
                            {
                                maxPlayers = players;
                                i++; // Skip next argument
                            }
                            break;
                        case "--maxteams":
                        case "-t":
                            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int teams))
                            {
                                maxTeams = teams;
                                i++; // Skip next argument
                            }
                            break;
                        case "--gamedir":
                        case "-g":
                            if (i + 1 < args.Length)
                            {
                                gameDirectory = args[i + 1];
                                i++; // Skip next argument
                            }
                            break;
                        case "--moddir":
                        case "-m":
                            if (i + 1 < args.Length)
                            {
                                modDirectory = args[i + 1];
                                i++; // Skip next argument
                            }
                            break;
                        case "--help":
                        case "-h":
                            ShowHelp();
                            return;
                    }
                }
                
                // Set default directories if not provided
                if (string.IsNullOrEmpty(gameDirectory))
                {
                    gameDirectory = FindCastleStoryDirectory();
                }
                
                if (string.IsNullOrEmpty(modDirectory))
                {
                    modDirectory = Path.Combine(Environment.CurrentDirectory, "mods");
                }
                
                // Validate directories
                if (!Directory.Exists(gameDirectory))
                {
                    Console.WriteLine($"❌ Game directory not found: {gameDirectory}");
                    Console.WriteLine("Please specify the correct Castle Story installation directory using --gamedir");
                    return;
                }
                
                Directory.CreateDirectory(modDirectory);
                
                // Initialize logging
                logPath = Path.Combine(modDirectory, "multiplayer_mod.log");
                LogMessage("=== Multiplayer Mod Started ===");
                LogMessage($"Game Directory: {gameDirectory}");
                LogMessage($"Mod Directory: {modDirectory}");
                LogMessage($"Max Players: {maxPlayers}");
                LogMessage($"Max Teams: {maxTeams}");
                
                // Apply modifications
                var modifier = new FileModifier(gameDirectory, modDirectory);
                bool success = modifier.ApplyModifications(maxPlayers, maxTeams);
                
                if (success)
                {
                    Console.WriteLine();
                    Console.WriteLine("🎉 Multiplayer mod applied successfully!");
                    Console.WriteLine($"   • Max Players: {maxPlayers}");
                    Console.WriteLine($"   • Max Teams: {maxTeams}");
                    Console.WriteLine($"   • Game Directory: {gameDirectory}");
                    Console.WriteLine($"   • Mod Directory: {modDirectory}");
                    Console.WriteLine();
                    Console.WriteLine("The game configuration files have been modified.");
                    Console.WriteLine("You can now launch Castle Story with increased multiplayer limits.");
                    Console.WriteLine();
                    Console.WriteLine("To restore original settings, run with --restore flag.");
                    
                    LogMessage("Mod applied successfully");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("❌ Failed to apply multiplayer mod!");
                    Console.WriteLine("Check the log file for details: " + logPath);
                    
                    LogMessage("Mod application failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
                LogMessage($"Unexpected error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Find Castle Story installation directory
        /// </summary>
        private static string FindCastleStoryDirectory()
        {
            var possiblePaths = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Castle Story",
                @"C:\Program Files\Steam\steamapps\common\Castle Story",
                @"D:\Steam\steamapps\common\Castle Story",
                @"C:\Games\Castle Story",
                @"D:\Games\Castle Story"
            };
            
            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path) && File.Exists(Path.Combine(path, "CastleStory.exe")))
                {
                    return path;
                }
            }
            
            // Try to find from current directory
            var currentDir = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(currentDir))
            {
                if (File.Exists(Path.Combine(currentDir, "CastleStory.exe")))
                {
                    return currentDir;
                }
                currentDir = Path.GetDirectoryName(currentDir);
            }
            
            return "";
        }
        
        /// <summary>
        /// Show help information
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Castle Story Multiplayer Mod (File-Based)");
            Console.WriteLine();
            Console.WriteLine("Usage: MultiplayerMod.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --maxplayers, -p <number>    Maximum number of players (default: 32)");
            Console.WriteLine("  --maxteams, -t <number>      Maximum number of teams (default: 16)");
            Console.WriteLine("  --gamedir, -g <path>         Castle Story installation directory");
            Console.WriteLine("  --moddir, -m <path>          Mod directory for logs and backups");
            Console.WriteLine("  --restore                    Restore original game files from backup");
            Console.WriteLine("  --help, -h                   Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  MultiplayerMod.exe --maxplayers 64 --maxteams 32");
            Console.WriteLine("  MultiplayerMod.exe --gamedir \"C:\\Games\\Castle Story\"");
            Console.WriteLine("  MultiplayerMod.exe --restore");
            Console.WriteLine();
            Console.WriteLine("This mod modifies game configuration files to increase multiplayer limits.");
            Console.WriteLine("Original files are backed up before modification.");
        }
        
        /// <summary>
        /// Log a message to the log file
        /// </summary>
        private static void LogMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(logPath))
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                    File.AppendAllText(logPath, logEntry);
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
