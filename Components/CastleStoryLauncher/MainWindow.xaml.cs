using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using Newtonsoft.Json;
using CastleStoryModdingTool;

namespace CastleStoryLauncher
{
    // Windows API for DLL injection
    public static class WinAPI
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, IntPtr lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCurrentThreadId();

        public const int PROCESS_CREATE_THREAD = 0x0002;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_VM_READ = 0x0010;
        public const uint MEM_COMMIT = 0x00001000;
        public const uint MEM_RESERVE = 0x00002000;
        public const uint PAGE_READWRITE = 4;
    }

    public partial class MainWindow : Window
    {
        private Process? gameProcess;
        private DispatcherTimer? statusTimer;
        private ObservableCollection<ModInfo> availableMods = new ObservableCollection<ModInfo>();
        private string gameExecutablePath = "";
        private string modsDirectory = "Mods";

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize logging
            var logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
            Logger.Initialize(logsDir);
            Logger.LogInfo("Castle Story Modding Tool started", "MainWindow");
            
            InitializeLauncher();
            SetupStatusTimer();
            LoadSettings();
            
            // Check if we should open the editor directly
            var args = Environment.GetCommandLineArgs();
            System.Diagnostics.Debug.WriteLine($"Command line args: {string.Join(" ", args)}");
            
            if (args.Length > 1 && args[1] == "--open-editor")
            {
                System.Diagnostics.Debug.WriteLine("Opening Lua Editor directly...");
                try
                {
                    // Hide the main launcher window immediately
                    this.Hide();
                    
                    // Open the Lua Editor directly
                    var luaEditor = new LuaEditorWindow();
                    luaEditor.Show();
                    luaEditor.Activate();
                    luaEditor.Topmost = true;
                    luaEditor.Topmost = false;
                    System.Diagnostics.Debug.WriteLine("Lua Editor opened successfully");
                    
                    // Close the main launcher window after the editor is shown
                    this.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error opening Lua Editor: {ex.Message}");
                    MessageBox.Show($"Error opening Lua Editor: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If an error occurs, ensure the main window is closed
                    this.Close();
                }
            }
        }

        private void InitializeLauncher()
        {
            ModListBox.ItemsSource = availableMods;
            ModDirectoryTextBox.Text = Path.GetFullPath(modsDirectory);
            RefreshMods();
            UpdateStatus("Ready", "Launcher initialized");
        }

        private void SetupStatusTimer()
        {
            statusTimer = new DispatcherTimer();
            statusTimer.Interval = TimeSpan.FromSeconds(2);
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            CheckGameStatus();
            UpdateModCount();
        }

        private void CheckGameStatus()
        {
            bool isRunning = gameProcess != null && !gameProcess.HasExited;
            
            if (isRunning)
            {
                GameStatusText2.Text = "Game: Running";
                GameStatusText2.Foreground = new SolidColorBrush(Colors.LightGreen);
                StatusIndicator.Fill = new SolidColorBrush(Colors.Green);
                StatusText.Text = "Game is running";
            }
            else
            {
                GameStatusText2.Text = "Game: Not Running";
                GameStatusText2.Foreground = new SolidColorBrush(Colors.Red);
                StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
                StatusText.Text = "Ready to launch";
                gameProcess = null;
            }
        }

        private void UpdateModCount()
        {
            int selectedCount = availableMods.Count(m => m.IsEnabled);
            ModCountText.Text = $"{selectedCount} mods selected";
        }

        private void BrowseGame_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Castle Story Executable",
                Filter = "Castle Story Executable (*.exe)|*.exe|All files (*.*)|*.*",
                InitialDirectory = @"D:\SteamLibrary\steamapps\common\Castle Story"
            };

            if (dialog.ShowDialog() == true)
            {
                GameDirectoryTextBox.Text = Path.GetDirectoryName(dialog.FileName) ?? "";
                gameExecutablePath = dialog.FileName;
                ValidateGamePath();
                SaveSettings();
            }
        }

        private void ValidateGamePath()
        {
            string gameDir = GameDirectoryTextBox.Text;
            string[] possibleExecutables = { 
                "CastleStory.exe", 
                "CastleStoryUnity.exe", 
                "Unity.exe",
                "Castle Story.exe",
                "CastleStory_Data/CastleStory.exe",
                "CastleStory_Data/CastleStoryUnity.exe"
            };
            
            bool found = false;
            foreach (string exe in possibleExecutables)
            {
                string fullPath = Path.Combine(gameDir, exe);
                if (File.Exists(fullPath))
                {
                    gameExecutablePath = fullPath;
                    GameStatusText.Text = $"Game found: {exe}";
                    GameStatusText.Foreground = new SolidColorBrush(Colors.LightGreen);
                    found = true;
                    break;
                }
            }
            
            // Also check common Steam subdirectories
            if (!found)
            {
                string[] steamSubdirs = { "", "CastleStory_Data", "Castle Story_Data", "Game" };
                foreach (string subdir in steamSubdirs)
                {
                    string checkDir = string.IsNullOrEmpty(subdir) ? gameDir : Path.Combine(gameDir, subdir);
                    foreach (string exe in possibleExecutables)
                    {
                        string fullPath = Path.Combine(checkDir, exe);
                        if (File.Exists(fullPath))
                        {
                            gameExecutablePath = fullPath;
                            GameStatusText.Text = $"Game found: {Path.Combine(subdir, exe)}";
                            GameStatusText.Foreground = new SolidColorBrush(Colors.LightGreen);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }
            
            if (!found)
            {
                GameStatusText.Text = "Game not found - Check the directory path";
                GameStatusText.Foreground = new SolidColorBrush(Colors.Red);
                gameExecutablePath = "";
            }
        }

        private void BrowseMods_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Mods Directory",
                InitialDirectory = Path.GetFullPath(ModDirectoryTextBox.Text)
            };

            if (dialog.ShowDialog() == true)
            {
                ModDirectoryTextBox.Text = dialog.FolderName;
                modsDirectory = dialog.FolderName;
                RefreshMods();
                SaveSettings();
            }
        }

        private void RefreshMods()
        {
            availableMods.Clear();
            
            try
            {
                string modsDir = ModDirectoryTextBox.Text;
                if (!Directory.Exists(modsDir))
                {
                    Directory.CreateDirectory(modsDir);
                    UpdateStatus("Ready", "Mods directory created");
                    return;
                }

                string[] modDirectories = Directory.GetDirectories(modsDir);
                var modInfos = new List<ModInfo>();
                
                foreach (string modDir in modDirectories)
                {
                    try
                    {
                        var modInfo = LoadModInfo(modDir);
                        if (modInfo != null)
                        {
                            modInfos.Add(modInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading mod from {modDir}: {ex.Message}");
                    }
                }
                
                // Sort mods by priority (higher priority first) then by name
                modInfos = modInfos.OrderByDescending(m => m.priority).ThenBy(m => m.name).ToList();
                
                // Validate mods for conflicts and dependencies
                ValidateMods(modInfos);
                
                // Add validated mods to the collection
                foreach (var mod in modInfos)
                {
                    availableMods.Add(mod);
                }
                
                UpdateStatus("Mods Loaded", $"Found {availableMods.Count} mods ({availableMods.Count(m => m.IsEnabled)} enabled)");
                UpdateModCount();
            }
            catch (Exception ex)
            {
                UpdateStatus("Error", $"Failed to load mods: {ex.Message}");
            }
        }

        private ModInfo? LoadModInfo(string modDir)
        {
            string modName = Path.GetFileName(modDir);
            string configPath = Path.Combine(modDir, "mod.json");
            
            var modInfo = new ModInfo
            {
                name = modName,
                modPath = modDir,
                configPath = configPath,
                lastModified = Directory.GetLastWriteTime(modDir),
                fileSize = CalculateDirectorySize(modDir)
            };
            
            if (File.Exists(configPath))
            {
                try
                {
                    string configJson = File.ReadAllText(configPath);
                    var configData = JsonConvert.DeserializeObject<Dictionary<string, object>>(configJson);
                    
                    if (configData != null)
                    {
                        // Parse basic properties
                        modInfo.name = configData.GetValueOrDefault("name", modName)?.ToString() ?? modName;
                        modInfo.version = configData.GetValueOrDefault("version", "1.0.0")?.ToString() ?? "1.0.0";
                        modInfo.author = configData.GetValueOrDefault("author", "Unknown")?.ToString() ?? "Unknown";
                        modInfo.description = configData.GetValueOrDefault("description", "No description")?.ToString() ?? "No description";
                        
                        // Parse category
                        if (configData.ContainsKey("category") && Enum.TryParse<ModCategory>(configData["category"]?.ToString(), true, out var category))
                        {
                            modInfo.category = category;
                        }
                        
                        // Parse priority
                        if (configData.ContainsKey("priority") && int.TryParse(configData["priority"]?.ToString(), out var priority))
                        {
                            modInfo.priority = priority;
                        }
                        
                        // Parse dependencies
                        if (configData.ContainsKey("dependencies") && configData["dependencies"] is Newtonsoft.Json.Linq.JArray deps)
                        {
                            modInfo.dependencies = deps.ToObject<List<string>>() ?? new List<string>();
                        }
                        
                        // Parse conflicts
                        if (configData.ContainsKey("conflicts") && configData["conflicts"] is Newtonsoft.Json.Linq.JArray conflicts)
                        {
                            modInfo.conflicts = conflicts.ToObject<List<string>>() ?? new List<string>();
                        }
                        
                        // Parse features
                        if (configData.ContainsKey("features") && configData["features"] is Newtonsoft.Json.Linq.JArray features)
                        {
                            modInfo.features = features.ToObject<List<string>>() ?? new List<string>();
                        }
                        
                        // Parse settings
                        if (configData.ContainsKey("settings") && configData["settings"] is Newtonsoft.Json.Linq.JObject settings)
                        {
                            modInfo.settings = settings.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>();
                        }
                        
                        // Parse version requirements
                        modInfo.minimumGameVersion = configData.GetValueOrDefault("minimumGameVersion", "")?.ToString() ?? "";
                        modInfo.maximumGameVersion = configData.GetValueOrDefault("maximumGameVersion", "")?.ToString() ?? "";
                        
                        // Parse required flag
                        if (configData.ContainsKey("isRequired") && bool.TryParse(configData["isRequired"]?.ToString(), out var isRequired))
                        {
                            modInfo.isRequired = isRequired;
                        }
                    }
                }
                catch (Exception ex)
                {
                    modInfo.status = ModStatus.Error;
                    modInfo.errorMessage = $"Failed to parse mod.json: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"Error parsing mod.json for {modName}: {ex.Message}");
                }
            }
            else
            {
                modInfo.status = ModStatus.Error;
                modInfo.errorMessage = "Missing mod.json file";
            }
            
            // Set default enabled state based on mod status
            modInfo.IsEnabled = modInfo.status == ModStatus.Ready;
            
            return modInfo;
        }

        private long CalculateDirectorySize(string directory)
        {
            try
            {
                return Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                    .Sum(file => new FileInfo(file).Length);
            }
            catch
            {
                return 0;
            }
        }

        private void ValidateMods(List<ModInfo> mods)
        {
            var modNames = mods.Select(m => m.name).ToHashSet();
            
            foreach (var mod in mods)
            {
                // Check dependencies
                foreach (var dependency in mod.dependencies)
                {
                    if (!modNames.Contains(dependency))
                    {
                        mod.status = ModStatus.MissingDependencies;
                        mod.errorMessage = $"Missing dependency: {dependency}";
                        mod.IsEnabled = false;
                        break;
                    }
                }
                
                // Check conflicts
                if (mod.status == ModStatus.Ready)
                {
                    foreach (var conflict in mod.conflicts)
                    {
                        var conflictingMod = mods.FirstOrDefault(m => m.name == conflict && m.IsEnabled);
                        if (conflictingMod != null)
                        {
                            mod.status = ModStatus.Conflicting;
                            mod.errorMessage = $"Conflicts with: {conflict}";
                            mod.IsEnabled = false;
                            break;
                        }
                    }
                }
                
                // Check game version compatibility
                if (mod.status == ModStatus.Ready && !string.IsNullOrEmpty(mod.minimumGameVersion))
                {
                    // This would need actual game version checking
                    // For now, assume compatible
                    mod.isCompatible = true;
                }
            }
        }

        private void RefreshMods_Click(object sender, RoutedEventArgs e) => RefreshMods();
        private void SelectAllMods_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mod in availableMods) mod.IsEnabled = true;
            UpdateModCount();
            UpdateStatus("Mods Updated", $"Selected all {availableMods.Count} mods");
        }
        private void DeselectAllMods_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mod in availableMods) mod.IsEnabled = false;
            UpdateModCount();
            UpdateStatus("Mods Updated", "Deselected all mods");
        }


        private void LaunchGame_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(gameExecutablePath) || !File.Exists(gameExecutablePath))
            {
                MessageBox.Show("Please select a valid Castle Story executable first.", "No Game Selected", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                LaunchGameButton.IsEnabled = false;
                LaunchGameButton.Content = "Launching...";
                
                // Launch game synchronously on main thread
                LaunchGameProcess();
                
                UpdateStatus("Game Launched", "Castle Story started successfully");
            }
            catch (Exception ex)
            {
                UpdateStatus("Launch Failed", $"Failed to launch game: {ex.Message}");
                MessageBox.Show($"Failed to launch game: {ex.Message}", "Launch Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LaunchGameButton.IsEnabled = true;
                LaunchGameButton.Content = "🚀 Launch Castle Story with Mods";
            }
        }

        private void LaunchGameProcess()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = gameExecutablePath,
                WorkingDirectory = Path.GetDirectoryName(gameExecutablePath),
                UseShellExecute = false
            };

            var args = new List<string>();
            
            // Basic launch options - Unity specific arguments
            if (WindowedRadio.IsChecked == true) 
            {
                args.Add("-windowed");
            }
            else if (BorderlessRadio.IsChecked == true) 
            {
                args.Add("-windowed");
                args.Add("-noborder");
            }
            else if (FullscreenRadio.IsChecked == true) 
            {
                args.Add("-fullscreen");
            }
            
            // Graphics settings - Unity specific arguments
            if (ResolutionComboBox.SelectedItem is ComboBoxItem selectedResolution)
            {
                string resolution = selectedResolution.Content.ToString() ?? "";
                string[] res = resolution.Split('x');
                if (res.Length == 2)
                {
                    args.Add($"-screen-width {res[0]}");
                    args.Add($"-screen-height {res[1]}");
                }
            }
            
            if (QualityComboBox.SelectedItem is ComboBoxItem selectedQuality)
            {
                string quality = selectedQuality.Content.ToString() ?? "";
                // Map quality names to Unity quality levels
                string unityQuality = quality.ToLower() switch
                {
                    "ultra" => "5",
                    "high" => "4", 
                    "medium" => "2",
                    "low" => "0",
                    _ => "5"
                };
                args.Add($"-quality {unityQuality}");
            }
            
            if (VSyncCheckBox.IsChecked == true) args.Add("-vsync");
            if (AntiAliasingCheckBox.IsChecked == true) args.Add("-force-d3d11");
            
            // Debug options
            if (DebugModeCheckBox.IsChecked == true) args.Add("-logFile");
            if (ConsoleCheckBox.IsChecked == true) args.Add("-console");
            if (LoggingCheckBox.IsChecked == true) 
            {
                args.Add("-logFile");
                args.Add("-verbose");
            }
            
            // RAM Fix Patches - Apply specific configurations based on selected RAM
            if (GarbageCollectionCheckBox.IsChecked == true)
            {
                string gcArgs = GetGarbageCollectionArgs();
                if (!string.IsNullOrEmpty(gcArgs))
                {
                    args.Add(gcArgs);
                }
            }
            
            if (LargeAddressAwareCheckBox.IsChecked == true)
            {
                args.Add("-force-32bit");
                args.Add("-maxram 4096");
            }
            
            if (MemoryPoolCheckBox.IsChecked == true)
            {
                args.Add("-memory-pool-size 1024");
                args.Add("-memory-pool-granularity 64");
            }
            
            
            // Steam Integration Options
            if (SteamOverlayCheckBox.IsChecked == true) args.Add("-steam-overlay");
            if (SteamAchievementsCheckBox.IsChecked == true) args.Add("-steam-achievements");
            if (SteamCloudCheckBox.IsChecked == true) args.Add("-steam-cloud");
            if (SteamOfflineCheckBox.IsChecked == true) args.Add("-steam-offline");
            if (SteamBigPictureCheckBox.IsChecked == true) args.Add("-steam-bigpicture");
            
            // Multiplayer Options
            if (MultiplayerHostCheckBox.IsChecked == true) args.Add("-host");
            if (MultiplayerClientCheckBox.IsChecked == true) args.Add("-client");
            if (LANModeCheckBox.IsChecked == true) args.Add("-lan");
            if (DedicatedServerCheckBox.IsChecked == true) args.Add("-dedicated-server");
            
            // Server configuration
            if (!string.IsNullOrWhiteSpace(ServerPortTextBox.Text) && int.TryParse(ServerPortTextBox.Text, out int port))
            {
                args.Add($"-port {port}");
            }
            
            if (!string.IsNullOrWhiteSpace(MaxPlayersTextBox.Text) && int.TryParse(MaxPlayersTextBox.Text, out int maxPlayers))
            {
                args.Add($"-maxplayers {maxPlayers}");
            }
            
            // Performance Options
            if (HighPriorityCheckBox.IsChecked == true) args.Add("-high-priority");
            if (DisableFullscreenOptimizationsCheckBox.IsChecked == true) args.Add("-disable-fullscreen-optimizations");
            if (DisableGameDVRCheckBox.IsChecked == true) args.Add("-disable-game-dvr");
            if (DisableWindowsDefenderCheckBox.IsChecked == true) args.Add("-disable-windows-defender");
            
            // Frame rate limiting
            if (FrameRateComboBox.SelectedItem is ComboBoxItem frameRateItem)
            {
                string frameRate = frameRateItem.Content.ToString() ?? "";
                if (frameRate != "Unlimited")
                {
                    string fps = frameRate.Replace(" FPS", "");
                    args.Add($"-target-fps {fps}");
                }
            }
            
            // Custom arguments
            if (!string.IsNullOrWhiteSpace(CustomArgsTextBox.Text))
                args.Add(CustomArgsTextBox.Text);
            
            // Set the arguments
            if (args.Count > 0)
            {
                startInfo.Arguments = string.Join(" ", args);
                System.Diagnostics.Debug.WriteLine($"Launch arguments: {startInfo.Arguments}");
            }

            // Set environment variables for RAM fixes
            if (LargeAddressAwareCheckBox.IsChecked == true)
            {
                startInfo.EnvironmentVariables["UNITY_LARGE_ADDRESS_AWARE"] = "1";
            }
            
            if (MemoryPoolCheckBox.IsChecked == true)
            {
                startInfo.EnvironmentVariables["UNITY_MEMORY_POOL_SIZE"] = "1024";
            }
            
            if (GarbageCollectionCheckBox.IsChecked == true)
            {
                startInfo.EnvironmentVariables["UNITY_GC_OPTIMIZE"] = "1";
            }

            // Additional Unity environment variables
            startInfo.EnvironmentVariables["UNITY_DISABLE_GRAPHICS_JOBS"] = "0";
            startInfo.EnvironmentVariables["UNITY_DISABLE_GRAPHICS_JOBS_LOAD_BALANCING"] = "0";
            
            // Force specific graphics settings
            if (VSyncCheckBox.IsChecked == true)
            {
                startInfo.EnvironmentVariables["UNITY_VSYNC"] = "1";
            }
            
            if (AntiAliasingCheckBox.IsChecked == true)
            {
                startInfo.EnvironmentVariables["UNITY_ANTIALIASING"] = "1";
            }
            
            // Force graphics quality via environment variables
            if (QualityComboBox.SelectedItem is ComboBoxItem qualityItem)
            {
                string quality = qualityItem.Content.ToString() ?? "";
                string unityQuality = quality.ToLower() switch
                {
                    "ultra" => "5",
                    "high" => "4", 
                    "medium" => "2",
                    "low" => "0",
                    _ => "5"
                };
                startInfo.EnvironmentVariables["UNITY_QUALITY_LEVEL"] = unityQuality;
            }
            
            // Force resolution via environment variables
            if (ResolutionComboBox.SelectedItem is ComboBoxItem resolutionItem)
            {
                string resolution = resolutionItem.Content.ToString() ?? "";
                string[] res = resolution.Split('x');
                if (res.Length == 2)
                {
                    startInfo.EnvironmentVariables["UNITY_SCREEN_WIDTH"] = res[0];
                    startInfo.EnvironmentVariables["UNITY_SCREEN_HEIGHT"] = res[1];
                }
            }
            
            // Steam Integration Environment Variables
            if (SteamOverlayCheckBox.IsChecked == true) startInfo.EnvironmentVariables["STEAM_OVERLAY"] = "1";
            if (SteamAchievementsCheckBox.IsChecked == true) startInfo.EnvironmentVariables["STEAM_ACHIEVEMENTS"] = "1";
            if (SteamCloudCheckBox.IsChecked == true) startInfo.EnvironmentVariables["STEAM_CLOUD"] = "1";
            if (SteamOfflineCheckBox.IsChecked == true) startInfo.EnvironmentVariables["STEAM_OFFLINE"] = "1";
            if (SteamBigPictureCheckBox.IsChecked == true) startInfo.EnvironmentVariables["STEAM_BIGPICTURE"] = "1";
            
            // Multiplayer Environment Variables
            if (MultiplayerHostCheckBox.IsChecked == true) startInfo.EnvironmentVariables["MULTIPLAYER_HOST"] = "1";
            if (MultiplayerClientCheckBox.IsChecked == true) startInfo.EnvironmentVariables["MULTIPLAYER_CLIENT"] = "1";
            if (LANModeCheckBox.IsChecked == true) startInfo.EnvironmentVariables["LAN_MODE"] = "1";
            if (DedicatedServerCheckBox.IsChecked == true) startInfo.EnvironmentVariables["DEDICATED_SERVER"] = "1";
            
            // Server configuration environment variables
            if (!string.IsNullOrWhiteSpace(ServerPortTextBox.Text) && int.TryParse(ServerPortTextBox.Text, out int envPort))
            {
                startInfo.EnvironmentVariables["SERVER_PORT"] = envPort.ToString();
            }
            
            if (!string.IsNullOrWhiteSpace(MaxPlayersTextBox.Text) && int.TryParse(MaxPlayersTextBox.Text, out int envMaxPlayers))
            {
                startInfo.EnvironmentVariables["MAX_PLAYERS"] = envMaxPlayers.ToString();
            }
            
            // Performance Environment Variables
            if (HighPriorityCheckBox.IsChecked == true) startInfo.EnvironmentVariables["HIGH_PRIORITY"] = "1";
            if (DisableFullscreenOptimizationsCheckBox.IsChecked == true) startInfo.EnvironmentVariables["DISABLE_FULLSCREEN_OPTIMIZATIONS"] = "1";
            if (DisableGameDVRCheckBox.IsChecked == true) startInfo.EnvironmentVariables["DISABLE_GAME_DVR"] = "1";
            if (DisableWindowsDefenderCheckBox.IsChecked == true) startInfo.EnvironmentVariables["DISABLE_WINDOWS_DEFENDER"] = "1";
            
            // Frame rate limiting environment variable
            if (FrameRateComboBox.SelectedItem is ComboBoxItem frameRateEnvItem)
            {
                string frameRate = frameRateEnvItem.Content.ToString() ?? "";
                if (frameRate != "Unlimited")
                {
                    string fps = frameRate.Replace(" FPS", "");
                    startInfo.EnvironmentVariables["TARGET_FPS"] = fps;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Starting process: {startInfo.FileName}");
            System.Diagnostics.Debug.WriteLine($"Working directory: {startInfo.WorkingDirectory}");
            System.Diagnostics.Debug.WriteLine($"Arguments: {startInfo.Arguments}");

            // Create a batch file for better argument handling
            string batchFile = CreateLaunchBatchFile(startInfo);
            if (!string.IsNullOrEmpty(batchFile))
            {
                var batchStartInfo = new ProcessStartInfo
                {
                    FileName = batchFile,
                    WorkingDirectory = startInfo.WorkingDirectory,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                
                System.Diagnostics.Debug.WriteLine($"Using batch file: {batchFile}");
                gameProcess = Process.Start(batchStartInfo);
            }
            else
            {
                gameProcess = Process.Start(startInfo);
            }
            
            if (gameProcess != null)
            {
                System.Diagnostics.Debug.WriteLine($"Launcher process started with PID: {gameProcess.Id}");
                
                // Create logs directory if it doesn't exist
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                
                string logFile = Path.Combine(logsDir, "INJECT_MODS_CALLED.txt");
                File.AppendAllText(logFile, $"\nLauncher process started with PID: {gameProcess.Id}");
                File.AppendAllText(logFile, $"\nLauncher process name: {gameProcess.ProcessName}");
                File.AppendAllText(logFile, $"\nTarget Castle Story executable: {gameExecutablePath}");
                
                // Don't inject mods immediately - wait for Castle Story to actually start
                // The InjectMods method will handle finding the real Castle Story process
                var selectedMods = availableMods.Where(m => m.IsEnabled).ToList();
                if (selectedMods.Count > 0)
                {
                    // Apply mods by modifying game files before launch
                    Task.Run(() => InjectMods(selectedMods));
                    
                    // Also apply memory patches after game starts
                    Task.Run(async () =>
                    {
                        await Task.Delay(5000); // Wait 5 seconds for Castle Story to fully load
                        await ApplyMemoryPatchesAfterLaunch();
                    });
                }
            }
            else
            {
                throw new Exception("Failed to start game process");
            }
        }

        private string CreateLaunchBatchFile(ProcessStartInfo startInfo)
        {
            try
            {
                string batchPath = Path.Combine(Path.GetTempPath(), "CastleStory_Launch.bat");
                
                var batchContent = new List<string>();
                batchContent.Add("@echo off");
                batchContent.Add("echo Starting Castle Story with custom settings...");
                batchContent.Add("echo.");
                
                // Set environment variables
                foreach (System.Collections.DictionaryEntry envVar in startInfo.EnvironmentVariables)
                {
                    batchContent.Add($"set {envVar.Key}={envVar.Value}");
                }
                
                batchContent.Add("");
                batchContent.Add($"cd /d \"{startInfo.WorkingDirectory}\"");
                batchContent.Add($"\"{startInfo.FileName}\" {startInfo.Arguments}");
                batchContent.Add("");
                batchContent.Add("echo Game has exited.");
                batchContent.Add("pause");
                
                File.WriteAllLines(batchPath, batchContent);
                return batchPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create batch file: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetGarbageCollectionArgs()
        {
            if (Ram8GBRadio.IsChecked == true)
            {
                return "--gc=sgen -heapsize [4194304] -major=marksweep-conc-par -minor=simple-par -nursery-size=524288";
            }
            else if (Ram16GBRadio.IsChecked == true)
            {
                return "--gc=sgen -heapsize [8388608] -major=marksweep-conc-par -minor=simple-par -nursery-size=524288";
            }
            else if (Ram32GBRadio.IsChecked == true)
            {
                return "--gc=sgen -heapsize [16777216] -major=marksweep-conc-par -minor=simple-par -nursery-size=524288";
            }
            
            return "--gc=sgen -heapsize [4194304] -major=marksweep-conc-par -minor=simple-par -nursery-size=524288"; // Default to 8GB
        }

        private ModManager? modManager;

        private Task ApplyMemoryPatchesAfterLaunch()
        {
            try
            {
                // Create logs directory if it doesn't exist
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                
                string logFile = Path.Combine(logsDir, "MEMORY_PATCH_LOG.txt");
                File.WriteAllText(logFile, $"Memory patching started at: {DateTime.Now}\n");
                
                // Find Castle Story process
                var castleStoryProcess = FindCastleStoryProcess(logFile);
                if (castleStoryProcess != null)
                {
                    File.AppendAllText(logFile, $"\nFound Castle Story process: {castleStoryProcess.ProcessName} (PID: {castleStoryProcess.Id})");
                    
                    // Apply memory patches
                    bool patchSuccess = MemoryPatcher.PatchCastleStoryLimits(castleStoryProcess, logsDir);
                    
                    if (patchSuccess)
                    {
                        File.AppendAllText(logFile, $"\n✅ Memory patching successful!");
                        Dispatcher.Invoke(() => UpdateStatus("Memory Patched", "Multiplayer mod applied successfully"));
                    }
                    else
                    {
                        File.AppendAllText(logFile, $"\n❌ Memory patching failed!");
                        Dispatcher.Invoke(() => UpdateStatus("Memory Patch Failed", "Failed to apply multiplayer mod"));
                    }
                }
                else
                {
                    File.AppendAllText(logFile, $"\n❌ Castle Story process not found for memory patching");
                    Dispatcher.Invoke(() => UpdateStatus("Memory Patch Failed", "Castle Story process not found"));
                }
            }
            catch (Exception ex)
            {
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                string logFile = Path.Combine(logsDir, "MEMORY_PATCH_LOG.txt");
                File.AppendAllText(logFile, $"\nError during memory patching: {ex.Message}");
                Dispatcher.Invoke(() => UpdateStatus("Memory Patch Error", $"Error: {ex.Message}"));
            }
            
            return Task.CompletedTask;
        }

        private void InjectMods(List<ModInfo> mods)
        {
            try
            {
                // Initialize mod manager if not already done
                modManager ??= new ModManager();
                
                // Create logs directory if it doesn't exist
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                
                string logFile = Path.Combine(logsDir, "APPLY_MODS_CALLED.txt");
                File.WriteAllText(logFile, $"ApplyMods called at: {DateTime.Now}\nMods count: {mods.Count}");
                File.AppendAllText(logFile, $"\nTarget executable: {gameExecutablePath}");
                
                // Get the game directory from the executable path
                string gameDir = Path.GetDirectoryName(gameExecutablePath) ?? "";
                File.AppendAllText(logFile, $"\nGame directory: {gameDir}");
                
                if (string.IsNullOrEmpty(gameDir) || !Directory.Exists(gameDir))
                {
                    Dispatcher.Invoke(() => UpdateStatus("Mod Application Failed", "Game directory not found"));
                    File.AppendAllText(logFile, $"\n❌ FAILED: Game directory not found: {gameDir}");
                    return;
                }
                
                int successCount = 0;
                int memoryPatchCount = 0;
                
                // Apply each mod using the new mod system
                foreach (var mod in mods)
                {
                    File.AppendAllText(logFile, $"\nProcessing mod: {mod.name}");
                    
                    try
                    {
                        // Normalize mod name
                        string modName = mod.name.ToLower().Replace(" ", "");
                        
                        // Map to actual mod names
                        string actualModName = modName switch
                        {
                            "laddermod" => "LadderMod",
                            "multiplayermod" => "MultiplayerMod",
                            _ => mod.name
                        };
                        
                        File.AppendAllText(logFile, $"\n[DEBUG] Original name: '{mod.name}'");
                        File.AppendAllText(logFile, $"\n[DEBUG] Normalized name: '{modName}'");
                        File.AppendAllText(logFile, $"\n[DEBUG] Actual mod name: '{actualModName}'");
                        
                        var result = modManager.ApplyMod(actualModName, gameDir, logFile);
                        
                        if (result.Success)
                        {
                            successCount++;
                            File.AppendAllText(logFile, $"\n✅ Successfully applied: {mod.name} ({result.IntegrationType})");
                            
                            if (result.IntegrationType == ModIntegrationType.MemoryPatching)
                            {
                                memoryPatchCount++;
                            }
                        }
                        else
                        {
                            File.AppendAllText(logFile, $"\n❌ Failed to apply: {mod.name} - {result.Message}");
                        }
                    }
                    catch (Exception modEx)
                    {
                        File.AppendAllText(logFile, $"\n❌ Error applying mod {mod.name}: {modEx.Message}");
                    }
                }
                
                string statusMessage = successCount > 0 ? 
                    $"Successfully applied {successCount}/{mods.Count} mods" + 
                    (memoryPatchCount > 0 ? $" ({memoryPatchCount} memory patches scheduled)" : "") : 
                    "No mods were applied successfully";
                Dispatcher.Invoke(() => UpdateStatus("Mods Applied", statusMessage));
                File.AppendAllText(logFile, $"\nFinal result: {statusMessage}");
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => UpdateStatus("Mod Application Failed", $"Failed to apply mods: {ex.Message}"));
            }
        }
        
        private bool ApplyMultiplayerMod(string gameDir, string backupDir, string logFile)
        {
            try
            {
                // This is handled by memory patching during game launch
                // For now, just return true as the memory patching happens elsewhere
                File.AppendAllText(logFile, $"\nMultiplayerMod: Will be applied via memory patching during launch");
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nMultiplayerMod error: {ex.Message}");
                return false;
            }
        }
        
        private bool ApplyLadderMod(string gameDir, string backupDir, string logFile)
        {
            try
            {
                string structureFile = Path.Combine(gameDir, "Info", "Lua", "LUI", "Meta", "Meta_Structure.lua");
                
                // Backup original file
                if (File.Exists(structureFile))
                {
                    File.Copy(structureFile, Path.Combine(backupDir, "Meta_Structure.lua"), true);
                    File.AppendAllText(logFile, $"\nBacked up: {structureFile}");
                }
                
                // Add ladder structures to the existing file
                if (File.Exists(structureFile))
                {
                    string ladderData = @"
-- LadderMod: Adding ladder structures
_t.Add(AssetKey.New(""Blueprints"", ""WoodenLadder""),			{ Name = ||GetLocalized(""##gamemenu_constructionwheel_woodenladder""),			Icon = ||IconKeys._Ladder_Wood:Get64(),			Hotkey = ""project_WoodenLadder"",		groupId = 4 })
_t.Add(AssetKey.New(""Blueprints"", ""IronLadder""),				{ Name = ||GetLocalized(""##gamemenu_constructionwheel_ironladder""),				Icon = ||IconKeys._Ladder_Iron:Get64(),			Hotkey = ""project_IronLadder"",		groupId = 4 })
_t.Add(AssetKey.New(""Blueprints"", ""StoneLadder""),			{ Name = ||GetLocalized(""##gamemenu_constructionwheel_stoneladder""),			Icon = ||IconKeys._Ladder_Stone:Get64(),		Hotkey = ""project_StoneLadder"",		groupId = 4 })
_t.Add(AssetKey.New(""Blueprints"", ""RopeLadder""),				{ Name = ||GetLocalized(""##gamemenu_constructionwheel_ropeladder""),				Icon = ||IconKeys._Ladder_Rope:Get64(),			Hotkey = ""project_RopeLadder"",		groupId = 4 })
";
                    
                    // Read the current file content
                    string currentContent = File.ReadAllText(structureFile);
                    
                    // Insert ladder data before the final "return _t" line
                    string updatedContent = currentContent.Replace("return _t", ladderData + "\nreturn _t");
                    
                    // Write the updated content back
                    File.WriteAllText(structureFile, updatedContent);
                    File.AppendAllText(logFile, $"\nAdded ladder structures to {structureFile}");
                }
                else
                {
                    File.AppendAllText(logFile, $"\n❌ Structure file not found: {structureFile}");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nLadderMod error: {ex.Message}");
                return false;
            }
        }

        private Process? FindCastleStoryProcess(string logFile)
        {
            try
            {
                // Look for Castle Story processes with more specific criteria
                string[] processNames = { "Castle Story", "CastleStory", "Unity" };
                
                foreach (string processName in processNames)
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            // Check if this is actually Castle Story by looking at the main window title
                            if (!string.IsNullOrEmpty(process.MainWindowTitle) && 
                                (process.MainWindowTitle.Contains("Castle Story") || 
                                 process.MainWindowTitle.Contains("CastleStory")))
                            {
                                File.AppendAllText(logFile, $"\nFound Castle Story process: {process.ProcessName} (PID: {process.Id}) - {process.MainWindowTitle}");
                                return process;
                            }
                            
                            // Also check by executable path
                            if (process.MainModule != null)
                            {
                                string exePath = process.MainModule.FileName.ToLower();
                                if (exePath.Contains("castle") && exePath.Contains(".exe"))
                                {
                                    File.AppendAllText(logFile, $"\nFound Castle Story by path: {process.ProcessName} (PID: {process.Id}) - {exePath}");
                                    return process;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Ignore errors for individual processes
                            File.AppendAllText(logFile, $"\nError checking process {process.Id}: {ex.Message}");
                        }
                    }
                }
                
                File.AppendAllText(logFile, $"\nNo Castle Story process found");
                return null;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\nError finding Castle Story process: {ex.Message}");
                return null;
            }
        }

        private Process? FindCastleStoryProcessByExecutable(string logFile)
        {
            try
            {
                File.AppendAllText(logFile, $"\nSearching for Castle Story process matching: {gameExecutablePath}");
                
                Process[] processes = Process.GetProcesses();
                
                foreach (Process process in processes)
                {
                    try
                    {
                        // Check if this process matches our target executable exactly
                        if (process.MainModule != null)
                        {
                            string processExePath = process.MainModule.FileName;
                            File.AppendAllText(logFile, $"\nChecking process: {process.ProcessName} (PID: {process.Id}) - {processExePath}");
                            
                            // Compare executable paths (case-insensitive)
                            if (string.Equals(processExePath, gameExecutablePath, StringComparison.OrdinalIgnoreCase))
                            {
                                File.AppendAllText(logFile, $"\n✅ FOUND EXACT MATCH: {process.ProcessName} (PID: {process.Id})");
                                return process;
                            }
                            
                            // Also check if the process name matches Castle Story (but not the launcher)
                            if (process.ProcessName.ToLower().Contains("castle") && 
                                processExePath.ToLower().Contains("castle") &&
                                !processExePath.ToLower().Contains("launcher") &&
                                !processExePath.ToLower().Contains("modding"))
                            {
                                File.AppendAllText(logFile, $"\n✅ FOUND CASTLE STORY PROCESS: {process.ProcessName} (PID: {process.Id})");
                                return process;
                            }
                        }
                        
                        // Check by window title as fallback (but exclude launcher)
                        if (!string.IsNullOrEmpty(process.MainWindowTitle) && 
                            (process.MainWindowTitle.Contains("Castle Story") || 
                             process.MainWindowTitle.Contains("CastleStory")) &&
                            !process.MainWindowTitle.Contains("Launcher") &&
                            !process.MainWindowTitle.Contains("Mod"))
                        {
                            File.AppendAllText(logFile, $"\n✅ FOUND BY WINDOW TITLE: {process.ProcessName} (PID: {process.Id}) - {process.MainWindowTitle}");
                            return process;
                        }
                    }
                    catch
                    {
                        // Ignore processes we can't access
                        continue;
                    }
                }
                
                File.AppendAllText(logFile, $"\n❌ No Castle Story process found matching executable: {gameExecutablePath}");
                return null;
                    }
                    catch (Exception ex)
                    {
                File.AppendAllText(logFile, $"\nError finding Castle Story process by executable: {ex.Message}");
                return null;
            }
        }


        private void StopGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create logs directory if it doesn't exist
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                string logFile = Path.Combine(logsDir, "INJECT_MODS_CALLED.txt");
                
                // Find and kill all Castle Story processes
                Process[] castleStoryProcesses = Process.GetProcessesByName("Castle Story");
                Process[] castleStoryExeProcesses = Process.GetProcessesByName("CastleStory");
                Process[] unityProcesses = Process.GetProcessesByName("Unity");
                
                int killedCount = 0;
                
                // Kill Castle Story processes
                foreach (var process in castleStoryProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                        killedCount++;
                        }
                        catch (Exception ex)
                        {
                        File.AppendAllText(logFile, $"\nError killing Castle Story process {process.Id}: {ex.Message}");
                    }
                }
                
                // Kill CastleStory.exe processes
                foreach (var process in castleStoryExeProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                        killedCount++;
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, $"\nError killing CastleStory.exe process {process.Id}: {ex.Message}");
                    }
                }
                
                // Kill Unity processes (Castle Story uses Unity)
                foreach (var process in unityProcesses)
                {
                    try
                    {
                        // Only kill Unity processes that might be Castle Story
                        if (process.MainWindowTitle.Contains("Castle Story") || 
                            process.ProcessName.Contains("Castle"))
                        {
                            process.Kill();
                            process.WaitForExit(3000);
                            killedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, $"\nError killing Unity process {process.Id}: {ex.Message}");
                    }
                }
                
                // Also try to kill the original game process
            if (gameProcess != null && !gameProcess.HasExited)
            {
                try
                {
                    gameProcess.Kill();
                        gameProcess.WaitForExit(3000);
                        killedCount++;
            }
            catch (Exception ex)
            {
                        File.AppendAllText(logFile, $"\nError killing original game process: {ex.Message}");
                    }
                }
                
                if (killedCount > 0)
                {
                    UpdateStatus("Game Stopped", $"Terminated {killedCount} Castle Story processes");
                }
                else
                {
                    UpdateStatus("No Game Running", "No Castle Story processes found to terminate");
                        }
                    }
                    catch (Exception ex)
                    {
                    UpdateStatus("Stop Failed", $"Failed to stop game: {ex.Message}");
            }
        }

        private void RestartGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                string logFile = Path.Combine(logsDir, "RESTART_GAME.txt");
                File.WriteAllText(logFile, $"Restart Game called at: {DateTime.Now}\n");
                
                UpdateStatus("Restarting", "Stopping current game instance...");
                File.AppendAllText(logFile, "Stopping current game instance...\n");
                
                // Stop the game
                StopGame_Click(sender, e);
                
                // Wait a bit longer to ensure process is fully terminated
                File.AppendAllText(logFile, "Waiting for process to fully terminate...\n");
                Thread.Sleep(3000); // Wait 3 seconds instead of 2
                
                // Double-check that no Castle Story processes are running
                var remainingProcesses = Process.GetProcessesByName("Castle Story");
                if (remainingProcesses.Length > 0)
                {
                    File.AppendAllText(logFile, $"Found {remainingProcesses.Length} remaining Castle Story processes, force killing...\n");
                    foreach (var proc in remainingProcesses)
                    {
                        try
                        {
                            proc.Kill();
                            proc.WaitForExit(5000);
                            File.AppendAllText(logFile, $"Killed process PID: {proc.Id}\n");
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, $"Failed to kill process PID {proc.Id}: {ex.Message}\n");
                        }
                    }
                }
                
                File.AppendAllText(logFile, "Starting fresh game instance...\n");
                UpdateStatus("Restarting", "Starting fresh game instance...");
                
                // Use a timer to delay the restart
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1); // Reduced delay since we already waited
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    File.AppendAllText(logFile, "Launching new game instance...\n");
                    LaunchGame_Click(sender, e);
                };
                timer.Start();
                
                File.AppendAllText(logFile, $"Restart process completed at: {DateTime.Now}\n");
                    }
                    catch (Exception ex)
                    {
                UpdateStatus("Restart Error", $"Failed to restart game: {ex.Message}");
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                string logFile = Path.Combine(logsDir, "RESTART_GAME_ERROR.txt");
                File.WriteAllText(logFile, $"Restart error at {DateTime.Now}: {ex.Message}\nStack trace: {ex.StackTrace}\n");
            }
        }

        private void ForceRestart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                string logFile = Path.Combine(logsDir, "FORCE_RESTART.txt");
                File.WriteAllText(logFile, $"Force Clean Restart called at: {DateTime.Now}\n");
                
                UpdateStatus("Force Restart", "Performing thorough cleanup...");
                File.AppendAllText(logFile, "Performing thorough cleanup...\n");
                
                // Kill ALL Castle Story related processes
                var castleStoryProcesses = Process.GetProcessesByName("Castle Story");
                foreach (var proc in castleStoryProcesses)
                {
                    try
                    {
                        File.AppendAllText(logFile, $"Force killing Castle Story process PID: {proc.Id}\n");
                        proc.Kill();
                        proc.WaitForExit(5000);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, $"Failed to kill Castle Story PID {proc.Id}: {ex.Message}\n");
                    }
                }
                
                // Also kill any Unity processes that might be related
                var unityProcesses = Process.GetProcessesByName("Unity");
                foreach (var proc in unityProcesses)
                {
                    try
                    {
                        if (proc.MainModule?.FileName?.Contains("Castle Story") == true)
                        {
                            File.AppendAllText(logFile, $"Force killing Unity process PID: {proc.Id}\n");
                            proc.Kill();
                            proc.WaitForExit(5000);
                            }
                        }
                        catch (Exception ex)
                        {
                        File.AppendAllText(logFile, $"Failed to kill Unity PID {proc.Id}: {ex.Message}\n");
                    }
                }
                
                // Wait longer to ensure all processes are terminated
                File.AppendAllText(logFile, "Waiting for all processes to terminate...\n");
                Thread.Sleep(5000);
                
                // Clear any potential Steam connection issues by restarting Steam processes
                File.AppendAllText(logFile, "Checking for Steam processes...\n");
                var steamProcesses = Process.GetProcessesByName("steam");
                if (steamProcesses.Length > 0)
                {
                    File.AppendAllText(logFile, "Steam is running, this should help with connection issues\n");
                }
                
                File.AppendAllText(logFile, "Starting fresh game instance...\n");
                UpdateStatus("Force Restart", "Starting fresh game instance...");
                
                // Launch the game
                LaunchGame_Click(sender, e);
                
                File.AppendAllText(logFile, $"Force restart completed at: {DateTime.Now}\n");
            }
            catch (Exception ex)
            {
                UpdateStatus("Force Restart Error", $"Failed to force restart: {ex.Message}");
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                string logFile = Path.Combine(logsDir, "FORCE_RESTART_ERROR.txt");
                File.WriteAllText(logFile, $"Force restart error at {DateTime.Now}: {ex.Message}\nStack trace: {ex.StackTrace}\n");
            }
        }


        private void LaunchLANServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                string logFile = Path.Combine(logsDir, "LAUNCH_LAN_SERVER.txt");
                File.WriteAllText(logFile, $"Launch LAN Server called at: {DateTime.Now}\n");
                
                UpdateStatus("Launching LAN Server", "Starting Castle Story LAN Server...");
                File.AppendAllText(logFile, "Starting Castle Story LAN Server...\n");
                
                string serverPath = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "LANServer");
                string batchFile = Path.Combine(serverPath, "LaunchLANServer.bat");
                
                if (File.Exists(batchFile))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = batchFile,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WorkingDirectory = serverPath
                    };
                    
                    var serverProcess = Process.Start(startInfo);
                    if (serverProcess != null)
                    {
                        File.AppendAllText(logFile, $"LAN Server launched (PID: {serverProcess.Id})\n");
                        UpdateStatus("LAN Server Running", "Castle Story LAN Server is running on port 7777");
                    }
                    else
                    {
                        File.AppendAllText(logFile, "Failed to launch LAN Server\n");
                        UpdateStatus("LAN Server Failed", "Failed to launch LAN Server");
                    }
                }
                else
                {
                    File.AppendAllText(logFile, $"LAN Server batch file not found: {batchFile}\n");
                    UpdateStatus("LAN Server Failed", "LAN Server files not found");
                }
                
                File.AppendAllText(logFile, $"Launch LAN Server completed at: {DateTime.Now}\n");
            }
            catch (Exception ex)
            {
                UpdateStatus("LAN Server Error", $"Failed to launch LAN Server: {ex.Message}");
            }
        }

        private void LaunchLANClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logsDir = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                string logFile = Path.Combine(logsDir, "LAUNCH_LAN_CLIENT.txt");
                File.WriteAllText(logFile, $"Launch LAN Client called at: {DateTime.Now}\n");
                
                UpdateStatus("Launching LAN Client", "Starting Castle Story LAN Client...");
                File.AppendAllText(logFile, "Starting Castle Story LAN Client...\n");
                
                string clientPath = Path.Combine(@"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool", "LANClient");
                string batchFile = Path.Combine(clientPath, "LaunchLANClient.bat");
                
                if (File.Exists(batchFile))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = batchFile,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WorkingDirectory = clientPath
                    };
                    
                    var clientProcess = Process.Start(startInfo);
                    if (clientProcess != null)
                    {
                        File.AppendAllText(logFile, $"LAN Client launched (PID: {clientProcess.Id})\n");
                        UpdateStatus("LAN Client Running", "Castle Story LAN Client is running");
                    }
                    else
                    {
                        File.AppendAllText(logFile, "Failed to launch LAN Client\n");
                        UpdateStatus("LAN Client Failed", "Failed to launch LAN Client");
                    }
                }
                else
                {
                    File.AppendAllText(logFile, $"LAN Client batch file not found: {batchFile}\n");
                    UpdateStatus("LAN Client Failed", "LAN Client files not found");
                }
                
                File.AppendAllText(logFile, $"Launch LAN Client completed at: {DateTime.Now}\n");
            }
            catch (Exception ex)
            {
                UpdateStatus("LAN Client Error", $"Failed to launch LAN Client: {ex.Message}");
            }
        }

        private void LaunchLuaEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Launching Lua Editor", "Starting Castle Story Lua Editor...");
                
                // Create Lua Editor window
                var luaEditor = new LuaEditorWindow();
                luaEditor.Show();
                
                UpdateStatus("Lua Editor Launched", "Castle Story Lua Editor is open");
            }
            catch (Exception ex)
            {
                UpdateStatus("Lua Editor Error", $"Failed to launch Lua Editor: {ex.Message}");
            }
        }

        private void OpenMemoryPatchEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Opening", "Launching Memory Patch Editor...");
                
                var memoryPatchEditor = new MemoryPatchEditor();
                memoryPatchEditor.ConfigurationSaved += (s, config) => {
                    UpdateStatus("Memory Patch", "Configuration saved successfully");
                };
                
                var window = new Window
                {
                    Title = "Memory Patch Configuration",
                    Content = memoryPatchEditor,
                    Width = 1000,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
                };
                
                window.ShowDialog();
                
                UpdateStatus("Success", "Memory Patch Editor closed");
            }
            catch (Exception ex)
            {
                UpdateStatus("Error", $"Failed to open Memory Patch Editor: {ex.Message}");
                Logger.LogError($"Failed to open Memory Patch Editor: {ex.Message}", ex, "MainWindow");
            }
        }

        private void OpenTeamManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Opening", "Launching Team Manager...");
                
                var teamManagerEditor = new TeamManagerEditor();
                teamManagerEditor.ConfigurationSaved += (s, teamManager) => {
                    UpdateStatus("Team Manager", "Configuration saved successfully");
                };
                
                var window = new Window
                {
                    Title = "Team Management",
                    Content = teamManagerEditor,
                    Width = 1200,
                    Height = 800,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
                };
                
                window.ShowDialog();
                
                UpdateStatus("Success", "Team Manager closed");
            }
            catch (Exception ex)
            {
                UpdateStatus("Error", $"Failed to open Team Manager: {ex.Message}");
                Logger.LogError($"Failed to open Team Manager: {ex.Message}", ex, "MainWindow");
            }
        }

        private void UpdateStatus(string action, string message)
        {
            try
            {
                if (Dispatcher.CheckAccess())
                {
                    StatusText.Text = $"{action}: {message}";
                    System.Diagnostics.Debug.WriteLine($"[{action}] {message}");
                    Logger.LogInfo($"{action}: {message}", "MainWindow");
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusText.Text = $"{action}: {message}";
                        System.Diagnostics.Debug.WriteLine($"[{action}] {message}");
                        Logger.LogInfo($"{action}: {message}", "MainWindow");
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to update status: {action} - {message}", ex, "MainWindow");
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsPath = "launcher_settings.json";
                if (File.Exists(settingsPath))
                {
                    string settingsJson = File.ReadAllText(settingsPath);
                    var settings = JsonConvert.DeserializeObject<LauncherSettings>(settingsJson);
                    
                    if (settings != null)
                    {
                        GameDirectoryTextBox.Text = settings.GameDirectory ?? "";
                        ModDirectoryTextBox.Text = settings.ModsDirectory ?? "Mods";
                        // Load display mode (convert from old checkbox settings)
                        if (settings.Fullscreen) FullscreenRadio.IsChecked = true;
                        else if (settings.Windowed) WindowedRadio.IsChecked = true;
                        else if (settings.Borderless) BorderlessRadio.IsChecked = true;
                        else FullscreenRadio.IsChecked = true; // Default
                        VSyncCheckBox.IsChecked = settings.VSync;
                        AntiAliasingCheckBox.IsChecked = settings.AntiAliasing;
                        DebugModeCheckBox.IsChecked = settings.DebugMode;
                        ConsoleCheckBox.IsChecked = settings.Console;
                        LoggingCheckBox.IsChecked = settings.Logging;
                        CustomArgsTextBox.Text = settings.CustomArgs ?? "";
                        
                        // Load RAM Fix Patches
                        if (settings.SelectedRAM == 8) Ram8GBRadio.IsChecked = true;
                        else if (settings.SelectedRAM == 16) Ram16GBRadio.IsChecked = true;
                        else if (settings.SelectedRAM == 32) Ram32GBRadio.IsChecked = true;
                        else Ram8GBRadio.IsChecked = true; // Default to 8GB
                        
                        LargeAddressAwareCheckBox.IsChecked = settings.LargeAddressAware;
                        MemoryPoolCheckBox.IsChecked = settings.MemoryPool;
                        GarbageCollectionCheckBox.IsChecked = settings.GarbageCollection;
                        
                        if (!string.IsNullOrEmpty(settings.Resolution))
                        {
                            foreach (ComboBoxItem item in ResolutionComboBox.Items)
                            {
                                if (item.Content.ToString() == settings.Resolution)
                                {
                                    ResolutionComboBox.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(settings.Quality))
                        {
                            foreach (ComboBoxItem item in QualityComboBox.Items)
                            {
                                if (item.Content.ToString() == settings.Quality)
                                {
                                    QualityComboBox.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                        
                        ValidateGamePath();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new LauncherSettings
                {
                    GameDirectory = GameDirectoryTextBox.Text,
                    ModsDirectory = ModDirectoryTextBox.Text,
                    Fullscreen = FullscreenRadio.IsChecked == true,
                    Windowed = WindowedRadio.IsChecked == true,
                    Borderless = BorderlessRadio.IsChecked == true,
                    VSync = VSyncCheckBox.IsChecked == true,
                    AntiAliasing = AntiAliasingCheckBox.IsChecked == true,
                    DebugMode = DebugModeCheckBox.IsChecked == true,
                    Console = ConsoleCheckBox.IsChecked == true,
                    Logging = LoggingCheckBox.IsChecked == true,
                    CustomArgs = CustomArgsTextBox.Text,
                    Resolution = (ResolutionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Quality = (QualityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    
                    // Save RAM Fix Patches
                    SelectedRAM = Ram8GBRadio.IsChecked == true ? 8 : (Ram16GBRadio.IsChecked == true ? 16 : 32),
                    LargeAddressAware = LargeAddressAwareCheckBox.IsChecked == true,
                    MemoryPool = MemoryPoolCheckBox.IsChecked == true,
                    GarbageCollection = GarbageCollectionCheckBox.IsChecked == true,
                };
                
                string settingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("launcher_settings.json", settingsJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                statusTimer?.Stop();
                SaveSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
            
            base.OnClosed(e);
        }
    }

    public class ModInfo
    {
        public string name { get; set; } = "";
        public string version { get; set; } = "1.0.0";
        public string author { get; set; } = "Unknown";
        public string description { get; set; } = "No description";
        public bool IsEnabled { get; set; } = true;
        public string modPath { get; set; } = "";
        public string configPath { get; set; } = "";
        public DateTime lastModified { get; set; } = DateTime.Now;
        public long fileSize { get; set; } = 0;
        public List<string> dependencies { get; set; } = new List<string>();
        public List<string> conflicts { get; set; } = new List<string>();
        public List<string> features { get; set; } = new List<string>();
        public Dictionary<string, string> settings { get; set; } = new Dictionary<string, string>();
        public ModStatus status { get; set; } = ModStatus.Ready;
        public string errorMessage { get; set; } = "";
        public ModCategory category { get; set; } = ModCategory.General;
        public int priority { get; set; } = 0; // Higher number = higher priority
        public bool isRequired { get; set; } = false;
        public bool isCompatible { get; set; } = true;
        public string minimumGameVersion { get; set; } = "";
        public string maximumGameVersion { get; set; } = "";
    }

    public enum ModStatus
    {
        Ready,
        Loading,
        Error,
        Disabled,
        Incompatible,
        MissingDependencies,
        Conflicting
    }

    public enum ModCategory
    {
        General,
        Graphics,
        Gameplay,
        Multiplayer,
        UI,
        Audio,
        Performance,
        Utility,
        Experimental
    }

    public class LauncherSettings
    {
        public string? GameDirectory { get; set; }
        public string? ModsDirectory { get; set; }
        public bool Fullscreen { get; set; } = true;
        public bool Windowed { get; set; } = false;
        public bool Borderless { get; set; } = false;
        public bool VSync { get; set; } = true;
        public bool AntiAliasing { get; set; } = true;
        public bool DebugMode { get; set; } = false;
        public bool Console { get; set; } = false;
        public bool Logging { get; set; } = false;
        public string? CustomArgs { get; set; }
        public string? Resolution { get; set; } = "1920x1080";
        public string? Quality { get; set; } = "Ultra";
        
        // RAM Fix Patches
        public int SelectedRAM { get; set; } = 8; // 8, 16, or 32
        public bool LargeAddressAware { get; set; } = true;
        public bool MemoryPool { get; set; } = true;
        public bool GarbageCollection { get; set; } = true;

        private void ApplySelectedMods_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get selected mods from the list
                var selectedMods = new List<string>();
                
                // For now, just show a message - full implementation will come later
                MessageBox.Show($"Apply Selected Mods functionality will be implemented soon!\n\nThis will apply all selected mods to the game.", 
                    "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying mods: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnapplyAllMods_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to unapply ALL mods? This will disable all mods and restore the original game files.",
                    "Confirm Unapply All Mods",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // For now, just show a message - full implementation will come later
                    MessageBox.Show("Unapply All Mods functionality will be implemented soon!\n\nThis will remove all applied mods from the game.", 
                        "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unapplying mods: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}