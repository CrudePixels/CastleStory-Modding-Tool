using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CastleStoryLauncher
{
    public partial class ModDependencyViewer : Window
    {
        private ModDependencyManager dependencyManager;
        private List<ModMetadata> allMods = new List<ModMetadata>();
        private List<ModMetadata> filteredMods = new List<ModMetadata>();

        public ModDependencyViewer(ModDependencyManager dependencyManager)
        {
            InitializeComponent();
            this.dependencyManager = dependencyManager;
            LoadMods();
            SetupFilters();
        }

        private void LoadMods()
        {
            allMods = dependencyManager.GetAllMods();
            filteredMods = new List<ModMetadata>(allMods);
            ModListBox.ItemsSource = filteredMods;
        }

        private void SetupFilters()
        {
            // Setup author filter
            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add(new ComboBoxItem { Content = "All Authors", IsSelected = true });
            
            var authors = dependencyManager.GetAuthors();
            foreach (var author in authors)
            {
                CategoryFilter.Items.Add(new ComboBoxItem { Content = author });
            }

            // Hide tag filter for now
            TagFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TagFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var authorFilter = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var searchText = SearchBox.Text?.ToLower() ?? "";

            filteredMods = allMods.Where(mod =>
            {
                // Author filter
                if (authorFilter != "All Authors" && !mod.Author.Equals(authorFilter, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Search filter
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (!mod.Name.ToLower().Contains(searchText) &&
                        !mod.Description.ToLower().Contains(searchText) &&
                        !mod.Author.ToLower().Contains(searchText))
                        return false;
                }

                return true;
            }).ToList();

            ModListBox.ItemsSource = filteredMods;
        }

        private void ModListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModListBox.SelectedItem is ModMetadata selectedMod)
            {
                ShowModDependencies(selectedMod);
            }
        }

        private void ShowModDependencies(ModMetadata mod)
        {
            DependencyInfoPanel.Children.Clear();

            // Mod Info Header
            var headerPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            
            var nameText = new TextBlock
            {
                Text = mod.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 5)
            };
            headerPanel.Children.Add(nameText);

            var nameText2 = new TextBlock
            {
                Text = $"Name: {mod.Name}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                Margin = new Thickness(0, 0, 0, 2)
            };
            headerPanel.Children.Add(nameText2);

            var versionText = new TextBlock
            {
                Text = $"Version: {mod.Version}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                Margin = new Thickness(0, 0, 0, 2)
            };
            headerPanel.Children.Add(versionText);

            var authorText = new TextBlock
            {
                Text = $"Author: {mod.Author}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                Margin = new Thickness(0, 0, 0, 2)
            };
            headerPanel.Children.Add(authorText);

            var authorText2 = new TextBlock
            {
                Text = $"Author: {mod.Author}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.LightGray),
                Margin = new Thickness(0, 0, 0, 2)
            };
            headerPanel.Children.Add(authorText2);

            DependencyInfoPanel.Children.Add(headerPanel);

            // Dependencies
            if (mod.Dependencies != null)
            {
                if (mod.Dependencies.Dependencies.Count > 0)
                {
                    var depsHeader = new TextBlock
                    {
                        Text = "Required Dependencies:",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Colors.LightGreen),
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    DependencyInfoPanel.Children.Add(depsHeader);

                    foreach (var dep in mod.Dependencies.Dependencies)
                    {
                        var depText = new TextBlock
                        {
                            Text = $"• {dep}",
                            FontSize = 11,
                            Foreground = new SolidColorBrush(Colors.LightGreen),
                            Margin = new Thickness(10, 0, 0, 2)
                        };
                        DependencyInfoPanel.Children.Add(depText);
                    }
                }

                if (mod.Dependencies.OptionalDependencies.Count > 0)
                {
                    var optDepsHeader = new TextBlock
                    {
                        Text = "Optional Dependencies:",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Colors.Orange),
                        Margin = new Thickness(0, 10, 0, 5)
                    };
                    DependencyInfoPanel.Children.Add(optDepsHeader);

                    foreach (var dep in mod.Dependencies.OptionalDependencies)
                    {
                        var depText = new TextBlock
                        {
                            Text = $"• {dep} (Optional)",
                            FontSize = 11,
                            Foreground = new SolidColorBrush(Colors.Orange),
                            Margin = new Thickness(10, 0, 0, 2)
                        };
                        DependencyInfoPanel.Children.Add(depText);
                    }
                }
            }

            // Conflicts
            if (mod.Dependencies != null && mod.Dependencies.Conflicts.Count > 0)
            {
                var conflictsHeader = new TextBlock
                {
                    Text = "Conflicts:",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.Red),
                    Margin = new Thickness(0, 10, 0, 5)
                };
                DependencyInfoPanel.Children.Add(conflictsHeader);

                foreach (var conflict in mod.Dependencies.Conflicts)
                {
                    var conflictText = new TextBlock
                    {
                        Text = $"• {conflict}",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Colors.Red),
                        Margin = new Thickness(10, 0, 0, 2)
                    };
                    DependencyInfoPanel.Children.Add(conflictText);
                }
            }

            // Priority
            var priorityText = new TextBlock
            {
                Text = $"Priority: {mod.Priority}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Yellow),
                Margin = new Thickness(0, 10, 0, 5)
            };
            DependencyInfoPanel.Children.Add(priorityText);
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            // Get enabled mods (this would need to be passed from the main window)
            var enabledModIds = new List<string>(); // TODO: Get from main window
            
            var conflicts = dependencyManager.ValidateModDependencies(enabledModIds);
            
            if (conflicts.Count == 0)
            {
                MessageBox.Show("No dependency conflicts found!", "Validation Complete", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var conflictWindow = new DependencyConflictWindow(conflicts);
                conflictWindow.ShowDialog();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
