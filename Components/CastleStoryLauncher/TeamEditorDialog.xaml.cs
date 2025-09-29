using System;
using System.Windows;

namespace CastleStoryLauncher
{
    public partial class TeamEditorDialog : Window
    {
        private TeamManager.Team team;

        public TeamEditorDialog()
        {
            InitializeComponent();
            InitializeDialog();
        }

        public TeamEditorDialog(TeamManager.Team existingTeam) : this()
        {
            team = existingTeam;
            LoadTeamData();
        }

        private void InitializeDialog()
        {
            // Set default values
            TeamColorComboBox.SelectedIndex = 0;
            FactionComboBox.SelectedIndex = 0;
        }

        private void LoadTeamData()
        {
            if (team != null)
            {
                TeamNameTextBox.Text = team.Name;
                MaxPlayersTextBox.Text = team.MaxPlayers.ToString();
                
                // Set color
                foreach (System.Windows.Controls.ComboBoxItem item in TeamColorComboBox.Items)
                {
                    if (item.Tag.ToString() == team.Color)
                    {
                        TeamColorComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // Set faction
                foreach (System.Windows.Controls.ComboBoxItem item in FactionComboBox.Items)
                {
                    if (item.Tag.ToString() == team.Faction)
                    {
                        FactionComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TeamNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a team name.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (!int.TryParse(MaxPlayersTextBox.Text, out int maxPlayers) || maxPlayers < 1)
                {
                    MessageBox.Show("Please enter a valid number for max players.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (team == null)
                {
                    team = new TeamManager.Team();
                }
                
                team.Name = TeamNameTextBox.Text.Trim();
                team.MaxPlayers = maxPlayers;
                
                if (TeamColorComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem colorItem)
                {
                    team.Color = colorItem.Tag.ToString();
                }
                
                if (FactionComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem factionItem)
                {
                    team.Faction = factionItem.Tag.ToString();
                }
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving team: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public TeamManager.Team GetTeam()
        {
            return team;
        }
    }
}
