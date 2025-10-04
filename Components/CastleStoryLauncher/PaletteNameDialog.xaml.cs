using System;
using System.Windows;

namespace CastleStoryLauncher
{
    public partial class PaletteNameDialog : Window
    {
        public string PaletteName { get; private set; } = "";
        public string PaletteDescription { get; private set; } = "";

        public PaletteNameDialog()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        public PaletteNameDialog(string title, string prompt, string defaultName = "") : this()
        {
            Title = title;
            TitleText.Text = prompt;
            NameTextBox.Text = defaultName;
            PaletteName = defaultName;
        }

        private void NameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PaletteName = NameTextBox.Text.Trim();
            OKButton.IsEnabled = !string.IsNullOrWhiteSpace(PaletteName);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            PaletteName = NameTextBox.Text.Trim();
            PaletteDescription = DescriptionTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(PaletteName))
            {
                MessageBox.Show("Please enter a palette name.", "Invalid Name", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
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
