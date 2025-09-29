using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CastleStoryLauncher
{
    public partial class FactionColorEditor : UserControl
    {
        public class FactionColor
        {
            public string Name { get; set; }
            public Color Color { get; set; }
            public string HexValue => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";
        }

        public ObservableCollection<FactionColor> FactionColors { get; private set; }
        public FactionColor SelectedColor { get; private set; }
        public event EventHandler<FactionColor> ColorSelected;
        public event EventHandler ColorsSaved;

        public FactionColorEditor()
        {
            InitializeComponent();
            FactionColors = new ObservableCollection<FactionColor>();
            ColorPaletteItems.ItemsSource = FactionColors;
            LoadDefaultColors();
        }

        private void LoadDefaultColors()
        {
            FactionColors.Clear();
            
            // Default Castle Story faction colors
            var defaultColors = new[]
            {
                new FactionColor { Name = "Blue", Color = Colors.Blue },
                new FactionColor { Name = "Green", Color = Colors.Green },
                new FactionColor { Name = "Orange", Color = Colors.Orange },
                new FactionColor { Name = "Purple", Color = Colors.Purple },
                new FactionColor { Name = "Red", Color = Colors.Red },
                new FactionColor { Name = "Yellow", Color = Colors.Yellow },
                new FactionColor { Name = "Cyan", Color = Colors.Cyan },
                new FactionColor { Name = "Magenta", Color = Colors.Magenta },
                new FactionColor { Name = "Lime", Color = Colors.Lime },
                new FactionColor { Name = "Pink", Color = Colors.Pink },
                new FactionColor { Name = "Teal", Color = Colors.Teal },
                new FactionColor { Name = "Indigo", Color = Colors.Indigo },
                new FactionColor { Name = "Brown", Color = Colors.Brown },
                new FactionColor { Name = "Gray", Color = Colors.Gray },
                new FactionColor { Name = "Gold", Color = Colors.Gold },
                new FactionColor { Name = "Silver", Color = Colors.Silver },
                new FactionColor { Name = "Crimson", Color = Colors.Crimson },
                new FactionColor { Name = "Forest Green", Color = Colors.ForestGreen },
                new FactionColor { Name = "Navy", Color = Colors.Navy },
                new FactionColor { Name = "Maroon", Color = Colors.Maroon },
                new FactionColor { Name = "Olive", Color = Colors.Olive },
                new FactionColor { Name = "Turquoise", Color = Colors.Turquoise },
                new FactionColor { Name = "Violet", Color = Colors.Violet },
                new FactionColor { Name = "Coral", Color = Colors.Coral },
                new FactionColor { Name = "Khaki", Color = Colors.Khaki },
                new FactionColor { Name = "Salmon", Color = Colors.Salmon },
                new FactionColor { Name = "Lavender", Color = Colors.Lavender },
                new FactionColor { Name = "Mint", Color = Colors.MintCream },
                new FactionColor { Name = "Peach", Color = Colors.PeachPuff },
                new FactionColor { Name = "Sky Blue", Color = Colors.SkyBlue }
            };

            foreach (var color in defaultColors)
            {
                FactionColors.Add(color);
            }

            // Select first color by default
            if (FactionColors.Count > 0)
            {
                SelectColor(FactionColors[0]);
            }
        }

        private void ColorButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is FactionColor color)
            {
                SelectColor(color);
            }
        }

        private void SelectColor(FactionColor color)
        {
            SelectedColor = color;
            SelectedColorBrush.Color = color.Color;
            SelectedColorText.Text = color.Name;
            ColorNameTextBox.Text = color.Name;
            RedSlider.Value = color.Color.R;
            GreenSlider.Value = color.Color.G;
            BlueSlider.Value = color.Color.B;
            HexValueTextBox.Text = color.HexValue;
            
            ColorSelected?.Invoke(this, color);
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SelectedColor != null)
            {
                var newColor = Color.FromRgb(
                    (byte)RedSlider.Value,
                    (byte)GreenSlider.Value,
                    (byte)BlueSlider.Value
                );
                
                SelectedColorBrush.Color = newColor;
                HexValueTextBox.Text = $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                
                // Update the selected color
                SelectedColor.Color = newColor;
            }
        }

        private void HexValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedColor != null && !string.IsNullOrEmpty(HexValueTextBox.Text))
            {
                try
                {
                    var hexValue = HexValueTextBox.Text.Trim();
                    if (!hexValue.StartsWith("#"))
                        hexValue = "#" + hexValue;
                    
                    if (hexValue.Length == 7) // #RRGGBB format
                    {
                        var color = (Color)ColorConverter.ConvertFromString(hexValue);
                        SelectedColor.Color = color;
                        SelectedColorBrush.Color = color;
                        RedSlider.Value = color.R;
                        GreenSlider.Value = color.G;
                        BlueSlider.Value = color.B;
                    }
                }
                catch
                {
                    // Invalid hex format, ignore
                }
            }
        }

        private void AddColorButton_Click(object sender, RoutedEventArgs e)
        {
            var colorName = ColorNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(colorName))
            {
                MessageBox.Show("Please enter a color name.", "Invalid Input", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FactionColors.Any(c => c.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A color with this name already exists.", "Duplicate Name", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newColor = new FactionColor
            {
                Name = colorName,
                Color = Color.FromRgb((byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value)
            };

            FactionColors.Add(newColor);
            SelectColor(newColor);
        }

        private void UpdateColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedColor != null)
            {
                var newName = ColorNameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(newName))
                {
                    MessageBox.Show("Please enter a color name.", "Invalid Input", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check for duplicate names (excluding current color)
                if (FactionColors.Any(c => c != SelectedColor && c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A color with this name already exists.", "Duplicate Name", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedColor.Name = newName;
                SelectedColor.Color = Color.FromRgb((byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value);
                SelectedColorText.Text = newName;
                
                // Refresh the collection to update the UI
                var index = FactionColors.IndexOf(SelectedColor);
                FactionColors.RemoveAt(index);
                FactionColors.Insert(index, SelectedColor);
            }
        }

        private void DeleteColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedColor != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the color '{SelectedColor.Name}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    FactionColors.Remove(SelectedColor);
                    if (FactionColors.Count > 0)
                    {
                        SelectColor(FactionColors[0]);
                    }
                    else
                    {
                        SelectedColor = null;
                        SelectedColorText.Text = "No Color Selected";
                        SelectedColorBrush.Color = Colors.Gray;
                    }
                }
            }
        }

        private void LoadDefaultPreset_Click(object sender, RoutedEventArgs e)
        {
            LoadDefaultColors();
        }

        private void LoadRainbowPreset_Click(object sender, RoutedEventArgs e)
        {
            FactionColors.Clear();
            
            var rainbowColors = new[]
            {
                new FactionColor { Name = "Red", Color = Colors.Red },
                new FactionColor { Name = "Orange", Color = Colors.Orange },
                new FactionColor { Name = "Yellow", Color = Colors.Yellow },
                new FactionColor { Name = "Green", Color = Colors.Green },
                new FactionColor { Name = "Cyan", Color = Colors.Cyan },
                new FactionColor { Name = "Blue", Color = Colors.Blue },
                new FactionColor { Name = "Purple", Color = Colors.Purple },
                new FactionColor { Name = "Pink", Color = Colors.Pink },
                new FactionColor { Name = "Magenta", Color = Colors.Magenta },
                new FactionColor { Name = "Violet", Color = Colors.Violet },
                new FactionColor { Name = "Indigo", Color = Colors.Indigo },
                new FactionColor { Name = "Teal", Color = Colors.Teal }
            };

            foreach (var color in rainbowColors)
            {
                FactionColors.Add(color);
            }

            if (FactionColors.Count > 0)
            {
                SelectColor(FactionColors[0]);
            }
        }

        private void LoadPastelPreset_Click(object sender, RoutedEventArgs e)
        {
            FactionColors.Clear();
            
            var pastelColors = new[]
            {
                new FactionColor { Name = "Pastel Pink", Color = Color.FromRgb(255, 182, 193) },
                new FactionColor { Name = "Pastel Blue", Color = Color.FromRgb(174, 198, 207) },
                new FactionColor { Name = "Pastel Green", Color = Color.FromRgb(152, 251, 152) },
                new FactionColor { Name = "Pastel Yellow", Color = Color.FromRgb(255, 255, 224) },
                new FactionColor { Name = "Pastel Purple", Color = Color.FromRgb(221, 160, 221) },
                new FactionColor { Name = "Pastel Orange", Color = Color.FromRgb(255, 218, 185) },
                new FactionColor { Name = "Pastel Mint", Color = Color.FromRgb(189, 252, 201) },
                new FactionColor { Name = "Pastel Lavender", Color = Color.FromRgb(230, 230, 250) },
                new FactionColor { Name = "Pastel Peach", Color = Color.FromRgb(255, 218, 185) },
                new FactionColor { Name = "Pastel Coral", Color = Color.FromRgb(255, 127, 80) },
                new FactionColor { Name = "Pastel Sky", Color = Color.FromRgb(135, 206, 235) },
                new FactionColor { Name = "Pastel Rose", Color = Color.FromRgb(255, 228, 225) }
            };

            foreach (var color in pastelColors)
            {
                FactionColors.Add(color);
            }

            if (FactionColors.Count > 0)
            {
                SelectColor(FactionColors[0]);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ColorsSaved?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the parent window and close it
            var parent = Parent;
            while (parent != null && !(parent is Window))
            {
                parent = ((FrameworkElement)parent).Parent;
            }
            
            if (parent is Window window)
            {
                window.Close();
            }
        }

        public string GenerateLuaCode()
        {
            var luaCode = "-- Faction Colors Configuration\n";
            luaCode += "-- Generated by Castle Story Modding Tool\n\n";
            luaCode += "FactionColors = {\n";
            
            foreach (var color in FactionColors)
            {
                luaCode += $"    {{\n";
                luaCode += $"        name = \"{color.Name}\",\n";
                luaCode += $"        color = Color({color.Color.R}, {color.Color.G}, {color.Color.B}),\n";
                luaCode += $"        hex = \"{color.HexValue}\"\n";
                luaCode += $"    }},\n";
            }
            
            luaCode += "}\n\n";
            luaCode += "-- Function to get color by name\n";
            luaCode += "function GetFactionColor(name)\n";
            luaCode += "    for i, color in ipairs(FactionColors) do\n";
            luaCode += "        if color.name == name then\n";
            luaCode += "            return color.color\n";
            luaCode += "        end\n";
            luaCode += "    end\n";
            luaCode += "    return Color(255, 255, 255) -- Default white\n";
            luaCode += "end\n";
            
            return luaCode;
        }
    }
}
