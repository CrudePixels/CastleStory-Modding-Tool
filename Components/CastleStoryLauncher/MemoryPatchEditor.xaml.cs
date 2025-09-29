using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CastleStoryLauncher
{
    public partial class MemoryPatchEditor : UserControl
    {
        private MemoryPatchConfig config;
        private string configPath;
        public event EventHandler<MemoryPatchConfig> ConfigurationSaved;

        public MemoryPatchEditor()
        {
            InitializeComponent();
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            try
            {
                // Set default config path
                configPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "memory_patch_config.json");
                
                // Load or create default configuration
                config = MemoryPatchConfig.LoadFromFile(configPath);
                
                // Bind data to UI
                CategoryItems.ItemsSource = config.Categories;
                
                // Set global settings
                EnableAllPatchesCheckBox.IsChecked = config.EnableAllPatches;
                EnableLoggingCheckBox.IsChecked = config.EnableLogging;
                
                // Set log level
                foreach (ComboBoxItem item in LogLevelComboBox.Items)
                {
                    if (item.Tag.ToString() == config.LogLevel)
                    {
                        LogLevelComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // Select first patch by default
                if (config.Categories.Count > 0 && config.Categories[0].Patterns.Count > 0)
                {
                    SelectPatch(config.Categories[0].Patterns[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing memory patch editor: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectPatch(MemoryPatchConfig.PatchPattern patch)
        {
            if (patch == null) return;
            
            SelectedPatchName.Text = patch.Name;
            SelectedPatchDescription.Text = patch.Description;
            PatchStatus.Text = $"Status: {(patch.Enabled ? "Enabled" : "Disabled")}";
            PatchCategory.Text = $"Category: {patch.Category}";
            PatchEnabled.Text = $"Enabled: {(patch.Enabled ? "Yes" : "No")}";
        }

        private void CategoryEnableAll_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is MemoryPatchConfig.PatchCategory category)
            {
                foreach (var pattern in category.Patterns)
                {
                    pattern.Enabled = true;
                }
            }
        }

        private void CategoryEnableAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is MemoryPatchConfig.PatchCategory category)
            {
                foreach (var pattern in category.Patterns)
                {
                    pattern.Enabled = false;
                }
            }
        }

        private void PatternEnable_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is MemoryPatchConfig.PatchPattern pattern)
            {
                pattern.Enabled = true;
                SelectPatch(pattern);
            }
        }

        private void PatternEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is MemoryPatchConfig.PatchPattern pattern)
            {
                pattern.Enabled = false;
                SelectPatch(pattern);
            }
        }

        private void EnableAllPatches_Checked(object sender, RoutedEventArgs e)
        {
            config.EnableAllPatches = true;
        }

        private void EnableAllPatches_Unchecked(object sender, RoutedEventArgs e)
        {
            config.EnableAllPatches = false;
        }

        private void EnableLogging_Checked(object sender, RoutedEventArgs e)
        {
            config.EnableLogging = true;
        }

        private void EnableLogging_Unchecked(object sender, RoutedEventArgs e)
        {
            config.EnableLogging = false;
        }

        private void TestPatch_Click(object sender, RoutedEventArgs e)
        {
            // This would test the selected patch without applying it
            MessageBox.Show("Patch testing functionality would be implemented here.", "Test Patch", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyPatch_Click(object sender, RoutedEventArgs e)
        {
            // This would apply the selected patch
            MessageBox.Show("Patch application functionality would be implemented here.", "Apply Patch", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RevertPatch_Click(object sender, RoutedEventArgs e)
        {
            // This would revert the selected patch
            MessageBox.Show("Patch reversion functionality would be implemented here.", "Revert Patch", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update log level from combo box
                if (LogLevelComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    config.LogLevel = selectedItem.Tag.ToString();
                }
                
                // Save configuration
                config.SaveToFile(configPath);
                
                // Notify parent
                ConfigurationSaved?.Invoke(this, config);
                
                MessageBox.Show("Configuration saved successfully!", "Save Configuration", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Load Memory Patch Configuration",
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json"
                };
                
                if (openFileDialog.ShowDialog() == true)
                {
                    config = MemoryPatchConfig.LoadFromFile(openFileDialog.FileName);
                    CategoryItems.ItemsSource = config.Categories;
                    
                    // Update global settings
                    EnableAllPatchesCheckBox.IsChecked = config.EnableAllPatches;
                    EnableLoggingCheckBox.IsChecked = config.EnableLogging;
                    
                    MessageBox.Show("Configuration loaded successfully!", "Load Configuration", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to reset to default configuration? This will lose all current settings.", 
                    "Reset Configuration", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    config = MemoryPatchConfig.CreateDefaultConfig();
                    CategoryItems.ItemsSource = config.Categories;
                    
                    // Update global settings
                    EnableAllPatchesCheckBox.IsChecked = config.EnableAllPatches;
                    EnableLoggingCheckBox.IsChecked = config.EnableLogging;
                    
                    MessageBox.Show("Configuration reset to defaults!", "Reset Configuration", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MemoryPatchConfig GetConfiguration()
        {
            return config;
        }

        public void SetConfiguration(MemoryPatchConfig newConfig)
        {
            config = newConfig;
            CategoryItems.ItemsSource = config.Categories;
            
            // Update global settings
            EnableAllPatchesCheckBox.IsChecked = config.EnableAllPatches;
            EnableLoggingCheckBox.IsChecked = config.EnableLogging;
        }
    }
}
