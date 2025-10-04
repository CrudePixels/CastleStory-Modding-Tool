using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace CastleStoryLauncher
{
    public class GameInstallation
    {
        public string Path { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Version { get; set; } = "";
        public bool IsValid { get; set; } = false;
        public string DisplayName => $"{Platform} - {Path}";
    }

    public static class GameDetector
    {
        private static readonly string[] PossibleExecutables = {
            "CastleStory.exe",
            "CastleStoryUnity.exe", 
            "Unity.exe",
            "Castle Story.exe",
            "CastleStory_Data/CastleStory.exe",
            "CastleStory_Data/CastleStoryUnity.exe"
        };

        private static readonly string[] SteamCommonPaths = {
            @"C:\Program Files (x86)\Steam\steamapps\common",
            @"C:\Program Files\Steam\steamapps\common",
            @"D:\SteamLibrary\steamapps\common",
            @"E:\SteamLibrary\steamapps\common",
            @"F:\SteamLibrary\steamapps\common"
        };

        private static readonly string[] EpicPaths = {
            @"C:\Program Files\Epic Games",
            @"C:\Program Files (x86)\Epic Games",
            @"D:\Epic Games",
            @"E:\Epic Games"
        };

        private static readonly string[] GogPaths = {
            @"C:\Program Files (x86)\GOG Galaxy\Games",
            @"C:\Program Files\GOG Galaxy\Games",
            @"D:\GOG Games",
            @"E:\GOG Games"
        };

        public static List<GameInstallation> DetectAllInstallations()
        {
            var installations = new List<GameInstallation>();

            // Detect Steam installations
            installations.AddRange(DetectSteamInstallations());
            
            // Detect Epic installations
            installations.AddRange(DetectEpicInstallations());
            
            // Detect GOG installations
            installations.AddRange(DetectGogInstallations());
            
            // Detect manual installations
            installations.AddRange(DetectManualInstallations());

            return installations.Where(i => i.IsValid).ToList();
        }

        private static List<GameInstallation> DetectSteamInstallations()
        {
            var installations = new List<GameInstallation>();

            try
            {
                // Check Steam registry for library folders
                var steamPaths = GetSteamLibraryPaths();
                
                foreach (var steamPath in steamPaths)
                {
                    var castleStoryPath = Path.Combine(steamPath, "Castle Story");
                    if (Directory.Exists(castleStoryPath))
                    {
                        var installation = ValidateInstallation(castleStoryPath, "Steam");
                        if (installation != null)
                        {
                            installations.Add(installation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detecting Steam installations: {ex.Message}");
            }

            return installations;
        }

        private static List<string> GetSteamLibraryPaths()
        {
            var paths = new List<string>();

            try
            {
                // Get Steam installation path from registry
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                {
                    if (key != null)
                    {
                        var steamPath = key.GetValue("InstallPath")?.ToString();
                        if (!string.IsNullOrEmpty(steamPath))
                        {
                            paths.Add(Path.Combine(steamPath, "steamapps", "common"));
                        }
                    }
                }

                // Also check common Steam library paths
                paths.AddRange(SteamCommonPaths.Where(Directory.Exists));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting Steam library paths: {ex.Message}");
            }

            return paths.Distinct().ToList();
        }

        private static List<GameInstallation> DetectEpicInstallations()
        {
            var installations = new List<GameInstallation>();

            try
            {
                foreach (var epicPath in EpicPaths)
                {
                    if (Directory.Exists(epicPath))
                    {
                        // Look for Castle Story in Epic Games directory
                        var castleStoryPath = Path.Combine(epicPath, "Castle Story");
                        if (Directory.Exists(castleStoryPath))
                        {
                            var installation = ValidateInstallation(castleStoryPath, "Epic Games");
                            if (installation != null)
                            {
                                installations.Add(installation);
                            }
                        }

                        // Also check subdirectories
                        var subdirs = Directory.GetDirectories(epicPath, "*Castle*", SearchOption.TopDirectoryOnly);
                        foreach (var subdir in subdirs)
                        {
                            var installation = ValidateInstallation(subdir, "Epic Games");
                            if (installation != null)
                            {
                                installations.Add(installation);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detecting Epic installations: {ex.Message}");
            }

            return installations;
        }

        private static List<GameInstallation> DetectGogInstallations()
        {
            var installations = new List<GameInstallation>();

            try
            {
                foreach (var gogPath in GogPaths)
                {
                    if (Directory.Exists(gogPath))
                    {
                        // Look for Castle Story in GOG directory
                        var castleStoryPath = Path.Combine(gogPath, "Castle Story");
                        if (Directory.Exists(castleStoryPath))
                        {
                            var installation = ValidateInstallation(castleStoryPath, "GOG");
                            if (installation != null)
                            {
                                installations.Add(installation);
                            }
                        }

                        // Also check subdirectories
                        var subdirs = Directory.GetDirectories(gogPath, "*Castle*", SearchOption.TopDirectoryOnly);
                        foreach (var subdir in subdirs)
                        {
                            var installation = ValidateInstallation(subdir, "GOG");
                            if (installation != null)
                            {
                                installations.Add(installation);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detecting GOG installations: {ex.Message}");
            }

            return installations;
        }

        private static List<GameInstallation> DetectManualInstallations()
        {
            var installations = new List<GameInstallation>();

            try
            {
                // Check common manual installation paths
                var commonPaths = new[]
                {
                    @"C:\Games\Castle Story",
                    @"D:\Games\Castle Story",
                    @"E:\Games\Castle Story",
                    @"C:\Program Files\Castle Story",
                    @"C:\Program Files (x86)\Castle Story",
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Castle Story",
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Castle Story"
                };

                foreach (var path in commonPaths)
                {
                    if (Directory.Exists(path))
                    {
                        var installation = ValidateInstallation(path, "Manual");
                        if (installation != null)
                        {
                            installations.Add(installation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detecting manual installations: {ex.Message}");
            }

            return installations;
        }

        private static GameInstallation ValidateInstallation(string gamePath, string platform)
        {
            try
            {
                if (!Directory.Exists(gamePath))
                    return null;

                // Look for executable files
                foreach (var executable in PossibleExecutables)
                {
                    var fullPath = Path.Combine(gamePath, executable);
                    if (File.Exists(fullPath))
                    {
                        var installation = new GameInstallation
                        {
                            Path = gamePath,
                            ExecutablePath = fullPath,
                            Platform = platform,
                            IsValid = true
                        };

                        // Try to get version information
                        installation.Version = GetGameVersion(fullPath);

                        return installation;
                    }
                }

                // Check subdirectories
                var subdirs = new[] { "CastleStory_Data", "Castle Story_Data", "Game" };
                foreach (var subdir in subdirs)
                {
                    var subdirPath = Path.Combine(gamePath, subdir);
                    if (Directory.Exists(subdirPath))
                    {
                        foreach (var executable in PossibleExecutables)
                        {
                            var fullPath = Path.Combine(subdirPath, executable);
                            if (File.Exists(fullPath))
                            {
                                var installation = new GameInstallation
                                {
                                    Path = gamePath,
                                    ExecutablePath = fullPath,
                                    Platform = platform,
                                    IsValid = true
                                };

                                installation.Version = GetGameVersion(fullPath);
                                return installation;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating installation at {gamePath}: {ex.Message}");
                return null;
            }
        }

        private static string GetGameVersion(string executablePath)
        {
            try
            {
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(executablePath);
                return !string.IsNullOrEmpty(versionInfo.FileVersion) ? versionInfo.FileVersion : "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static GameInstallation DetectBestInstallation()
        {
            var installations = DetectAllInstallations();
            
            if (installations.Count == 0)
                return null;

            // Prefer Steam installations, then Epic, then GOG, then Manual
            var orderedInstallations = installations.OrderBy(i => i.Platform switch
            {
                "Steam" => 1,
                "Epic Games" => 2,
                "GOG" => 3,
                "Manual" => 4,
                _ => 5
            }).ToList();

            return orderedInstallations.First();
        }

        public static bool IsValidGameDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return false;

            return PossibleExecutables.Any(exe => File.Exists(Path.Combine(directory, exe)));
        }
    }
}
