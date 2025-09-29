using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CastleStoryLauncher
{
    public partial class TeamManagerEditor : UserControl
    {
        private TeamManager teamManager;
        public event EventHandler<TeamManager> ConfigurationSaved;

        public TeamManagerEditor()
        {
            InitializeComponent();
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            try
            {
                teamManager = new TeamManager();
                RefreshUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing team manager: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshUI()
        {
            // Refresh teams list
            TeamsList.ItemsSource = teamManager.Teams.Where(t => t.IsActive).ToList();
            
            // Refresh players list
            PlayersList.ItemsSource = teamManager.Players.ToList();
            
            // Refresh player combo box
            PlayerComboBox.ItemsSource = teamManager.Players.Select(p => p.Name).ToList();
            
            // Refresh team combo box
            TeamComboBox.ItemsSource = teamManager.Teams.Where(t => t.IsActive).Select(t => new { t.Id, t.Name }).ToList();
            TeamComboBox.DisplayMemberPath = "Name";
            TeamComboBox.SelectedValuePath = "Id";
            
            // Update settings
            AutoBalanceCheckBox.IsChecked = teamManager.AutoBalanceTeams;
            AllowTeamSwitchCheckBox.IsChecked = teamManager.AllowTeamSwitching;
            MaxTeamsTextBox.Text = teamManager.MaxTeams.ToString();
            MaxPlayersPerTeamTextBox.Text = teamManager.MaxPlayersPerTeam.ToString();
        }

        private void AddTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new TeamEditorDialog();
                if (dialog.ShowDialog() == true)
                {
                    var team = dialog.GetTeam();
                    if (teamManager.CreateTeam(team.Name, team.Color, team.Faction))
                    {
                        RefreshUI();
                        MessageBox.Show("Team created successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create team. Maximum teams reached.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating team: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is TeamManager.Team team)
                {
                    var dialog = new TeamEditorDialog(team);
                    if (dialog.ShowDialog() == true)
                    {
                        var updatedTeam = dialog.GetTeam();
                        team.Name = updatedTeam.Name;
                        team.Color = updatedTeam.Color;
                        team.Faction = updatedTeam.Faction;
                        team.MaxPlayers = updatedTeam.MaxPlayers;
                        
                        RefreshUI();
                        MessageBox.Show("Team updated successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing team: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is TeamManager.Team team)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete team '{team.Name}'?", 
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        if (teamManager.RemoveTeam(team.Id))
                        {
                            RefreshUI();
                            MessageBox.Show("Team deleted successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete team.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting team: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPlayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var playerName = PlayerNameTextBox.Text.Trim();
                var steamId = SteamIdTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(playerName))
                {
                    MessageBox.Show("Please enter a player name.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (teamManager.AddPlayer(playerName, string.IsNullOrEmpty(steamId) ? null : steamId))
                {
                    PlayerNameTextBox.Clear();
                    SteamIdTextBox.Clear();
                    RefreshUI();
                    MessageBox.Show("Player added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Player already exists.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding player: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemovePlayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is TeamManager.Player player)
                {
                    var result = MessageBox.Show($"Are you sure you want to remove player '{player.Name}'?", 
                        "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        if (teamManager.RemovePlayer(player.Name))
                        {
                            RefreshUI();
                            MessageBox.Show("Player removed successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to remove player.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing player: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignPlayerToTeam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedPlayer = PlayerComboBox.SelectedItem?.ToString();
                var selectedTeamId = TeamComboBox.SelectedValue;
                
                if (string.IsNullOrEmpty(selectedPlayer) || selectedTeamId == null)
                {
                    MessageBox.Show("Please select both a player and a team.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (teamManager.AssignPlayerToTeam(selectedPlayer, (int)selectedTeamId))
                {
                    RefreshUI();
                    MessageBox.Show("Player assigned to team successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to assign player to team. Team may be full or player is in cooldown.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning player to team: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update settings
                teamManager.AutoBalanceTeams = AutoBalanceCheckBox.IsChecked ?? false;
                teamManager.AllowTeamSwitching = AllowTeamSwitchCheckBox.IsChecked ?? false;
                
                if (int.TryParse(MaxTeamsTextBox.Text, out int maxTeams))
                {
                    teamManager.MaxTeams = maxTeams;
                }
                
                if (int.TryParse(MaxPlayersPerTeamTextBox.Text, out int maxPlayersPerTeam))
                {
                    teamManager.MaxPlayersPerTeam = maxPlayersPerTeam;
                }
                
                // Save configuration
                teamManager.SaveConfiguration();
                
                // Notify parent
                ConfigurationSaved?.Invoke(this, teamManager);
                
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
                    Title = "Load Team Configuration",
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json"
                };
                
                if (openFileDialog.ShowDialog() == true)
                {
                    // This would load from a different file
                    MessageBox.Show("Load functionality would be implemented here.", "Load Configuration", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateLuaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var luaCode = teamManager.GenerateLuaConfiguration();
                
                var window = new Window
                {
                    Title = "Generated Lua Configuration",
                    Content = new ScrollViewer
                    {
                        Content = new TextBox
                        {
                            Text = luaCode,
                            IsReadOnly = true,
                            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                            FontSize = 12,
                            Background = System.Windows.Media.Brushes.Black,
                            Foreground = System.Windows.Media.Brushes.White,
                            TextWrapping = TextWrapping.Wrap
                        }
                    },
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Application.Current.MainWindow
                };
                
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating Lua code: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public TeamManager GetTeamManager()
        {
            return teamManager;
        }
    }
}
