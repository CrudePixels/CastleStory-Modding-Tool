using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CastleStoryLauncher
{
    public partial class GameInstallationPicker : Window
    {
        public GameInstallation SelectedInstallation { get; private set; }

        public GameInstallationPicker(List<GameInstallation> installations)
        {
            InitializeComponent();
            InstallationListBox.ItemsSource = installations;
        }

        private void InstallationListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectButton.IsEnabled = InstallationListBox.SelectedItem != null;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedInstallation = InstallationListBox.SelectedItem as GameInstallation;
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
