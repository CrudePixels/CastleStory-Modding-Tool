using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CastleStoryLauncher
{
    public partial class ColorPickerDialog : Window
    {
        public string ColorName { get; private set; } = "";
        public Color SelectedColor { get; private set; } = Colors.White;

        public ColorPickerDialog()
        {
            InitializeComponent();
            ColorNameTextBox.Focus();
            UpdateColorPreview();
        }

        public ColorPickerDialog(string name, Color color) : this()
        {
            ColorNameTextBox.Text = name;
            ColorName = name;
            SelectedColor = color;
            UpdateSlidersFromColor(color);
            UpdateColorPreview();
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newColor = Color.FromRgb(
                (byte)RedSlider.Value,
                (byte)GreenSlider.Value,
                (byte)BlueSlider.Value
            );
            
            SelectedColor = newColor;
            UpdateColorPreview();
            UpdateHexValue();
        }

        private void HexValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(HexValueTextBox.Text)) return;

            try
            {
                var hexValue = HexValueTextBox.Text.Trim();
                if (!hexValue.StartsWith("#"))
                    hexValue = "#" + hexValue;
                
                if (hexValue.Length == 7) // #RRGGBB format
                {
                    var color = (Color)ColorConverter.ConvertFromString(hexValue);
                    SelectedColor = color;
                    UpdateSlidersFromColor(color);
                    UpdateColorPreview();
                }
            }
            catch
            {
                // Invalid hex format, ignore
            }
        }

        private void QuickColor_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string colorName)
            {
                var color = GetColorFromName(colorName);
                SelectedColor = color;
                UpdateSlidersFromColor(color);
                UpdateColorPreview();
                UpdateHexValue();
            }
        }

        private void UpdateColorPreview()
        {
            ColorPreviewBorder.Background = new SolidColorBrush(SelectedColor);
        }

        private void UpdateSlidersFromColor(Color color)
        {
            RedSlider.Value = color.R;
            GreenSlider.Value = color.G;
            BlueSlider.Value = color.B;
            RedValue.Text = color.R.ToString();
            GreenValue.Text = color.G.ToString();
            BlueValue.Text = color.B.ToString();
        }

        private void UpdateHexValue()
        {
            HexValueTextBox.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
        }

        private Color GetColorFromName(string colorName)
        {
            return colorName.ToLower() switch
            {
                "red" => Colors.Red,
                "green" => Colors.Green,
                "blue" => Colors.Blue,
                "yellow" => Colors.Yellow,
                "orange" => Colors.Orange,
                "purple" => Colors.Purple,
                "cyan" => Colors.Cyan,
                "magenta" => Colors.Magenta,
                "white" => Colors.White,
                "black" => Colors.Black,
                _ => Colors.White
            };
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ColorName = ColorNameTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(ColorName))
            {
                MessageBox.Show("Please enter a color name.", "Invalid Name", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ColorNameTextBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
