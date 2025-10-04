using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace CastleStoryLauncher
{
    public partial class ColorPaletteEditor : Window
    {
        private ColorPaletteManager paletteManager;
        private ColorPalette? currentPalette;
        private FactionColor? selectedColor;

        public ColorPaletteEditor()
        {
            InitializeComponent();
            
            // Initialize palette manager
            var palettesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ColorPalettes");
            paletteManager = new ColorPaletteManager(palettesDir);
            
            // Subscribe to events
            paletteManager.PaletteChanged += OnPaletteChanged;
            paletteManager.PaletteSaved += OnPaletteSaved;
            paletteManager.PaletteDeleted += OnPaletteDeleted;
            
            LoadPalettes();
        }

        private void LoadPalettes()
        {
            PaletteListBox.ItemsSource = paletteManager.Palettes;
            
            // Select first palette if none selected
            if (PaletteListBox.Items.Count > 0 && currentPalette == null)
            {
                PaletteListBox.SelectedIndex = 0;
            }
        }

        private void PaletteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaletteListBox.SelectedItem is ColorPalette palette)
            {
                currentPalette = palette;
                paletteManager.SetCurrentPalette(palette);
                UpdatePaletteInfo();
                LoadColors();
            }
        }

        private void UpdatePaletteInfo()
        {
            if (currentPalette == null)
            {
                PaletteNameText.Text = "Select a palette";
                PaletteDescriptionText.Text = "";
                PaletteStatsText.Text = "";
                return;
            }

            PaletteNameText.Text = currentPalette.Name;
            PaletteDescriptionText.Text = currentPalette.Description;
            PaletteStatsText.Text = $"{currentPalette.Colors.Count} colors • Created: {currentPalette.CreatedDate:MMM dd, yyyy} • Modified: {currentPalette.LastModified:MMM dd, yyyy}";
            
            // Update button states
            ClonePaletteButton.IsEnabled = currentPalette != null;
            DeletePaletteButton.IsEnabled = currentPalette != null && currentPalette.IsCustom;
            SetDefaultButton.IsEnabled = currentPalette != null && !currentPalette.IsDefault;
            ExportButton.IsEnabled = currentPalette != null;
            AddColorButton.IsEnabled = currentPalette != null;
        }

        private void LoadColors()
        {
            if (currentPalette == null)
            {
                ColorItemsControl.ItemsSource = null;
                return;
            }

            ColorItemsControl.ItemsSource = currentPalette.Colors;
        }

        private void NewPaletteButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PaletteNameDialog("Create New Palette", "Enter a name for the new palette:");
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var newPalette = paletteManager.CreatePalette(dialog.PaletteName, dialog.PaletteDescription);
                    LoadPalettes();
                    PaletteListBox.SelectedItem = newPalette;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create palette: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClonePaletteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPalette == null) return;

            var dialog = new PaletteNameDialog("Clone Palette", $"Enter a name for the cloned palette:", currentPalette.Name + " Copy");
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var clonedPalette = paletteManager.ClonePalette(currentPalette, dialog.PaletteName);
                    LoadPalettes();
                    PaletteListBox.SelectedItem = clonedPalette;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to clone palette: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeletePaletteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPalette == null || !currentPalette.IsCustom) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the palette '{currentPalette.Name}'?\n\nThis action cannot be undone.",
                "Delete Palette",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    paletteManager.DeletePalette(currentPalette);
                    LoadPalettes();
                    
                    // Select first available palette
                    if (PaletteListBox.Items.Count > 0)
                    {
                        PaletteListBox.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete palette: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPalette == null) return;

            try
            {
                paletteManager.SetAsDefault(currentPalette);
                MessageBox.Show($"'{currentPalette.Name}' has been set as the default palette.", 
                    "Default Palette Set", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPalettes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set default palette: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPalette == null) return;

            var saveDialog = new SaveFileDialog
            {
                Title = "Export Color Palette",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = $"{currentPalette.Name}.json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    paletteManager.ExportPalette(currentPalette, saveDialog.FileName);
                    MessageBox.Show($"Palette '{currentPalette.Name}' exported successfully.", 
                        "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export palette: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Import Color Palette",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var importedPalette = paletteManager.ImportPalette(openDialog.FileName);
                    LoadPalettes();
                    PaletteListBox.SelectedItem = importedPalette;
                    MessageBox.Show($"Palette '{importedPalette.Name}' imported successfully.", 
                        "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to import palette: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPalette == null) return;

            var colorDialog = new ColorPickerDialog();
            if (colorDialog.ShowDialog() == true)
            {
                try
                {
                    var newColor = new FactionColor
                    {
                        Name = colorDialog.ColorName,
                        Color = colorDialog.SelectedColor
                    };

                    currentPalette.AddColor(newColor);
                    paletteManager.SavePalette(currentPalette);
                    LoadColors();
                    UpdatePaletteInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add color: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPalette == null || selectedColor == null) return;

            var colorDialog = new ColorPickerDialog(selectedColor.Name, selectedColor.Color);
            if (colorDialog.ShowDialog() == true)
            {
                try
                {
                    var updatedColor = new FactionColor
                    {
                        Name = colorDialog.ColorName,
                        Color = colorDialog.SelectedColor
                    };

                    currentPalette.UpdateColor(selectedColor, updatedColor);
                    paletteManager.SavePalette(currentPalette);
                    LoadColors();
                    UpdatePaletteInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update color: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPalette == null || selectedColor == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove the color '{selectedColor.Name}'?",
                "Remove Color",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    currentPalette.RemoveColor(selectedColor);
                    paletteManager.SavePalette(currentPalette);
                    LoadColors();
                    UpdatePaletteInfo();
                    selectedColor = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to remove color: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ColorBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is FactionColor color)
            {
                selectedColor = color;
                
                // Update visual selection
                foreach (Border colorBorder in FindVisualChildren<Border>(ColorItemsControl))
                {
                    if (colorBorder.DataContext is FactionColor)
                    {
                        colorBorder.BorderThickness = new Thickness(2);
                        colorBorder.BorderBrush = new SolidColorBrush(Colors.White);
                    }
                }
                
                border.BorderThickness = new Thickness(4);
                border.BorderBrush = new SolidColorBrush(Colors.Yellow);
                
                // Update button states
                EditColorButton.IsEnabled = true;
                RemoveColorButton.IsEnabled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnPaletteChanged(object? sender, ColorPalette palette)
        {
            // Handle palette change if needed
        }

        private void OnPaletteSaved(object? sender, ColorPalette palette)
        {
            // Handle palette save if needed
        }

        private void OnPaletteDeleted(object? sender, ColorPalette palette)
        {
            // Handle palette deletion if needed
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
