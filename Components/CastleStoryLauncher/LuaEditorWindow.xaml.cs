using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace CastleStoryLauncher
{
    public partial class LuaEditorWindow : Window
    {
        private string currentFilePath = "";
        private string gameDirectory = "";
        private bool hasUnsavedChanges = false;
        private string originalContent = "";
        private bool isEasyMode = true;
        private readonly string[] supportedExtensions = { ".lua", ".txt", ".csv", ".json", ".xml", ".png" };

        public LuaEditorWindow()
        {
            InitializeComponent();
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            // Set default game directory
            gameDirectory = @"C:\Users\wolf0\OneDrive\Desktop\CASTLE STORY\Modded Castle Story\Castle Story";
            GameDirectoryTextBox.Text = gameDirectory;
            
            // Load supported files
            RefreshFiles();
            
            // Set up text change detection
            CodeEditorTextBox.TextChanged += (s, e) => {
                hasUnsavedChanges = (CodeEditorTextBox.Text != originalContent);
                UpdateStatus();
                UpdateLineNumbers();
            };
            
            // Initialize Easy Mode after UI is fully loaded
            this.Loaded += (s, e) => {
                SwitchToEasyMode();
            };
        }

        private void ModeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (EasyModeRadio.IsChecked == true)
            {
                SwitchToEasyMode();
            }
            else if (AdvancedModeRadio.IsChecked == true)
            {
                SwitchToAdvancedMode();
            }
        }

        private void SwitchToEasyMode()
        {
            try
            {
                isEasyMode = true;
                if (EasyModePanel != null)
                    EasyModePanel.Visibility = Visibility.Visible;
                if (AdvancedModePanel != null)
                    AdvancedModePanel.Visibility = Visibility.Collapsed;
                LoadEasyModeSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SwitchToEasyMode: {ex.Message}");
                // Don't rethrow, just log the error
            }
        }

        private void SwitchToAdvancedMode()
        {
            try
            {
                isEasyMode = false;
                if (EasyModePanel != null)
                    EasyModePanel.Visibility = Visibility.Collapsed;
                if (AdvancedModePanel != null)
                    AdvancedModePanel.Visibility = Visibility.Visible;
                UpdateLineNumbers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SwitchToAdvancedMode: {ex.Message}");
                // Don't rethrow, just log the error
            }
        }

        private void LoadEasyModeSettings()
        {
            // Clear existing dynamic controls
            ClearDynamicEasyModeControls();
            
            // Load current values from the loaded file
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                var content = File.ReadAllText(currentFilePath);
                var extension = Path.GetExtension(currentFilePath).ToLower();
                
                // Determine file type and parse accordingly
                switch (extension)
                {
                    case ".lua":
                        ParseLuaSettings(content);
                        break;
                    case ".json":
                        ParseJsonSettings(content);
                        break;
                    case ".xml":
                        ParseXmlSettings(content);
                        break;
                    case ".csv":
                        ParseCsvSettings(content);
                        break;
                    default:
                        ShowBasicTextEditor();
                        break;
                }
            }
            else
            {
                // Show file type selection if no file is loaded
                ShowFileTypeSelection();
            }
        }

        private void BrowseGame_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Castle Story Game Directory",
                InitialDirectory = gameDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                gameDirectory = dialog.FolderName;
                GameDirectoryTextBox.Text = gameDirectory;
                RefreshFiles();
            }
        }

        private void RefreshFiles_Click(object sender, RoutedEventArgs e)
        {
            RefreshFiles();
        }

        private void RefreshFiles()
        {
            try
            {
                FileListBox.Items.Clear();
                
                if (!Directory.Exists(gameDirectory))
                {
                    UpdateStatus("Game directory not found", false);
                    return;
                }

                // Find all supported files recursively
                var allFiles = new List<string>();
                foreach (var extension in supportedExtensions)
                {
                    var files = Directory.GetFiles(gameDirectory, $"*{extension}", SearchOption.AllDirectories)
                        .Select(f => Path.GetRelativePath(gameDirectory, f))
                        .ToList();
                    allFiles.AddRange(files);
                }

                allFiles = allFiles.OrderBy(f => f).ToList();

                foreach (var file in allFiles)
                {
                    FileListBox.Items.Add(file);
                }

                UpdateStatus($"Found {allFiles.Count} supported files", true);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading files: {ex.Message}", false);
            }
        }

        private void FileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileListBox.SelectedItem != null)
            {
                LoadFile(FileListBox.SelectedItem.ToString());
            }
        }

        private void LoadFile(string relativePath)
        {
            try
            {
                // Check for unsaved changes
                if (hasUnsavedChanges)
                {
                    var result = MessageBox.Show(
                        "You have unsaved changes. Do you want to save them before loading a new file?",
                        "Unsaved Changes",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveCurrentFile();
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                currentFilePath = Path.Combine(gameDirectory, relativePath);
                var extension = Path.GetExtension(relativePath).ToLower();
                
                if (extension == ".png")
                {
                    HandleImageFile(relativePath);
                }
                else
                {
                    var content = File.ReadAllText(currentFilePath);
                    
                    CodeEditorTextBox.Text = content;
                    originalContent = content;
                    hasUnsavedChanges = false;
                    
                    CurrentFileLabel.Text = $"Editing: {relativePath}";
                    FileStatusLabel.Text = "File loaded";
                    
                    UpdateStatus($"Loaded: {relativePath}", true);
                    UpdateLineNumbers();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading file: {ex.Message}", false);
            }
        }

        private void HandleImageFile(string relativePath)
        {
            try
            {
                var result = MessageBox.Show(
                    $"This is an image file ({relativePath}). Would you like to replace it with a new image from your computer?",
                    "Image File",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = "Select New Image",
                        Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        var newImagePath = dialog.FileName;
                        var targetPath = Path.Combine(gameDirectory, relativePath);
                        
                        File.Copy(newImagePath, targetPath, true);
                        UpdateStatus($"Image replaced: {relativePath}", true);
                        FileStatusLabel.Text = "Image replaced";
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error handling image: {ex.Message}", false);
            }
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Create New Lua File",
                Filter = "Lua files (*.lua)|*.lua|All files (*.*)|*.*",
                InitialDirectory = gameDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                currentFilePath = dialog.FileName;
                CodeEditorTextBox.Text = "-- New Lua file\n-- Created: " + DateTime.Now.ToString() + "\n\n";
                originalContent = CodeEditorTextBox.Text;
                hasUnsavedChanges = false;
                
                CurrentFileLabel.Text = $"New file: {Path.GetFileName(currentFilePath)}";
                FileStatusLabel.Text = "New file";
                
                UpdateStatus("New file created", true);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentFile();
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Lua File As",
                Filter = "Lua files (*.lua)|*.lua|All files (*.*)|*.*",
                InitialDirectory = gameDirectory,
                FileName = Path.GetFileName(currentFilePath)
            };

            if (dialog.ShowDialog() == true)
            {
                currentFilePath = dialog.FileName;
                SaveCurrentFile();
            }
        }

        private void SaveCurrentFile()
        {
            try
            {
                if (string.IsNullOrEmpty(currentFilePath))
                {
                    SaveAsButton_Click(null, null);
                    return;
                }

                File.WriteAllText(currentFilePath, CodeEditorTextBox.Text);
                originalContent = CodeEditorTextBox.Text;
                hasUnsavedChanges = false;
                
                FileStatusLabel.Text = "Saved";
                UpdateStatus($"Saved: {Path.GetFileName(currentFilePath)}", true);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error saving file: {ex.Message}", false);
            }
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var content = CodeEditorTextBox.Text;
                var extension = Path.GetExtension(currentFilePath).ToLower();
                var validationResults = new List<string>();
                bool isValid = true;
                
                switch (extension)
                {
                    case ".lua":
                        isValid = ValidateLuaFile(content, validationResults);
                        break;
                    case ".json":
                        isValid = ValidateJsonFile(content, validationResults);
                        break;
                    case ".xml":
                        isValid = ValidateXmlFile(content, validationResults);
                        break;
                    case ".csv":
                        isValid = ValidateCsvFile(content, validationResults);
                        break;
                    case ".txt":
                        isValid = ValidateTextFile(content, validationResults);
                        break;
                    default:
                        validationResults.Add($"No specific validation available for {extension} files");
                        break;
                }
                
                var resultMessage = string.Join("\n", validationResults);
                if (isValid)
                {
                    UpdateStatus($"✅ Validation passed: {resultMessage}", true);
                    FileStatusLabel.Text = $"Valid {extension.ToUpper()}";
                }
                else
                {
                    UpdateStatus($"❌ Validation failed: {resultMessage}", false);
                    FileStatusLabel.Text = "Validation errors";
                    MessageBox.Show($"Validation failed:\n{resultMessage}", "File Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Validation error: {ex.Message}", false);
            }
        }

        private List<string> ValidateLuaSyntax(string content)
        {
            var errors = new List<string>();
            var lines = content.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                var lineNumber = i + 1;
                
                // Check for common Lua syntax issues
                if (line.StartsWith("--") || string.IsNullOrEmpty(line))
                    continue;
                
                // Check for unmatched quotes
                var singleQuotes = line.Count(c => c == '\'');
                var doubleQuotes = line.Count(c => c == '"');
                
                if (singleQuotes % 2 != 0)
                {
                    errors.Add($"Line {lineNumber}: Unmatched single quotes");
                }
                
                if (doubleQuotes % 2 != 0)
                {
                    errors.Add($"Line {lineNumber}: Unmatched double quotes");
                }
                
                // Check for basic syntax patterns
                if (line.Contains("if") && !line.Contains("then"))
                {
                    errors.Add($"Line {lineNumber}: 'if' statement missing 'then'");
                }
                
                if (line.Contains("function") && !line.Contains("end"))
                {
                    // Check if there's a matching 'end' later
                    bool hasEnd = false;
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].Trim().StartsWith("end"))
                        {
                            hasEnd = true;
                            break;
                        }
                    }
                    if (!hasEnd)
                    {
                        errors.Add($"Line {lineNumber}: 'function' statement missing 'end'");
                    }
                }
            }
            
            return errors;
        }
        
        private bool ValidateLuaFile(string content, List<string> results)
        {
            var errors = ValidateLuaSyntax(content);
            if (errors.Count == 0)
            {
                results.Add("Lua syntax is valid");
                return true;
            }
            else
            {
                results.AddRange(errors);
                return false;
            }
        }
        
        private bool ValidateJsonFile(string content, List<string> results)
        {
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(content);
                results.Add("JSON syntax is valid");
                return true;
            }
            catch (System.Text.Json.JsonException ex)
            {
                results.Add($"JSON error: {ex.Message}");
                return false;
            }
        }
        
        private bool ValidateXmlFile(string content, List<string> results)
        {
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(content);
                results.Add("XML syntax is valid");
                return true;
            }
            catch (System.Xml.XmlException ex)
            {
                results.Add($"XML error: {ex.Message}");
                return false;
            }
        }
        
        private bool ValidateCsvFile(string content, List<string> results)
        {
            var lines = content.Split('\n');
            if (lines.Length == 0)
            {
                results.Add("CSV file is empty");
                return true;
            }
            
            var firstLineColumns = lines[0].Split(',').Length;
            var inconsistentRows = 0;
            
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                
                var columns = lines[i].Split(',').Length;
                if (columns != firstLineColumns)
                {
                    inconsistentRows++;
                }
            }
            
            if (inconsistentRows > 0)
            {
                results.Add($"CSV has {inconsistentRows} rows with inconsistent column count");
                return false;
            }
            
            results.Add($"CSV has {lines.Length} rows with {firstLineColumns} columns");
            return true;
        }
        
        private bool ValidateTextFile(string content, List<string> results)
        {
            results.Add($"Text file has {content.Length} characters, {content.Split('\n').Length} lines");
            return true;
        }
        
        // Dynamic Easy Mode Controls Management
        private List<FrameworkElement> dynamicControls = new List<FrameworkElement>();
        
        private void ClearDynamicEasyModeControls()
        {
            foreach (var control in dynamicControls)
            {
                if (control.Parent is Panel panel)
                {
                    panel.Children.Remove(control);
                }
            }
            dynamicControls.Clear();
        }
        
        private void ShowFileTypeSelection()
        {
            ClearDynamicEasyModeControls();
            
            var typeSelectionGroup = new GroupBox
            {
                Header = "Select File Type to Create",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            var gamemodeButton = new Button
            {
                Content = "🎮 Gamemode (config.lua)",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219))
            };
            gamemodeButton.Click += (s, e) => CreateGamemodeTemplate();
            
            var presetsButton = new Button
            {
                Content = "⚙️ Presets (presets.lua)",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113))
            };
            presetsButton.Click += (s, e) => CreatePresetsTemplate();
            
            var bricktronButton = new Button
            {
                Content = "🤖 Bricktron Names (names.lua)",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(155, 89, 182))
            };
            bricktronButton.Click += (s, e) => CreateBricktronNamesTemplate();
            
            var languageButton = new Button
            {
                Content = "🌐 Language/Translations (lang.lua)",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(241, 196, 15))
            };
            languageButton.Click += (s, e) => CreateLanguageTemplate();
            
            stackPanel.Children.Add(gamemodeButton);
            stackPanel.Children.Add(presetsButton);
            stackPanel.Children.Add(bricktronButton);
            stackPanel.Children.Add(languageButton);
            
            typeSelectionGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(typeSelectionGroup);
            }
            dynamicControls.Add(typeSelectionGroup);
        }
        
        private void ParseLuaSettings(string content)
        {
            ClearDynamicEasyModeControls();
            
            // Check if this is a config.lua file (gamemode)
            if (content.Contains("sv_Settings") || content.Contains("bricktronCap"))
            {
                ParseGamemodeConfig(content);
            }
            else if (content.Contains("Characters") || content.Contains("Bricktron"))
            {
                ParseBricktronNames(content);
            }
            else if (content.Contains("Language") || content.Contains("Translations"))
            {
                ParseLanguageFile(content);
            }
            else
            {
                ShowBasicLuaEditor(content);
            }
        }
        
        private void ParseGamemodeConfig(string content)
        {
            var configGroup = new GroupBox
            {
                Header = "🎮 Gamemode Configuration",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            // Parse sv_Settings table
            var settings = ParseLuaTable(content, "sv_Settings");
            
            // Raid Management Section
            var raidGroup = new GroupBox { Header = "Raid Management", Margin = new Thickness(0, 5, 0, 5) };
            var raidPanel = new StackPanel();
            
            AddNumericField(raidPanel, "Player Attack Interval", "playerAttackInterval", settings, "600");
            AddNumericField(raidPanel, "First Wave Duration Bonus", "firstWaveDurationBonus", settings, "300");
            AddNumericField(raidPanel, "Maximum Enemy Level", "maximumEnemyLevel", settings, "10");
            AddNumericField(raidPanel, "Initial Enemy Level", "initialEnemyLevel", settings, "0");
            AddNumericField(raidPanel, "Level Clock Interval", "levelClockInterval", settings, "480");
            AddNumericField(raidPanel, "Neutral Attack Interval", "neutralAttackInterval", settings, "60");
            AddNumericField(raidPanel, "Starting Corrupt Crystals", "startingCorruptCrystals", settings, "2");
            AddBooleanField(raidPanel, "Force Player Fireflies to Player Crystal", "forcePlayerFirefliesToPlayerCrystal", settings, "true");
            AddNumericField(raidPanel, "Corruptron Cap", "corruptronCap", settings, "50");
            AddNumericField(raidPanel, "Base Corruptron Offense", "baseCorruptronOffense", settings, "6");
            AddNumericField(raidPanel, "Base Corruptron Defense", "baseCorruptronDefense", settings, "6");
            AddNumericField(raidPanel, "Offense Increase Per Level", "offenseIncreasePerLevel", settings, "3");
            AddNumericField(raidPanel, "Defense Increase Per Level", "defenseIncreasePerLevel", settings, "3");
            AddBooleanField(raidPanel, "Random Corruptron Capture", "randomCorruptronCapture", settings, "false");
            
            raidGroup.Content = raidPanel;
            stackPanel.Children.Add(raidGroup);
            
            // Resources Section
            var resourcesGroup = new GroupBox { Header = "Resources", Margin = new Thickness(0, 5, 0, 5) };
            var resourcesPanel = new StackPanel();
            
            AddNumericField(resourcesPanel, "Bricktron Cap", "bricktronCap", settings, "100");
            AddNumericField(resourcesPanel, "Starting Workers Count", "startingWorkersCount", settings, "10");
            AddNumericField(resourcesPanel, "Starting Knight Count", "startingKnightCount", settings, "2");
            AddNumericField(resourcesPanel, "Starting Archer Count", "startingArcherCount", settings, "2");
            AddNumericField(resourcesPanel, "Firefly Cost Multiplier", "fireflyCostMultiplier", settings, "0.2");
            
            resourcesGroup.Content = resourcesPanel;
            stackPanel.Children.Add(resourcesGroup);
            
            // Global Settings Section
            var globalGroup = new GroupBox { Header = "Global Settings", Margin = new Thickness(0, 5, 0, 5) };
            var globalPanel = new StackPanel();
            
            AddBooleanField(globalPanel, "Can Dig Ground", "canDigGround", settings, "true");
            AddNumericField(globalPanel, "Player Relations", "playerRelations", settings, "2");
            
            globalGroup.Content = globalPanel;
            stackPanel.Children.Add(globalGroup);
            
            // Time of Day Section
            var timeGroup = new GroupBox { Header = "Time of Day", Margin = new Thickness(0, 5, 0, 5) };
            var timePanel = new StackPanel();
            
            AddNumericField(timePanel, "Starting Time of Day", "startingTimeOfDay", settings, "7");
            AddNumericField(timePanel, "Day/Night Cycle Setting", "daynightCycleSetting", settings, "0");
            AddNumericField(timePanel, "Daytime Factor", "daytimeFactor", settings, "1.4");
            AddNumericField(timePanel, "Nighttime Factor", "nighttimeFactor", settings, "0.6");
            AddBooleanField(timePanel, "Pause Time of Day", "pauseTimeOfDay", settings, "false");
            
            timeGroup.Content = timePanel;
            stackPanel.Children.Add(timeGroup);
            
            configGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(configGroup);
            }
            dynamicControls.Add(configGroup);
        }
        
        private void AddNumericField(Panel parent, string label, string key, Dictionary<string, string> settings, string defaultValue)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            
            var labelControl = new TextBlock
            {
                Text = label + ":",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetColumn(labelControl, 0);
            
            var textBox = new TextBox
            {
                Text = settings.ContainsKey(key) ? settings[key] : defaultValue,
                Margin = new Thickness(5),
                Tag = key
            };
            Grid.SetColumn(textBox, 1);
            
            grid.Children.Add(labelControl);
            grid.Children.Add(textBox);
            parent.Children.Add(grid);
            
            dynamicControls.Add(grid);
        }
        
        private void AddBooleanField(Panel parent, string label, string key, Dictionary<string, string> settings, string defaultValue)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            
            var labelControl = new TextBlock
            {
                Text = label + ":",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetColumn(labelControl, 0);
            
            var checkBox = new CheckBox
            {
                IsChecked = (settings.ContainsKey(key) ? settings[key] : defaultValue).ToLower() == "true",
                Margin = new Thickness(5),
                Tag = key
            };
            Grid.SetColumn(checkBox, 1);
            
            grid.Children.Add(labelControl);
            grid.Children.Add(checkBox);
            parent.Children.Add(grid);
            
            dynamicControls.Add(grid);
        }
        
        private Dictionary<string, string> ParseLuaTable(string content, string tableName)
        {
            var result = new Dictionary<string, string>();
            
            // Find the table definition
            var tablePattern = $@"{tableName}\s*=\s*\{{([^}}]+)\}}";
            var match = System.Text.RegularExpressions.Regex.Match(content, tablePattern, System.Text.RegularExpressions.RegexOptions.Singleline);
            
            if (match.Success)
            {
                var tableContent = match.Groups[1].Value;
                
                // Parse key-value pairs
                var kvpPattern = @"(\w+)\s*=\s*([^,}]+)";
                var kvpMatches = System.Text.RegularExpressions.Regex.Matches(tableContent, kvpPattern);
                
                foreach (System.Text.RegularExpressions.Match kvpMatch in kvpMatches)
                {
                    var key = kvpMatch.Groups[1].Value;
                    var value = kvpMatch.Groups[2].Value.Trim();
                    result[key] = value;
                }
            }
            
            return result;
        }
        
        private void ParseJsonSettings(string content)
        {
            // JSON parsing implementation
            ShowBasicTextEditor();
        }
        
        private void ParseXmlSettings(string content)
        {
            // XML parsing implementation
            ShowBasicTextEditor();
        }
        
        private void ParseCsvSettings(string content)
        {
            // CSV parsing implementation
            ShowBasicTextEditor();
        }
        
        private void ShowBasicTextEditor()
        {
            var basicGroup = new GroupBox
            {
                Header = "Text Editor",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var textBlock = new TextBlock
            {
                Text = "This file type doesn't have specific parsing. Use Advanced Mode for full editing capabilities.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5)
            };
            
            basicGroup.Content = textBlock;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(basicGroup);
            }
            dynamicControls.Add(basicGroup);
        }
        
        private void ShowBasicLuaEditor(string content)
        {
            var luaGroup = new GroupBox
            {
                Header = "Lua File Editor",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var textBlock = new TextBlock
            {
                Text = "This Lua file doesn't match known patterns. Use Advanced Mode for full editing capabilities.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5)
            };
            
            luaGroup.Content = textBlock;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(luaGroup);
            }
            dynamicControls.Add(luaGroup);
        }
        
        private void CreateGamemodeTemplate()
        {
            var template = @"--These all get overritten by presets if those are loaded with the map
--If you want to change values, change them there and/or make your own preset
sv_Settings = {
	--Raid Management
	playerAttackInterval = 600,
	firstWaveDurationBonus = 300,
	maximumEnemyLevel = 10,
	initialEnemyLevel = 0,
	levelClockInterval = 480,
	neutralAttackInterval = 60,
	startingCorruptCrystals = 2,
	forcePlayerFirefliesToPlayerCrystal = true,
  corruptronCap = 50,
  baseCorruptronOffense = 6,
  baseCorruptronDefense = 6,
  offenseIncreasePerLevel = 3,
  defenseIncreasePerLevel = 3,
  randomCorruptronCapture = false,
	--Resources
	bricktronCap = 100,
	startingWorkersCount = 10,
	startingKnightCount = 2,
	startingArcherCount = 2,
	--Global Settings
  fireflyCostMultiplier = 0.2,
	canDigGround = true,
	playerRelations = 2, --0 == allied, 1 == neutral, 2 == enemy
	--Time of Day
	startingTimeOfDay = 7,
	daynightCycleSetting = 0, --0 == daytime/nightime, 1 == only daytime, 2 == only nighttime
	daytimeFactor = 1.4,
	nighttimeFactor = 0.6,
  pauseTimeOfDay = false,
  moonlight = nil,
  ambientColor = nil
}

Characters = {
	Bricktron = {
		Ref = fy_Bricktron,
		Cost = 1
	},
	Corruptron = {
		Ref = fy_Corruptron,
    Occupation = Occupations.Corruptron,
		Cost = 3
	},
	Biftron = {
		Ref = fy_Biftron,
    Occupation = Occupations.Biftron,
		Cost = 12
	},
	Minitron = {
		Ref = fy_Minitron,
    Occupation = Occupations.Minitron,
		Cost = 1.5
	},
	Magitron = {
		Ref = fy_Magitron,
    Occupation = Occupations.Magitron,
		Cost = 18
	}
}

Registry = {
  currentLevel = sv_Settings.initialEnemyLevel,
  currentFibonacci = 1,
  previousFibonacci = 0,
  currentExperiencePoints = 0,
  timers = {
    timeForPlayerAttack = nil,
    timeBetweenLevels = nil,
    timeForNeutralAttack = nil,
    timeBeforeCanCapture = nil
  }
}

InterestPoints = {}";
            
            CodeEditorTextBox.Text = template;
            UpdateStatus("Created gamemode template", true);
        }
        
        private void CreatePresetsTemplate()
        {
            var template = @"--Presets for Castle Story
Presets = {
    Easy = {
        bricktronCap = 150,
        startingWorkersCount = 15,
        startingKnightCount = 3,
        startingArcherCount = 3,
        canDigGround = true,
        daytimeFactor = 1.6
    },
    Normal = {
        bricktronCap = 100,
        startingWorkersCount = 10,
        startingKnightCount = 2,
        startingArcherCount = 2,
        canDigGround = true,
        daytimeFactor = 1.4
    },
    Hard = {
        bricktronCap = 75,
        startingWorkersCount = 8,
        startingKnightCount = 1,
        startingArcherCount = 1,
        canDigGround = false,
        daytimeFactor = 1.2
    }
}";
            
            CodeEditorTextBox.Text = template;
            UpdateStatus("Created presets template", true);
        }
        
        private void CreateBricktronNamesTemplate()
        {
            var template = @"--Bricktron Names for Castle Story
BricktronNames = {
    Workers = {
        ""Builder Bob"",
        ""Constructor Carl"",
        ""Mason Mike"",
        ""Craftsman Chris"",
        ""Architect Alex""
    },
    Knights = {
        ""Sir Lancelot"",
        ""Knight Kevin"",
        ""Warrior Will"",
        ""Guardian Greg"",
        ""Defender Dave""
    },
    Archers = {
        ""Archer Andy"",
        ""Bowman Ben"",
        ""Sniper Sam"",
        ""Ranger Rick"",
        ""Hunter Harry""
    }
}";
            
            CodeEditorTextBox.Text = template;
            UpdateStatus("Created bricktron names template", true);
        }
        
        private void CreateLanguageTemplate()
        {
            var template = @"--Language and Translation Settings
Language = {
    English = {
        welcome = ""Welcome to Castle Story!"",
        build_castle = ""Build your castle"",
        defend_kingdom = ""Defend your kingdom"",
        collect_resources = ""Collect resources"",
        train_units = ""Train your units""
    },
    Spanish = {
        welcome = ""¡Bienvenido a Castle Story!"",
        build_castle = ""Construye tu castillo"",
        defend_kingdom = ""Defiende tu reino"",
        collect_resources = ""Recolecta recursos"",
        train_units = ""Entrena tus unidades""
    },
    French = {
        welcome = ""Bienvenue à Castle Story!"",
        build_castle = ""Construisez votre château"",
        defend_kingdom = ""Défendez votre royaume"",
        collect_resources = ""Collectez des ressources"",
        train_units = ""Entraînez vos unités""
    }
}";
            
            CodeEditorTextBox.Text = template;
            UpdateStatus("Created language template", true);
        }
        
        private void ParseBricktronNames(string content)
        {
            // Implementation for parsing bricktron names
            ShowBasicLuaEditor(content);
        }
        
        private void ParseLanguageFile(string content)
        {
            // Implementation for parsing language files
            ShowBasicLuaEditor(content);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to reset all changes? This cannot be undone.",
                    "Reset Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CodeEditorTextBox.Text = originalContent;
                    hasUnsavedChanges = false;
                    UpdateStatus("Changes reset", true);
                }
            }
        }

        private void ApplyEasyMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Generate Lua configuration based on Easy Mode settings
                var luaConfig = GenerateLuaConfig();
                
                // Save to a temporary file first
                var tempPath = Path.Combine(Path.GetTempPath(), "castle_story_easy_config.lua");
                File.WriteAllText(tempPath, luaConfig);
                
                // Ask user where to save
                var dialog = new SaveFileDialog
                {
                    Title = "Save Easy Mode Configuration",
                    Filter = "Lua files (*.lua)|*.lua|All files (*.*)|*.*",
                    InitialDirectory = gameDirectory,
                    FileName = "easy_mode_config.lua"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.Copy(tempPath, dialog.FileName, true);
                    File.Delete(tempPath);
                    
                    UpdateStatus("Easy Mode configuration saved successfully!", true);
                    FileStatusLabel.Text = "Configuration saved";
                }
                else
                {
                    File.Delete(tempPath);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error applying Easy Mode settings: {ex.Message}", false);
            }
        }

        private string GenerateLuaConfig()
        {
            var config = new System.Text.StringBuilder();
            config.AppendLine("-- Castle Story Easy Mode Configuration");
            config.AppendLine("-- Generated: " + DateTime.Now.ToString());
            config.AppendLine();
            
            // Player Settings
            config.AppendLine("-- Player Settings");
            if (!string.IsNullOrEmpty(MaxPlayersTextBox.Text) && int.TryParse(MaxPlayersTextBox.Text, out int maxPlayers))
            {
                config.AppendLine($"max_players = {maxPlayers}");
            }
            if (!string.IsNullOrEmpty(MaxTeamsTextBox.Text) && int.TryParse(MaxTeamsTextBox.Text, out int maxTeams))
            {
                config.AppendLine($"max_teams = {maxTeams}");
            }
            if (!string.IsNullOrEmpty(PlayerNameTextBox.Text))
            {
                config.AppendLine($"player_name = \"{PlayerNameTextBox.Text}\"");
            }
            config.AppendLine();
            
            // Game Settings
            config.AppendLine("-- Game Settings");
            var gameSpeed = GameSpeedComboBox.SelectedItem?.ToString() ?? "Normal";
            config.AppendLine($"game_speed = \"{gameSpeed}\"");
            
            var difficulty = DifficultyComboBox.SelectedItem?.ToString() ?? "Normal";
            config.AppendLine($"difficulty = \"{difficulty}\"");
            
            config.AppendLine($"god_mode = {GodModeCheckBox.IsChecked.ToString().ToLower()}");
            config.AppendLine($"infinite_resources = {InfiniteResourcesCheckBox.IsChecked.ToString().ToLower()}");
            config.AppendLine();
            
            // Language Settings
            config.AppendLine("-- Language Settings");
            var language = LanguageComboBox.SelectedItem?.ToString() ?? "English";
            config.AppendLine($"language = \"{language}\"");
            
            return config.ToString();
        }

        private void CodeEditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            hasUnsavedChanges = (CodeEditorTextBox.Text != originalContent);
            UpdateStatus();
            UpdateLineNumbers();
        }

        private void CodeEditorTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateLineNumbers();
        }

        private void UpdateLineNumbers()
        {
            if (!isEasyMode && CodeEditorTextBox != null)
            {
                var text = CodeEditorTextBox.Text;
                var lines = text.Split('\n');
                var lineNumbers = string.Join("\n", Enumerable.Range(1, lines.Length));
                
                LineNumbersTextBox.Text = lineNumbers;
                
                // Update cursor position
                var caretIndex = CodeEditorTextBox.CaretIndex;
                var textBeforeCaret = text.Substring(0, caretIndex);
                var lineNumber = textBeforeCaret.Count(c => c == '\n') + 1;
                var columnNumber = caretIndex - textBeforeCaret.LastIndexOf('\n');
                
                LineNumberLabel.Text = $"Line: {lineNumber}, Column: {columnNumber}";
            }
        }

        private void UpdateStatus(string message = null, bool isSuccess = true)
        {
            if (message != null)
            {
                StatusText.Text = message;
            }
            else
            {
                var status = hasUnsavedChanges ? "Unsaved changes" : "Ready";
                StatusText.Text = status;
            }
            
            StatusIndicator.Fill = isSuccess ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save them before closing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }
    }
}
