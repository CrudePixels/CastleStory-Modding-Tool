using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
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
            try
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
                    // Show main menu if no file is loaded
                    ShowMainMenu();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadEasyModeSettings: {ex.Message}");
                // Show main menu as fallback
                ShowMainMenu();
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
            gamemodeButton.Click += (s, e) => ShowGamemodeEasyMode();
            
            var presetsButton = new Button
            {
                Content = "⚙️ Presets (presets.lua)",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113))
            };
            presetsButton.Click += (s, e) => ShowPresetsEasyMode();
            
            var bricktronButton = new Button
            {
                Content = "🤖 Bricktron Names (names.lua)",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(155, 89, 182))
            };
            bricktronButton.Click += (s, e) => ShowBricktronNamesEasyMode();
            
            var languageButton = new Button
            {
                Content = "🌐 Language/Translations (lang.lua)",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(241, 196, 15))
            };
            languageButton.Click += (s, e) => ShowLanguageEasyMode();
            
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
        
        private void ShowMainMenu()
        {
            ClearDynamicEasyModeControls();
            
            var mainMenuGroup = new GroupBox
            {
                Header = "Easy Mode - Quick Game Modifications",
                Margin = new Thickness(10),
                Padding = new Thickness(10, 10, 10, 10)
            };
            
            var stackPanel = new StackPanel();
            
            var createModButton = new Button
            {
                Content = "📝 Create New Mod File",
                Margin = new Thickness(5, 5, 5, 5),
                Padding = new Thickness(15, 10, 15, 10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            createModButton.Click += (s, e) => ShowFileTypeSelection();
            
            var generalSettingsButton = new Button
            {
                Content = "⚙️ General Settings",
                Margin = new Thickness(5, 5, 5, 5),
                Padding = new Thickness(15, 10, 15, 10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125)),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            generalSettingsButton.Click += (s, e) => ShowGeneralSettings();
            
            stackPanel.Children.Add(createModButton);
            stackPanel.Children.Add(generalSettingsButton);
            
            mainMenuGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(mainMenuGroup);
            }
            dynamicControls.Add(mainMenuGroup);
        }
        
        private void ShowGeneralSettings()
        {
            ClearDynamicEasyModeControls();
            
            // Add back button
            var backButtonGroup = new GroupBox
            {
                Header = "Navigation",
                Margin = new Thickness(10, 10, 10, 5),
                Padding = new Thickness(10, 10, 10, 10)
            };
            
            var backButton = new Button
            {
                Content = "← Back to Main Menu",
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125))
            };
            backButton.Click += (s, e) => ShowMainMenu();
            
            var backPanel = new StackPanel { Orientation = Orientation.Horizontal };
            backPanel.Children.Add(backButton);
            backButtonGroup.Content = backPanel;
            
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(backButtonGroup);
            }
            dynamicControls.Add(backButtonGroup);
            
            // Player Settings Section
            var playerGroup = new GroupBox
            {
                Header = "Player Settings",
                Margin = new Thickness(10, 5, 10, 5),
                Padding = new Thickness(10, 10, 10, 10)
            };
            
            var playerPanel = new StackPanel();
            AddNumericField(playerPanel, "Max Players", "maxPlayers", new Dictionary<string, string>(), "4");
            AddNumericField(playerPanel, "Max Teams", "maxTeams", new Dictionary<string, string>(), "2");
            AddTextField(playerPanel, "Player Name", "playerName", new Dictionary<string, string>(), "Player");
            AddNumericField(playerPanel, "Starting Health", "startingHealth", new Dictionary<string, string>(), "100");
            AddNumericField(playerPanel, "Starting Resources", "startingResources", new Dictionary<string, string>(), "1000");
            
            playerGroup.Content = playerPanel;
            if (EasyModePanel.Content is StackPanel easyPanel2)
            {
                easyPanel2.Children.Add(playerGroup);
            }
            dynamicControls.Add(playerGroup);
            
            // Game Settings Section
            var gameGroup = new GroupBox
            {
                Header = "Game Settings",
                Margin = new Thickness(10, 5, 10, 5),
                Padding = new Thickness(10, 10, 10, 10)
            };
            
            var gamePanel = new StackPanel();
            AddDropdownField(gamePanel, "Game Speed", "gameSpeed", new Dictionary<string, string>(), "Normal", new[] { "Slow", "Normal", "Fast", "Very Fast" });
            AddDropdownField(gamePanel, "Difficulty", "difficulty", new Dictionary<string, string>(), "Normal", new[] { "Easy", "Normal", "Hard", "Expert" });
            AddBooleanField(gamePanel, "God Mode", "godMode", new Dictionary<string, string>(), "false");
            AddBooleanField(gamePanel, "Infinite Resources", "infiniteResources", new Dictionary<string, string>(), "false");
            AddBooleanField(gamePanel, "Unlimited Building", "unlimitedBuilding", new Dictionary<string, string>(), "false");
            AddBooleanField(gamePanel, "No Fog of War", "noFogOfWar", new Dictionary<string, string>(), "false");
            AddNumericField(gamePanel, "Map Size Multiplier", "mapSizeMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(gamePanel, "Resource Generation Rate", "resourceGenerationRate", new Dictionary<string, string>(), "1.0");
            
            gameGroup.Content = gamePanel;
            if (EasyModePanel.Content is StackPanel easyPanel3)
            {
                easyPanel3.Children.Add(gameGroup);
            }
            dynamicControls.Add(gameGroup);
            
            // Language Settings Section
            var languageGroup = new GroupBox
            {
                Header = "Language Settings",
                Margin = new Thickness(10, 5, 10, 5),
                Padding = new Thickness(10, 10, 10, 10)
            };
            
            var languagePanel = new StackPanel();
            AddDropdownField(languagePanel, "Language", "language", new Dictionary<string, string>(), "English", new[] { "English", "Spanish", "French", "German", "Italian", "Portuguese", "Russian", "Chinese", "Japanese" });
            AddBooleanField(languagePanel, "Show Subtitles", "showSubtitles", new Dictionary<string, string>(), "true");
            AddBooleanField(languagePanel, "Show Tooltips", "showTooltips", new Dictionary<string, string>(), "true");
            AddNumericField(languagePanel, "Text Size", "textSize", new Dictionary<string, string>(), "12");
            
            languageGroup.Content = languagePanel;
            if (EasyModePanel.Content is StackPanel easyPanel4)
            {
                easyPanel4.Children.Add(languageGroup);
            }
            dynamicControls.Add(languageGroup);
        }
        
        private void ShowFileTypeButtons()
        {
            var buttonGroup = new GroupBox
            {
                Header = "File Type Selection",
                Margin = new Thickness(10, 10, 10, 5),
                Padding = new Thickness(10, 10, 10, 10)
            };
            
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            
            var backButton = new Button
            {
                Content = "← Back to Main Menu",
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125))
            };
            backButton.Click += (s, e) => ShowMainMenu();
            
            var gamemodeButton = new Button
            {
                Content = "🎮 Gamemode",
                Margin = new Thickness(5, 5, 5, 5),
                Padding = new Thickness(10, 5, 10, 5),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219))
            };
            gamemodeButton.Click += (s, e) => ShowGamemodeEasyMode();
            
            var presetsButton = new Button
            {
                Content = "⚙️ Presets",
                Margin = new Thickness(5, 5, 5, 5),
                Padding = new Thickness(10, 5, 10, 5),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113))
            };
            presetsButton.Click += (s, e) => ShowPresetsEasyMode();
            
            var bricktronButton = new Button
            {
                Content = "🤖 Bricktron Names",
                Margin = new Thickness(5, 5, 5, 5),
                Padding = new Thickness(10, 5, 10, 5),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(155, 89, 182))
            };
            bricktronButton.Click += (s, e) => ShowBricktronNamesEasyMode();
            
            var languageButton = new Button
            {
                Content = "🌐 Language",
                Margin = new Thickness(5, 5, 5, 5),
                Padding = new Thickness(10, 5, 10, 5),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(241, 196, 15))
            };
            languageButton.Click += (s, e) => ShowLanguageEasyMode();
            
            buttonPanel.Children.Add(backButton);
            buttonPanel.Children.Add(gamemodeButton);
            buttonPanel.Children.Add(presetsButton);
            buttonPanel.Children.Add(bricktronButton);
            buttonPanel.Children.Add(languageButton);
            
            buttonGroup.Content = buttonPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(buttonGroup);
            }
            dynamicControls.Add(buttonGroup);
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
            else if (content.Contains("Faction") && (content.Contains("Color") || content.Contains("FactionColor")))
            {
                ParseFactionColors(content);
            }
            else if (content.Contains("Ladder") && content.Contains("Config"))
            {
                ParseLadderConfig(content);
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
            
            // Detect gamemode type
            var gamemodeType = DetectGamemodeType(content);
            var typeInfo = new TextBlock
            {
                Text = $"Gamemode Type: {gamemodeType}",
                Foreground = new SolidColorBrush(Colors.LightBlue),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(typeInfo);
            
            // Parse sv_Settings table
            var settings = ParseLuaTable(content, "sv_Settings");
            
            // Add gamemode-specific sections
            AddGamemodeSpecificSections(stackPanel, settings, gamemodeType, content);
            
            // Add common sections
            AddCommonGamemodeSections(stackPanel, settings);
            
            // Add character configuration
            AddCharacterConfiguration(stackPanel, content);
            
            configGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(configGroup);
            }
            dynamicControls.Add(configGroup);
        }

        private string DetectGamemodeType(string content)
        {
            var lowerContent = content.ToLower();
            
            if (lowerContent.Contains("wave") && lowerContent.Contains("invasion"))
                return "Invasion";
            else if (lowerContent.Contains("conquest") || lowerContent.Contains("conquer"))
                return "Conquest";
            else if (lowerContent.Contains("pumpkin") || lowerContent.Contains("grabthegems"))
                return "Grab the Gems";
            else if (lowerContent.Contains("credit") || lowerContent.Contains("backer"))
                return "Credit Island";
            else if (lowerContent.Contains("rttt") || lowerContent.Contains("race"))
                return "Race to the Top";
            else if (lowerContent.Contains("tutorial"))
                return "Tutorial";
            else if (lowerContent.Contains("world_editor"))
                return "World Editor";
            else if (lowerContent.Contains("sandbox"))
                return "Sandbox";
            else if (lowerContent.Contains("nothing"))
                return "Nothing";
            else
                return "Unknown";
        }

        private void AddGamemodeSpecificSections(StackPanel stackPanel, Dictionary<string, string> settings, string gamemodeType, string content)
        {
            switch (gamemodeType)
            {
                case "Invasion":
                    AddInvasionSections(stackPanel, settings);
                    break;
                case "Conquest":
                    AddConquestSections(stackPanel, settings);
                    break;
                case "Grab the Gems":
                    AddGrabTheGemsSections(stackPanel, settings);
                    break;
                case "Credit Island":
                    AddCreditIslandSections(stackPanel, settings);
                    break;
                case "Race to the Top":
                    AddRaceToTheTopSections(stackPanel, settings);
                    break;
                case "Tutorial":
                    AddTutorialSections(stackPanel, settings);
                    break;
                case "World Editor":
                    AddWorldEditorSections(stackPanel, settings);
                    break;
                case "Sandbox":
                    AddSandboxSections(stackPanel, settings);
                    break;
                default:
                    AddGenericSections(stackPanel, settings);
                    break;
            }
        }

        private void AddInvasionSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Wave Management Section
            var waveGroup = new GroupBox { Header = "🌊 Wave Management", Margin = new Thickness(0, 5, 0, 5) };
            var wavePanel = new StackPanel();
            
            AddBooleanField(wavePanel, "Display Time Before Wave", "displayTimeBeforeWave", settings, "true");
            AddNumericField(wavePanel, "First Wave Duration Bonus", "firstWaveDurationBonus", settings, "240.0");
            AddNumericField(wavePanel, "Wave Duration", "waveDuration", settings, "480.0");
            AddBooleanField(wavePanel, "Can Manual Trigger Waves", "canManualTriggerWaves", settings, "true");
            
            waveGroup.Content = wavePanel;
            stackPanel.Children.Add(waveGroup);
            
            // Wave Difficulty Section
            var difficultyGroup = new GroupBox { Header = "⚔️ Wave Difficulty", Margin = new Thickness(0, 5, 0, 5) };
            var difficultyPanel = new StackPanel();
            
            AddNumericField(difficultyPanel, "First Wave Budget", "firstWaveBudget", settings, "2");
            AddNumericField(difficultyPanel, "Additional Wave Budget", "additionalWaveBudget", settings, "2");
            AddNumericField(difficultyPanel, "Wave Budget Multiplier", "waveBudgetMultiplier", settings, "1");
            AddNumericField(difficultyPanel, "Wave Budget Random", "waveBudgetRandom", settings, "0");
            AddNumericField(difficultyPanel, "Wave Level Up Count", "waveLevelUpCount", settings, "20");
            AddNumericField(difficultyPanel, "Maximum Corruptron Level", "maximumCorruptronLevel", settings, "3");
            AddNumericField(difficultyPanel, "Victory Wave Number", "victoryWaveNumber", settings, "61");
            
            difficultyGroup.Content = difficultyPanel;
            stackPanel.Children.Add(difficultyGroup);
        }

        private void AddConquestSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Raid Management Section
            var raidGroup = new GroupBox { Header = "⚔️ Raid Management", Margin = new Thickness(0, 5, 0, 5) };
            var raidPanel = new StackPanel();
            
            AddNumericField(raidPanel, "Player Attack Interval", "playerAttackInterval", settings, "600");
            AddNumericField(raidPanel, "First Wave Duration Bonus", "firstWaveDurationBonus", settings, "300");
            AddNumericField(raidPanel, "Maximum Enemy Level", "maximumEnemyLevel", settings, "10");
            AddNumericField(raidPanel, "Initial Enemy Level", "initialEnemyLevel", settings, "0");
            AddNumericField(raidPanel, "Level Clock Interval", "levelClockInterval", settings, "480");
            AddNumericField(raidPanel, "Neutral Attack Interval", "neutralAttackInterval", settings, "60");
            AddNumericField(raidPanel, "Starting Corrupt Crystals", "startingCorruptCrystals", settings, "2");
            AddBooleanField(raidPanel, "Force Player Fireflies to Player Crystal", "forcePlayerFirefliesToPlayerCrystal", settings, "true");
            
            raidGroup.Content = raidPanel;
            stackPanel.Children.Add(raidGroup);
            
            // Corruptron Settings Section
            var corruptronGroup = new GroupBox { Header = "👹 Corruptron Settings", Margin = new Thickness(0, 5, 0, 5) };
            var corruptronPanel = new StackPanel();
            
            AddNumericField(corruptronPanel, "Corruptron Cap", "corruptronCap", settings, "50");
            AddNumericField(corruptronPanel, "Base Corruptron Offense", "baseCorruptronOffense", settings, "6");
            AddNumericField(corruptronPanel, "Base Corruptron Defense", "baseCorruptronDefense", settings, "6");
            AddNumericField(corruptronPanel, "Offense Increase Per Level", "offenseIncreasePerLevel", settings, "3");
            AddNumericField(corruptronPanel, "Defense Increase Per Level", "defenseIncreasePerLevel", settings, "3");
            AddBooleanField(corruptronPanel, "Random Corruptron Capture", "randomCorruptronCapture", settings, "false");
            
            corruptronGroup.Content = corruptronPanel;
            stackPanel.Children.Add(corruptronGroup);
        }

        private void AddGrabTheGemsSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Pumpkin/Gem Settings Section
            var gemGroup = new GroupBox { Header = "🎃 Pumpkin/Gem Settings", Margin = new Thickness(0, 5, 0, 5) };
            var gemPanel = new StackPanel();
            
            AddNumericField(gemPanel, "Pumpkin Spawn Time", "pumpkinSpawnTime", settings, "480");
            AddNumericField(gemPanel, "First Pumpkin Spawn Time", "firstPumpkinSpawnTime", settings, "720");
            AddNumericField(gemPanel, "Pumpkins to Win", "pumpkinsToWin", settings, "10");
            AddNumericField(gemPanel, "Pumpkins Per Wave", "pumpkinsPerWave", settings, "5");
            
            gemGroup.Content = gemPanel;
            stackPanel.Children.Add(gemGroup);
        }

        private void AddCreditIslandSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Credit Island is typically a peaceful mode, so we'll add basic settings
            var creditGroup = new GroupBox { Header = "🏝️ Credit Island Settings", Margin = new Thickness(0, 5, 0, 5) };
            var creditPanel = new StackPanel();
            
            var infoText = new TextBlock
            {
                Text = "Credit Island is a peaceful exploration mode featuring backer content.",
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            creditPanel.Children.Add(infoText);
            
            creditGroup.Content = creditPanel;
            stackPanel.Children.Add(creditGroup);
        }

        private void AddRaceToTheTopSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Race to the Top specific settings
            var raceGroup = new GroupBox { Header = "🏁 Race to the Top Settings", Margin = new Thickness(0, 5, 0, 5) };
            var racePanel = new StackPanel();
            
            AddNumericField(racePanel, "Starting Builders", "startingBuilders", settings, "3");
            
            var infoText = new TextBlock
            {
                Text = "Race to the Top is a competitive building mode where players race to build the highest structure.",
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0)
            };
            racePanel.Children.Add(infoText);
            
            raceGroup.Content = racePanel;
            stackPanel.Children.Add(raceGroup);
        }

        private void AddTutorialSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Tutorial specific settings
            var tutorialGroup = new GroupBox { Header = "📚 Tutorial Settings", Margin = new Thickness(0, 5, 0, 5) };
            var tutorialPanel = new StackPanel();
            
            var infoText = new TextBlock
            {
                Text = "Tutorial mode provides guided gameplay with step-by-step instructions.",
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            tutorialPanel.Children.Add(infoText);
            
            tutorialGroup.Content = tutorialPanel;
            stackPanel.Children.Add(tutorialGroup);
        }

        private void AddWorldEditorSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // World Editor specific settings
            var editorGroup = new GroupBox { Header = "🌍 World Editor Settings", Margin = new Thickness(0, 5, 0, 5) };
            var editorPanel = new StackPanel();
            
            var infoText = new TextBlock
            {
                Text = "World Editor mode allows you to create and edit custom maps.",
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            editorPanel.Children.Add(infoText);
            
            editorGroup.Content = editorPanel;
            stackPanel.Children.Add(editorGroup);
        }

        private void AddSandboxSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Sandbox specific settings
            var sandboxGroup = new GroupBox { Header = "🏗️ Sandbox Settings", Margin = new Thickness(0, 5, 0, 5) };
            var sandboxPanel = new StackPanel();
            
            var infoText = new TextBlock
            {
                Text = "Sandbox mode provides unlimited resources and creative freedom.",
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            sandboxPanel.Children.Add(infoText);
            
            sandboxGroup.Content = sandboxPanel;
            stackPanel.Children.Add(sandboxGroup);
        }

        private void AddGenericSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Generic settings for unknown gamemodes
            var genericGroup = new GroupBox { Header = "⚙️ Generic Settings", Margin = new Thickness(0, 5, 0, 5) };
            var genericPanel = new StackPanel();
            
            var infoText = new TextBlock
            {
                Text = "This gamemode type is not specifically recognized. Showing available settings.",
                Foreground = new SolidColorBrush(Colors.Orange),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            genericPanel.Children.Add(infoText);
            
            genericGroup.Content = genericPanel;
            stackPanel.Children.Add(genericGroup);
        }

        private void AddCommonGamemodeSections(StackPanel stackPanel, Dictionary<string, string> settings)
        {
            // Resources Section
            var resourcesGroup = new GroupBox { Header = "💎 Resources", Margin = new Thickness(0, 5, 0, 5) };
            var resourcesPanel = new StackPanel();
            
            AddNumericField(resourcesPanel, "Bricktron Cap", "bricktronCap", settings, "15");
            AddNumericField(resourcesPanel, "Starting Workers Count", "startingWorkersCount", settings, "3");
            AddNumericField(resourcesPanel, "Starting Knight Count", "startingKnightCount", settings, "1");
            AddNumericField(resourcesPanel, "Starting Archer Count", "startingArcherCount", settings, "1");
            
            resourcesGroup.Content = resourcesPanel;
            stackPanel.Children.Add(resourcesGroup);
            
            // Global Settings Section
            var globalGroup = new GroupBox { Header = "🌐 Global Settings", Margin = new Thickness(0, 5, 0, 5) };
            var globalPanel = new StackPanel();
            
            AddNumericField(globalPanel, "Firefly Cost Multiplier", "fireflyCostMultiplier", settings, "0.7");
            AddBooleanField(globalPanel, "Can Dig Ground", "canDigGround", settings, "false");
            AddNumericField(globalPanel, "Player Relations", "playerRelations", settings, "2");
            
            globalGroup.Content = globalPanel;
            stackPanel.Children.Add(globalGroup);
            
            // Time of Day Section
            var timeGroup = new GroupBox { Header = "🌅 Time of Day", Margin = new Thickness(0, 5, 0, 5) };
            var timePanel = new StackPanel();
            
            AddNumericField(timePanel, "Starting Time of Day", "startingTimeOfDay", settings, "7");
            AddNumericField(timePanel, "Day/Night Cycle Setting", "daynightCycleSetting", settings, "0");
            AddNumericField(timePanel, "Daytime Factor", "daytimeFactor", settings, "1.4");
            AddNumericField(timePanel, "Nighttime Factor", "nighttimeFactor", settings, "0.6");
            AddBooleanField(timePanel, "Pause Time of Day", "pauseTimeOfDay", settings, "false");
            
            timeGroup.Content = timePanel;
            stackPanel.Children.Add(timeGroup);
        }

        private void AddCharacterConfiguration(StackPanel stackPanel, string content)
        {
            // Character Configuration Section
            var characterGroup = new GroupBox { Header = "👥 Character Configuration", Margin = new Thickness(0, 5, 0, 5) };
            var characterPanel = new StackPanel();
            
            var infoText = new TextBlock
            {
                Text = "Configure character costs and properties for this gamemode.",
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            characterPanel.Children.Add(infoText);
            
            // Parse Characters table
            var characters = ParseLuaTable(content, "Characters");
            
            // Add character cost fields
            var characterCosts = new StackPanel();
            
            AddNumericField(characterCosts, "Bricktron Cost", "Bricktron.Cost", characters, "1");
            AddNumericField(characterCosts, "Corruptron Cost", "Corruptron.Cost", characters, "3");
            AddNumericField(characterCosts, "Biftron Cost", "Biftron.Cost", characters, "12");
            AddNumericField(characterCosts, "Minitron Cost", "Minitron.Cost", characters, "1.5");
            AddNumericField(characterCosts, "Magitron Cost", "Magitron.Cost", characters, "18");
            
            characterPanel.Children.Add(characterCosts);
            
            // Add character management buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            var addCharacterButton = new Button
            {
                Content = "➕ Add Character",
                Background = new SolidColorBrush(Colors.DarkGreen),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 10, 0)
            };
            addCharacterButton.Click += AddCharacter_Click;
            buttonPanel.Children.Add(addCharacterButton);
            
            var editCharacterButton = new Button
            {
                Content = "✏️ Edit Character",
                Background = new SolidColorBrush(Colors.DarkBlue),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 10, 0)
            };
            editCharacterButton.Click += EditCharacter_Click;
            buttonPanel.Children.Add(editCharacterButton);
            
            var removeCharacterButton = new Button
            {
                Content = "🗑️ Remove Character",
                Background = new SolidColorBrush(Colors.DarkRed),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(10, 5, 10, 5)
            };
            removeCharacterButton.Click += RemoveCharacter_Click;
            buttonPanel.Children.Add(removeCharacterButton);
            
            characterPanel.Children.Add(buttonPanel);
            characterGroup.Content = characterPanel;
            stackPanel.Children.Add(characterGroup);
        }

        private void AddCharacter_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Adding new character...", true);
            // This would open a character editor dialog
        }

        private void EditCharacter_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Editing character...", true);
            // This would open a character editor dialog
        }

        private void RemoveCharacter_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Removing character...", true);
            // This would remove a character from the configuration
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
        
        private void ParseFactionColors(string content)
        {
            var configGroup = new GroupBox
            {
                Header = "🎨 Faction Color Configuration",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            // Add info text
            var infoText = new TextBlock
            {
                Text = "Configure faction colors for your Castle Story game. You can add custom colors, modify existing ones, and create color palettes.",
                Foreground = new SolidColorBrush(Colors.LightBlue),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(infoText);
            
            // Add faction color editor button
            var colorEditorButton = new Button
            {
                Content = "🎨 Open Faction Color Editor",
                Background = new SolidColorBrush(Colors.DarkGreen),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(20, 10, 20, 10),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 0, 10)
            };
            colorEditorButton.Click += OpenFactionColorEditor_Click;
            stackPanel.Children.Add(colorEditorButton);
            
            // Add current colors display
            var currentColorsGroup = new GroupBox
            {
                Header = "Current Faction Colors",
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            var colorsPanel = new WrapPanel();
            var defaultColors = new[] { "Blue", "Green", "Orange", "Purple", "Red", "Yellow" };
            
            foreach (var colorName in defaultColors)
            {
                var colorButton = new Button
                {
                    Content = colorName,
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(5),
                    Cursor = Cursors.Hand
                };
                
                // Set button color based on name
                switch (colorName.ToLower())
                {
                    case "blue": colorButton.Background = new SolidColorBrush(Colors.Blue); break;
                    case "green": colorButton.Background = new SolidColorBrush(Colors.Green); break;
                    case "orange": colorButton.Background = new SolidColorBrush(Colors.Orange); break;
                    case "purple": colorButton.Background = new SolidColorBrush(Colors.Purple); break;
                    case "red": colorButton.Background = new SolidColorBrush(Colors.Red); break;
                    case "yellow": colorButton.Background = new SolidColorBrush(Colors.Yellow); break;
                }
                
                colorButton.Foreground = new SolidColorBrush(Colors.White);
                colorButton.FontWeight = FontWeights.Bold;
                colorsPanel.Children.Add(colorButton);
            }
            
            currentColorsGroup.Content = colorsPanel;
            stackPanel.Children.Add(currentColorsGroup);
            
            configGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(configGroup);
            }
        }

        private void OpenFactionColorEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var colorEditor = new FactionColorEditor();
                colorEditor.ColorsSaved += (s, args) => {
                    // Generate and save the faction colors Lua code
                    var luaCode = colorEditor.GenerateLuaCode();
                    CodeEditorTextBox.Text = luaCode;
                    originalContent = luaCode;
                    hasUnsavedChanges = true;
                    UpdateStatus("Faction colors updated", true);
                };
                
                var window = new Window
                {
                    Title = "Faction Color Editor",
                    Content = colorEditor,
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
                };
                
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error opening color editor: {ex.Message}", false);
            }
        }

        private void ParseLadderConfig(string content)
        {
            var configGroup = new GroupBox
            {
                Header = "🪜 Ladder System Configuration",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            // Add info text
            var infoText = new TextBlock
            {
                Text = "Configure the enhanced ladder system for Castle Story. Enable climbing mechanics, set ladder types, and customize physics.",
                Foreground = new SolidColorBrush(Colors.LightGreen),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(infoText);
            
            // Parse ladder configuration
            var ladderConfig = ParseLuaTable(content, "LadderConfig");
            
            // Basic Settings Section
            var basicGroup = new GroupBox { Header = "Basic Settings", Margin = new Thickness(0, 5, 0, 5) };
            var basicPanel = new StackPanel();
            
            AddBooleanField(basicPanel, "Enable Ladder System", "enabled", ladderConfig, "true");
            AddNumericField(basicPanel, "Maximum Height", "maxHeight", ladderConfig, "50");
            AddNumericField(basicPanel, "Climb Speed", "climbSpeed", ladderConfig, "2.0");
            AddBooleanField(basicPanel, "Auto Snap to Ladder", "autoSnap", ladderConfig, "true");
            
            basicGroup.Content = basicPanel;
            stackPanel.Children.Add(basicGroup);
            
            // Physics Settings Section
            var physicsGroup = new GroupBox { Header = "Physics Settings", Margin = new Thickness(0, 5, 0, 5) };
            var physicsPanel = new StackPanel();
            
            var physicsConfig = ParseLuaTable(content, "LadderConfig.physics");
            AddNumericField(physicsPanel, "Grab Distance", "grabDistance", physicsConfig, "2.0");
            AddNumericField(physicsPanel, "Release Distance", "releaseDistance", physicsConfig, "3.0");
            AddNumericField(physicsPanel, "Snap Distance", "snapDistance", physicsConfig, "1.0");
            AddNumericField(physicsPanel, "Climb Height", "climbHeight", physicsConfig, "1.0");
            AddBooleanField(physicsPanel, "Fall Damage", "fallDamage", physicsConfig, "false");
            
            physicsGroup.Content = physicsPanel;
            stackPanel.Children.Add(physicsGroup);
            
            // Requirements Section
            var requirementsGroup = new GroupBox { Header = "Building Requirements", Margin = new Thickness(0, 5, 0, 5) };
            var requirementsPanel = new StackPanel();
            
            var reqConfig = ParseLuaTable(content, "LadderConfig.requirements");
            AddNumericField(requirementsPanel, "Minimum Level", "minLevel", reqConfig, "1");
            AddBooleanField(requirementsPanel, "Requires Blueprint", "requiresBlueprint", reqConfig, "false");
            AddNumericField(requirementsPanel, "Max Per Player", "maxPerPlayer", reqConfig, "10");
            AddNumericField(requirementsPanel, "Cooldown (seconds)", "cooldown", reqConfig, "5.0");
            
            requirementsGroup.Content = requirementsPanel;
            stackPanel.Children.Add(requirementsGroup);
            
            // Ladder Types Section
            var typesGroup = new GroupBox { Header = "Ladder Types", Margin = new Thickness(0, 5, 0, 5) };
            var typesPanel = new StackPanel();
            
            var typesInfo = new TextBlock
            {
                Text = "Configure different ladder types with varying materials, durability, and costs.",
                Foreground = new SolidColorBrush(Colors.LightGray),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            typesPanel.Children.Add(typesInfo);
            
            // Add ladder type buttons
            var ladderTypes = new[] { "Wooden Ladder", "Iron Ladder", "Stone Ladder", "Rope Ladder" };
            var typesButtonPanel = new WrapPanel();
            
            foreach (var ladderType in ladderTypes)
            {
                var typeButton = new Button
                {
                    Content = ladderType,
                    Width = 120,
                    Height = 35,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush(Colors.DarkSlateGray),
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand
                };
                typeButton.Click += (s, e) => EditLadderType_Click(ladderType);
                typesButtonPanel.Children.Add(typeButton);
            }
            
            typesPanel.Children.Add(typesButtonPanel);
            typesGroup.Content = typesPanel;
            stackPanel.Children.Add(typesGroup);
            
            // Action Buttons
            var actionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            
            var enableButton = new Button
            {
                Content = "✅ Enable Ladder System",
                Background = new SolidColorBrush(Colors.DarkGreen),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.Bold
            };
            enableButton.Click += EnableLadderSystem_Click;
            actionPanel.Children.Add(enableButton);
            
            var testButton = new Button
            {
                Content = "🧪 Test Ladder",
                Background = new SolidColorBrush(Colors.DarkBlue),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.Bold
            };
            testButton.Click += TestLadderSystem_Click;
            actionPanel.Children.Add(testButton);
            
            var resetButton = new Button
            {
                Content = "🔄 Reset to Defaults",
                Background = new SolidColorBrush(Colors.DarkOrange),
                Foreground = new SolidColorBrush(Colors.White),
                Padding = new Thickness(15, 8, 15, 8),
                FontWeight = FontWeights.Bold
            };
            resetButton.Click += ResetLadderConfig_Click;
            actionPanel.Children.Add(resetButton);
            
            stackPanel.Children.Add(actionPanel);
            
            configGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(configGroup);
            }
        }

        private void EditLadderType_Click(string ladderType)
        {
            UpdateStatus($"Editing ladder type: {ladderType}", true);
            // This would open a detailed editor for the specific ladder type
        }

        private void EnableLadderSystem_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Enabling ladder system...", true);
            // This would enable the ladder system in the game
        }

        private void TestLadderSystem_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Testing ladder system...", true);
            // This would test the ladder system functionality
        }

        private void ResetLadderConfig_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Resetting ladder configuration to defaults...", true);
            // This would reset the ladder configuration
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
        
        // New Easy Mode Methods for Dynamic Form Generation
        private void ShowGamemodeEasyMode()
        {
            ClearDynamicEasyModeControls();
            
            // Add file type selection buttons at the top
            ShowFileTypeButtons();
            
            var gamemodeGroup = new GroupBox
            {
                Header = "🎮 Gamemode Configuration - Quick Game Modifications",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            // Raid Management Section
            var raidGroup = new GroupBox { Header = "Raid Management", Margin = new Thickness(0, 5, 0, 5) };
            var raidPanel = new StackPanel();
            
            AddNumericField(raidPanel, "Player Attack Interval (seconds)", "playerAttackInterval", new Dictionary<string, string>(), "600");
            AddNumericField(raidPanel, "First Wave Duration Bonus (seconds)", "firstWaveDurationBonus", new Dictionary<string, string>(), "300");
            AddNumericField(raidPanel, "Maximum Enemy Level", "maximumEnemyLevel", new Dictionary<string, string>(), "10");
            AddNumericField(raidPanel, "Initial Enemy Level", "initialEnemyLevel", new Dictionary<string, string>(), "0");
            AddNumericField(raidPanel, "Level Clock Interval (seconds)", "levelClockInterval", new Dictionary<string, string>(), "480");
            AddNumericField(raidPanel, "Neutral Attack Interval (seconds)", "neutralAttackInterval", new Dictionary<string, string>(), "60");
            AddNumericField(raidPanel, "Starting Corrupt Crystals", "startingCorruptCrystals", new Dictionary<string, string>(), "2");
            AddBooleanField(raidPanel, "Force Player Fireflies to Player Crystal", "forcePlayerFirefliesToPlayerCrystal", new Dictionary<string, string>(), "true");
            AddNumericField(raidPanel, "Corruptron Cap", "corruptronCap", new Dictionary<string, string>(), "50");
            AddNumericField(raidPanel, "Base Corruptron Offense", "baseCorruptronOffense", new Dictionary<string, string>(), "6");
            AddNumericField(raidPanel, "Base Corruptron Defense", "baseCorruptronDefense", new Dictionary<string, string>(), "6");
            AddNumericField(raidPanel, "Offense Increase Per Level", "offenseIncreasePerLevel", new Dictionary<string, string>(), "3");
            AddNumericField(raidPanel, "Defense Increase Per Level", "defenseIncreasePerLevel", new Dictionary<string, string>(), "3");
            AddBooleanField(raidPanel, "Random Corruptron Capture", "randomCorruptronCapture", new Dictionary<string, string>(), "false");
            
            raidGroup.Content = raidPanel;
            stackPanel.Children.Add(raidGroup);
            
            // Resources Section
            var resourcesGroup = new GroupBox { Header = "Resources", Margin = new Thickness(0, 5, 0, 5) };
            var resourcesPanel = new StackPanel();
            
            AddNumericField(resourcesPanel, "Bricktron Cap", "bricktronCap", new Dictionary<string, string>(), "100");
            AddNumericField(resourcesPanel, "Starting Workers Count", "startingWorkersCount", new Dictionary<string, string>(), "10");
            AddNumericField(resourcesPanel, "Starting Knight Count", "startingKnightCount", new Dictionary<string, string>(), "2");
            AddNumericField(resourcesPanel, "Starting Archer Count", "startingArcherCount", new Dictionary<string, string>(), "2");
            
            resourcesGroup.Content = resourcesPanel;
            stackPanel.Children.Add(resourcesGroup);
            
            // Global Settings Section
            var globalGroup = new GroupBox { Header = "Global Settings", Margin = new Thickness(0, 5, 0, 5) };
            var globalPanel = new StackPanel();
            
            AddNumericField(globalPanel, "Firefly Cost Multiplier", "fireflyCostMultiplier", new Dictionary<string, string>(), "0.2");
            AddBooleanField(globalPanel, "Can Dig Ground", "canDigGround", new Dictionary<string, string>(), "true");
            AddNumericField(globalPanel, "Player Relations (0=allied, 1=neutral, 2=enemy)", "playerRelations", new Dictionary<string, string>(), "2");
            
            globalGroup.Content = globalPanel;
            stackPanel.Children.Add(globalGroup);
            
            // Time of Day Section
            var timeGroup = new GroupBox { Header = "Time of Day", Margin = new Thickness(0, 5, 0, 5) };
            var timePanel = new StackPanel();
            
            AddNumericField(timePanel, "Starting Time of Day", "startingTimeOfDay", new Dictionary<string, string>(), "7");
            AddNumericField(timePanel, "Day/Night Cycle Setting (0=both, 1=day only, 2=night only)", "daynightCycleSetting", new Dictionary<string, string>(), "0");
            AddNumericField(timePanel, "Daytime Factor", "daytimeFactor", new Dictionary<string, string>(), "1.4");
            AddNumericField(timePanel, "Nighttime Factor", "nighttimeFactor", new Dictionary<string, string>(), "0.6");
            AddBooleanField(timePanel, "Pause Time of Day", "pauseTimeOfDay", new Dictionary<string, string>(), "false");
            AddNumericField(timePanel, "Day Length (minutes)", "dayLength", new Dictionary<string, string>(), "20");
            AddNumericField(timePanel, "Night Length (minutes)", "nightLength", new Dictionary<string, string>(), "10");
            AddBooleanField(timePanel, "Dynamic Weather", "dynamicWeather", new Dictionary<string, string>(), "true");
            
            timeGroup.Content = timePanel;
            stackPanel.Children.Add(timeGroup);
            
            // Advanced Combat Section
            var combatGroup = new GroupBox { Header = "Advanced Combat", Margin = new Thickness(0, 5, 0, 5) };
            var combatPanel = new StackPanel();
            
            AddNumericField(combatPanel, "Damage Multiplier", "damageMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Armor Effectiveness", "armorEffectiveness", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Critical Hit Chance", "criticalHitChance", new Dictionary<string, string>(), "0.1");
            AddNumericField(combatPanel, "Critical Hit Damage", "criticalHitDamage", new Dictionary<string, string>(), "2.0");
            AddBooleanField(combatPanel, "Friendly Fire", "friendlyFire", new Dictionary<string, string>(), "false");
            AddBooleanField(combatPanel, "Auto-Attack", "autoAttack", new Dictionary<string, string>(), "true");
            AddNumericField(combatPanel, "Attack Range Multiplier", "attackRangeMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Attack Speed Multiplier", "attackSpeedMultiplier", new Dictionary<string, string>(), "1.0");
            
            combatGroup.Content = combatPanel;
            stackPanel.Children.Add(combatGroup);
            
            // Economy Section
            var economyGroup = new GroupBox { Header = "Economy", Margin = new Thickness(0, 5, 0, 5) };
            var economyPanel = new StackPanel();
            
            AddNumericField(economyPanel, "Resource Drop Rate", "resourceDropRate", new Dictionary<string, string>(), "1.0");
            AddNumericField(economyPanel, "Building Cost Multiplier", "buildingCostMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(economyPanel, "Unit Cost Multiplier", "unitCostMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(economyPanel, "Research Cost Multiplier", "researchCostMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(economyPanel, "Trade Value Multiplier", "tradeValueMultiplier", new Dictionary<string, string>(), "1.0");
            AddBooleanField(economyPanel, "Infinite Resources", "infiniteResources", new Dictionary<string, string>(), "false");
            AddBooleanField(economyPanel, "Free Buildings", "freeBuildings", new Dictionary<string, string>(), "false");
            AddBooleanField(economyPanel, "Instant Research", "instantResearch", new Dictionary<string, string>(), "false");
            
            economyGroup.Content = economyPanel;
            stackPanel.Children.Add(economyGroup);
            
            gamemodeGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(gamemodeGroup);
            }
            dynamicControls.Add(gamemodeGroup);
        }
        
        private void ShowPresetsEasyMode()
        {
            ClearDynamicEasyModeControls();
            
            // Add file type selection buttons at the top
            ShowFileTypeButtons();
            
            var presetsGroup = new GroupBox
            {
                Header = "⚙️ Presets Configuration - Quick Game Modifications",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            // Difficulty Presets
            var difficultyGroup = new GroupBox { Header = "Difficulty Settings", Margin = new Thickness(0, 5, 0, 5) };
            var difficultyPanel = new StackPanel();
            
            AddNumericField(difficultyPanel, "Easy - Enemy Level Multiplier", "easyEnemyLevel", new Dictionary<string, string>(), "0.5");
            AddNumericField(difficultyPanel, "Normal - Enemy Level Multiplier", "normalEnemyLevel", new Dictionary<string, string>(), "1.0");
            AddNumericField(difficultyPanel, "Hard - Enemy Level Multiplier", "hardEnemyLevel", new Dictionary<string, string>(), "1.5");
            AddNumericField(difficultyPanel, "Expert - Enemy Level Multiplier", "expertEnemyLevel", new Dictionary<string, string>(), "2.0");
            
            difficultyGroup.Content = difficultyPanel;
            stackPanel.Children.Add(difficultyGroup);
            
            // Resource Presets
            var resourceGroup = new GroupBox { Header = "Resource Multipliers", Margin = new Thickness(0, 5, 0, 5) };
            var resourcePanel = new StackPanel();
            
            AddNumericField(resourcePanel, "Resource Generation Rate", "resourceRate", new Dictionary<string, string>(), "1.0");
            AddNumericField(resourcePanel, "Unit Cost Multiplier", "unitCost", new Dictionary<string, string>(), "1.0");
            AddNumericField(resourcePanel, "Building Cost Multiplier", "buildingCost", new Dictionary<string, string>(), "1.0");
            AddNumericField(resourcePanel, "Research Cost Multiplier", "researchCost", new Dictionary<string, string>(), "1.0");
            AddNumericField(resourcePanel, "Upgrade Cost Multiplier", "upgradeCost", new Dictionary<string, string>(), "1.0");
            AddNumericField(resourcePanel, "Repair Cost Multiplier", "repairCost", new Dictionary<string, string>(), "1.0");
            
            resourceGroup.Content = resourcePanel;
            stackPanel.Children.Add(resourceGroup);
            
            // Combat Presets
            var combatGroup = new GroupBox { Header = "Combat Multipliers", Margin = new Thickness(0, 5, 0, 5) };
            var combatPanel = new StackPanel();
            
            AddNumericField(combatPanel, "Damage Multiplier", "damageMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Health Multiplier", "healthMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Armor Multiplier", "armorMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Speed Multiplier", "speedMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Attack Speed Multiplier", "attackSpeedMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(combatPanel, "Range Multiplier", "rangeMultiplier", new Dictionary<string, string>(), "1.0");
            
            combatGroup.Content = combatPanel;
            stackPanel.Children.Add(combatGroup);
            
            // Gameplay Presets
            var gameplayGroup = new GroupBox { Header = "Gameplay Multipliers", Margin = new Thickness(0, 5, 0, 5) };
            var gameplayPanel = new StackPanel();
            
            AddNumericField(gameplayPanel, "Experience Multiplier", "experienceMultiplier", new Dictionary<string, string>(), "1.0");
            AddNumericField(gameplayPanel, "Level Up Speed", "levelUpSpeed", new Dictionary<string, string>(), "1.0");
            AddNumericField(gameplayPanel, "Building Speed Multiplier", "buildingSpeed", new Dictionary<string, string>(), "1.0");
            AddNumericField(gameplayPanel, "Research Speed Multiplier", "researchSpeed", new Dictionary<string, string>(), "1.0");
            AddNumericField(gameplayPanel, "Movement Speed Multiplier", "movementSpeed", new Dictionary<string, string>(), "1.0");
            AddNumericField(gameplayPanel, "Time Scale Multiplier", "timeScale", new Dictionary<string, string>(), "1.0");
            
            gameplayGroup.Content = gameplayPanel;
            stackPanel.Children.Add(gameplayGroup);
            
            presetsGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(presetsGroup);
            }
            dynamicControls.Add(presetsGroup);
        }
        
        private void ShowBricktronNamesEasyMode()
        {
            ClearDynamicEasyModeControls();
            
            // Add file type selection buttons at the top
            ShowFileTypeButtons();
            
            var bricktronGroup = new GroupBox
            {
                Header = "🤖 Bricktron Names Configuration - Quick Game Modifications",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            // Name Categories
            var namesGroup = new GroupBox { Header = "Name Categories", Margin = new Thickness(0, 5, 0, 5) };
            var namesPanel = new StackPanel();
            
            AddTextField(namesPanel, "Worker Names (comma separated)", "workerNames", new Dictionary<string, string>(), "Bob, Alice, Charlie, Diana, Edward");
            AddTextField(namesPanel, "Knight Names (comma separated)", "knightNames", new Dictionary<string, string>(), "Sir Galahad, Sir Lancelot, Sir Percival, Sir Tristan");
            AddTextField(namesPanel, "Archer Names (comma separated)", "archerNames", new Dictionary<string, string>(), "Robin, Legolas, Hawkeye, Artemis");
            AddTextField(namesPanel, "Magician Names (comma separated)", "magicianNames", new Dictionary<string, string>(), "Merlin, Gandalf, Saruman, Dumbledore");
            
            namesGroup.Content = namesPanel;
            stackPanel.Children.Add(namesGroup);
            
            // Character Settings
            var characterGroup = new GroupBox { Header = "Character Settings", Margin = new Thickness(0, 5, 0, 5) };
            var characterPanel = new StackPanel();
            
            AddNumericField(characterPanel, "Name Pool Size", "namePoolSize", new Dictionary<string, string>(), "50");
            AddBooleanField(characterPanel, "Use Random Names", "useRandomNames", new Dictionary<string, string>(), "true");
            AddBooleanField(characterPanel, "Allow Duplicate Names", "allowDuplicates", new Dictionary<string, string>(), "false");
            AddBooleanField(characterPanel, "Use Last Names", "useLastNames", new Dictionary<string, string>(), "true");
            AddBooleanField(characterPanel, "Use Titles", "useTitles", new Dictionary<string, string>(), "false");
            AddNumericField(characterPanel, "Name Length Min", "nameLengthMin", new Dictionary<string, string>(), "3");
            AddNumericField(characterPanel, "Name Length Max", "nameLengthMax", new Dictionary<string, string>(), "12");
            
            characterGroup.Content = characterPanel;
            stackPanel.Children.Add(characterGroup);
            
            // Special Names
            var specialGroup = new GroupBox { Header = "Special Names", Margin = new Thickness(0, 5, 0, 5) };
            var specialPanel = new StackPanel();
            
            AddTextField(specialPanel, "Legendary Names (comma separated)", "legendaryNames", new Dictionary<string, string>(), "Dragon Slayer, Legend, Hero, Champion");
            AddTextField(specialPanel, "Veteran Names (comma separated)", "veteranNames", new Dictionary<string, string>(), "Old Guard, Veteran, Experienced, Seasoned");
            AddTextField(specialPanel, "Rookie Names (comma separated)", "rookieNames", new Dictionary<string, string>(), "Newbie, Rookie, Fresh, Green");
            AddTextField(specialPanel, "Funny Names (comma separated)", "funnyNames", new Dictionary<string, string>(), "Silly, Goofy, Clumsy, Bumbling");
            
            specialGroup.Content = specialPanel;
            stackPanel.Children.Add(specialGroup);
            
            // Name Generation Rules
            var rulesGroup = new GroupBox { Header = "Name Generation Rules", Margin = new Thickness(0, 5, 0, 5) };
            var rulesPanel = new StackPanel();
            
            AddBooleanField(rulesPanel, "Gender-Specific Names", "genderSpecific", new Dictionary<string, string>(), "false");
            AddBooleanField(rulesPanel, "Cultural Names", "culturalNames", new Dictionary<string, string>(), "false");
            AddBooleanField(rulesPanel, "Fantasy Names", "fantasyNames", new Dictionary<string, string>(), "true");
            AddBooleanField(rulesPanel, "Modern Names", "modernNames", new Dictionary<string, string>(), "false");
            AddNumericField(rulesPanel, "Name Generation Attempts", "nameGenerationAttempts", new Dictionary<string, string>(), "3");
            AddBooleanField(rulesPanel, "Log Name Generation", "logNameGeneration", new Dictionary<string, string>(), "false");
            
            rulesGroup.Content = rulesPanel;
            stackPanel.Children.Add(rulesGroup);
            
            bricktronGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(bricktronGroup);
            }
            dynamicControls.Add(bricktronGroup);
        }
        
        private void ShowLanguageEasyMode()
        {
            ClearDynamicEasyModeControls();
            
            // Add file type selection buttons at the top
            ShowFileTypeButtons();
            
            var languageGroup = new GroupBox
            {
                Header = "🌐 Language/Translations Configuration - Quick Game Modifications",
                Margin = new Thickness(10),
                Padding = new Thickness(10)
            };
            
            var stackPanel = new StackPanel();
            
            // Language Settings
            var langGroup = new GroupBox { Header = "Language Settings", Margin = new Thickness(0, 5, 0, 5) };
            var langPanel = new StackPanel();
            
            AddDropdownField(langPanel, "Default Language", "defaultLanguage", new Dictionary<string, string>(), "English", new[] { "English", "Spanish", "French", "German", "Italian", "Portuguese", "Russian", "Chinese", "Japanese" });
            AddBooleanField(langPanel, "Enable Multi-Language", "enableMultiLanguage", new Dictionary<string, string>(), "true");
            AddBooleanField(langPanel, "Auto-Detect Language", "autoDetectLanguage", new Dictionary<string, string>(), "false");
            
            langGroup.Content = langPanel;
            stackPanel.Children.Add(langGroup);
            
            // Translation Settings
            var translationGroup = new GroupBox { Header = "Translation Settings", Margin = new Thickness(0, 5, 0, 5) };
            var translationPanel = new StackPanel();
            
            AddTextField(translationPanel, "Game Title Translation", "gameTitle", new Dictionary<string, string>(), "Castle Story");
            AddTextField(translationPanel, "Main Menu Text", "mainMenu", new Dictionary<string, string>(), "Main Menu");
            AddTextField(translationPanel, "Play Button Text", "playButton", new Dictionary<string, string>(), "Play");
            AddTextField(translationPanel, "Settings Button Text", "settingsButton", new Dictionary<string, string>(), "Settings");
            AddTextField(translationPanel, "Quit Button Text", "quitButton", new Dictionary<string, string>(), "Quit");
            AddTextField(translationPanel, "Load Game Text", "loadGame", new Dictionary<string, string>(), "Load Game");
            AddTextField(translationPanel, "Save Game Text", "saveGame", new Dictionary<string, string>(), "Save Game");
            AddTextField(translationPanel, "New Game Text", "newGame", new Dictionary<string, string>(), "New Game");
            AddTextField(translationPanel, "Multiplayer Text", "multiplayer", new Dictionary<string, string>(), "Multiplayer");
            AddTextField(translationPanel, "Single Player Text", "singlePlayer", new Dictionary<string, string>(), "Single Player");
            
            translationGroup.Content = translationPanel;
            stackPanel.Children.Add(translationGroup);
            
            // UI Text Settings
            var uiGroup = new GroupBox { Header = "UI Text Settings", Margin = new Thickness(0, 5, 0, 5) };
            var uiPanel = new StackPanel();
            
            AddTextField(uiPanel, "Health Text", "healthText", new Dictionary<string, string>(), "Health");
            AddTextField(uiPanel, "Resources Text", "resourcesText", new Dictionary<string, string>(), "Resources");
            AddTextField(uiPanel, "Units Text", "unitsText", new Dictionary<string, string>(), "Units");
            AddTextField(uiPanel, "Buildings Text", "buildingsText", new Dictionary<string, string>(), "Buildings");
            AddTextField(uiPanel, "Research Text", "researchText", new Dictionary<string, string>(), "Research");
            AddTextField(uiPanel, "Level Text", "levelText", new Dictionary<string, string>(), "Level");
            AddTextField(uiPanel, "Experience Text", "experienceText", new Dictionary<string, string>(), "Experience");
            AddTextField(uiPanel, "Score Text", "scoreText", new Dictionary<string, string>(), "Score");
            
            uiGroup.Content = uiPanel;
            stackPanel.Children.Add(uiGroup);
            
            // Game Messages
            var messagesGroup = new GroupBox { Header = "Game Messages", Margin = new Thickness(0, 5, 0, 5) };
            var messagesPanel = new StackPanel();
            
            AddTextField(messagesPanel, "Victory Message", "victoryMessage", new Dictionary<string, string>(), "Victory!");
            AddTextField(messagesPanel, "Defeat Message", "defeatMessage", new Dictionary<string, string>(), "Defeat!");
            AddTextField(messagesPanel, "Pause Message", "pauseMessage", new Dictionary<string, string>(), "Game Paused");
            AddTextField(messagesPanel, "Resume Message", "resumeMessage", new Dictionary<string, string>(), "Game Resumed");
            AddTextField(messagesPanel, "Loading Message", "loadingMessage", new Dictionary<string, string>(), "Loading...");
            AddTextField(messagesPanel, "Saving Message", "savingMessage", new Dictionary<string, string>(), "Saving...");
            AddTextField(messagesPanel, "Error Message", "errorMessage", new Dictionary<string, string>(), "An error occurred");
            AddTextField(messagesPanel, "Success Message", "successMessage", new Dictionary<string, string>(), "Success!");
            
            messagesGroup.Content = messagesPanel;
            stackPanel.Children.Add(messagesGroup);
            
            languageGroup.Content = stackPanel;
            if (EasyModePanel.Content is StackPanel easyPanel)
            {
                easyPanel.Children.Add(languageGroup);
            }
            dynamicControls.Add(languageGroup);
        }
        
        // Helper methods for different field types
        private void AddTextField(Panel parent, string label, string key, Dictionary<string, string> settings, string defaultValue)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            
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
        
        private void AddDropdownField(Panel parent, string label, string key, Dictionary<string, string> settings, string defaultValue, string[] options)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            
            var labelControl = new TextBlock
            {
                Text = label + ":",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetColumn(labelControl, 0);
            
            var comboBox = new ComboBox
            {
                Margin = new Thickness(5),
                Tag = key
            };
            
            foreach (var option in options)
            {
                comboBox.Items.Add(option);
            }
            
            comboBox.SelectedItem = settings.ContainsKey(key) ? settings[key] : defaultValue;
            Grid.SetColumn(comboBox, 1);
            
            grid.Children.Add(labelControl);
            grid.Children.Add(comboBox);
            parent.Children.Add(grid);
            
            dynamicControls.Add(grid);
        }
    }
}
