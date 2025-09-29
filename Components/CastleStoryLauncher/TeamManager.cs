using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CastleStoryLauncher
{
    public class TeamManager
    {
        public class Team
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public List<string> Players { get; set; } = new List<string>();
            public bool IsActive { get; set; } = true;
            public int MaxPlayers { get; set; } = 4;
            public string Faction { get; set; } = "Neutral";
            public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        }

        public class Player
        {
            public string Name { get; set; } = string.Empty;
            public string SteamId { get; set; } = string.Empty;
            public int TeamId { get; set; } = -1;
            public bool IsOnline { get; set; } = false;
            public DateTime LastSeen { get; set; } = DateTime.Now;
            public Dictionary<string, object> Stats { get; set; } = new Dictionary<string, object>();
        }

        public List<Team> Teams { get; private set; } = new List<Team>();
        public List<Player> Players { get; private set; } = new List<Player>();
        public int MaxTeams { get; set; } = 8;
        public int MaxPlayersPerTeam { get; set; } = 4;
        public bool AutoBalanceTeams { get; set; } = true;
        public bool AllowTeamSwitching { get; set; } = true;
        public int TeamSwitchCooldown { get; set; } = 300; // 5 minutes in seconds

        private string configPath;
        private Dictionary<string, DateTime> lastTeamSwitch = new Dictionary<string, DateTime>();

        public TeamManager()
        {
            configPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "team_config.json");
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<TeamManagerConfig>(json);
                    
                    Teams = config.Teams ?? new List<Team>();
                    Players = config.Players ?? new List<Player>();
                    MaxTeams = config.MaxTeams;
                    MaxPlayersPerTeam = config.MaxPlayersPerTeam;
                    AutoBalanceTeams = config.AutoBalanceTeams;
                    AllowTeamSwitching = config.AllowTeamSwitching;
                    TeamSwitchCooldown = config.TeamSwitchCooldown;
                }
                else
                {
                    CreateDefaultTeams();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading team configuration: {ex.Message}");
                CreateDefaultTeams();
            }
        }

        public void SaveConfiguration()
        {
            try
            {
                var config = new TeamManagerConfig
                {
                    Teams = Teams,
                    Players = Players,
                    MaxTeams = MaxTeams,
                    MaxPlayersPerTeam = MaxPlayersPerTeam,
                    AutoBalanceTeams = AutoBalanceTeams,
                    AllowTeamSwitching = AllowTeamSwitching,
                    TeamSwitchCooldown = TeamSwitchCooldown
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving team configuration: {ex.Message}");
            }
        }

        private void CreateDefaultTeams()
        {
            Teams.Clear();
            
            var defaultTeams = new[]
            {
                new Team { Id = 1, Name = "Red Team", Color = "#FF0000", MaxPlayers = MaxPlayersPerTeam, Faction = "Red" },
                new Team { Id = 2, Name = "Blue Team", Color = "#0000FF", MaxPlayers = MaxPlayersPerTeam, Faction = "Blue" },
                new Team { Id = 3, Name = "Green Team", Color = "#00FF00", MaxPlayers = MaxPlayersPerTeam, Faction = "Green" },
                new Team { Id = 4, Name = "Yellow Team", Color = "#FFFF00", MaxPlayers = MaxPlayersPerTeam, Faction = "Yellow" }
            };

            Teams.AddRange(defaultTeams);
        }

        public bool AddPlayer(string playerName, string? steamId = null)
        {
            if (Players.Any(p => p.Name == playerName))
            {
                return false; // Player already exists
            }

            var player = new Player
            {
                Name = playerName,
                SteamId = steamId ?? Guid.NewGuid().ToString(),
                IsOnline = true,
                LastSeen = DateTime.Now
            };

            Players.Add(player);

            // Auto-assign to team if auto-balance is enabled
            if (AutoBalanceTeams)
            {
                AssignPlayerToBestTeam(player);
            }

            return true;
        }

        public bool RemovePlayer(string playerName)
        {
            var player = Players.FirstOrDefault(p => p.Name == playerName);
            if (player == null) return false;

            // Remove from current team
            if (player.TeamId > 0)
            {
                var team = Teams.FirstOrDefault(t => t.Id == player.TeamId);
                team?.Players.Remove(playerName);
            }

            Players.Remove(player);
            return true;
        }

        public bool AssignPlayerToTeam(string playerName, int teamId)
        {
            var player = Players.FirstOrDefault(p => p.Name == playerName);
            var team = Teams.FirstOrDefault(t => t.Id == teamId);

            if (player == null || team == null) return false;

            // Check if team is full
            if (team.Players.Count >= team.MaxPlayers) return false;

            // Check team switch cooldown
            if (lastTeamSwitch.ContainsKey(playerName))
            {
                var timeSinceLastSwitch = DateTime.Now - lastTeamSwitch[playerName];
                if (timeSinceLastSwitch.TotalSeconds < TeamSwitchCooldown)
                {
                    return false; // Still in cooldown
                }
            }

            // Remove from current team
            if (player.TeamId > 0)
            {
                var currentTeam = Teams.FirstOrDefault(t => t.Id == player.TeamId);
                currentTeam?.Players.Remove(playerName);
            }

            // Add to new team
            player.TeamId = teamId;
            team.Players.Add(playerName);
            lastTeamSwitch[playerName] = DateTime.Now;

            return true;
        }

        public bool AssignPlayerToBestTeam(Player player)
        {
            // Find team with least players
            var bestTeam = Teams
                .Where(t => t.IsActive && t.Players.Count < t.MaxPlayers)
                .OrderBy(t => t.Players.Count)
                .FirstOrDefault();

            if (bestTeam == null) return false;

            return AssignPlayerToTeam(player.Name, bestTeam.Id);
        }

        public bool CreateTeam(string teamName, string color, string faction = "Neutral")
        {
            if (Teams.Count >= MaxTeams) return false;

            var newTeam = new Team
            {
                Id = Teams.Count > 0 ? Teams.Max(t => t.Id) + 1 : 1,
                Name = teamName,
                Color = color,
                Faction = faction,
                MaxPlayers = MaxPlayersPerTeam
            };

            Teams.Add(newTeam);
            return true;
        }

        public bool RemoveTeam(int teamId)
        {
            var team = Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return false;

            // Move all players to other teams or unassign them
            foreach (var playerName in team.Players.ToList())
            {
                var player = Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null)
                {
                    player.TeamId = -1;
                    if (AutoBalanceTeams)
                    {
                        AssignPlayerToBestTeam(player);
                    }
                }
            }

            Teams.Remove(team);
            return true;
        }

        public bool UpdateTeamSettings(int teamId, Dictionary<string, object> settings)
        {
            var team = Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return false;

            foreach (var setting in settings)
            {
                team.Settings[setting.Key] = setting.Value;
            }

            return true;
        }

        public List<Player> GetTeamPlayers(int teamId)
        {
            var team = Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return new List<Player>();

            return Players.Where(p => team.Players.Contains(p.Name)).ToList();
        }

        public Team GetPlayerTeam(string playerName)
        {
            var player = Players.FirstOrDefault(p => p.Name == playerName);
            if (player == null || player.TeamId <= 0) return null;

            return Teams.FirstOrDefault(t => t.Id == player.TeamId);
        }

        public Dictionary<string, int> GetTeamStats()
        {
            var stats = new Dictionary<string, int>();
            
            foreach (var team in Teams.Where(t => t.IsActive))
            {
                stats[team.Name] = team.Players.Count;
            }

            return stats;
        }

        public bool BalanceTeams()
        {
            if (!AutoBalanceTeams) return false;

            var activeTeams = Teams.Where(t => t.IsActive).ToList();
            if (activeTeams.Count < 2) return false;

            var unassignedPlayers = Players.Where(p => p.TeamId <= 0).ToList();
            var totalPlayers = Players.Count(p => p.TeamId > 0);
            var targetPlayersPerTeam = totalPlayers / activeTeams.Count;

            // Redistribute players to balance teams
            foreach (var team in activeTeams)
            {
                while (team.Players.Count < targetPlayersPerTeam && unassignedPlayers.Any())
                {
                    var player = unassignedPlayers.First();
                    AssignPlayerToTeam(player.Name, team.Id);
                    unassignedPlayers.Remove(player);
                }
            }

            return true;
        }

        public string GenerateLuaConfiguration()
        {
            var luaCode = "-- Team Configuration\n";
            luaCode += "-- Generated by Castle Story Modding Tool\n\n";
            
            luaCode += "TeamConfig = {\n";
            luaCode += $"    maxTeams = {MaxTeams},\n";
            luaCode += $"    maxPlayersPerTeam = {MaxPlayersPerTeam},\n";
            luaCode += $"    autoBalanceTeams = {AutoBalanceTeams.ToString().ToLower()},\n";
            luaCode += $"    allowTeamSwitching = {AllowTeamSwitching.ToString().ToLower()},\n";
            luaCode += $"    teamSwitchCooldown = {TeamSwitchCooldown},\n\n";
            
            luaCode += "    teams = {\n";
            foreach (var team in Teams.Where(t => t.IsActive))
            {
                luaCode += $"        {{\n";
                luaCode += $"            id = {team.Id},\n";
                luaCode += $"            name = \"{team.Name}\",\n";
                luaCode += $"            color = \"{team.Color}\",\n";
                luaCode += $"            faction = \"{team.Faction}\",\n";
                luaCode += $"            maxPlayers = {team.MaxPlayers},\n";
                luaCode += $"            players = {{";
                luaCode += string.Join(", ", team.Players.Select(p => $"\"{p}\""));
                luaCode += $"}}\n";
                luaCode += $"        }},\n";
            }
            luaCode += "    }\n";
            luaCode += "}\n\n";
            
            luaCode += "-- Function to get team by ID\n";
            luaCode += "function GetTeamById(id)\n";
            luaCode += "    for i, team in ipairs(TeamConfig.teams) do\n";
            luaCode += "        if team.id == id then\n";
            luaCode += "            return team\n";
            luaCode += "        end\n";
            luaCode += "    end\n";
            luaCode += "    return nil\n";
            luaCode += "end\n\n";
            
            luaCode += "-- Function to get player team\n";
            luaCode += "function GetPlayerTeam(playerName)\n";
            luaCode += "    for i, team in ipairs(TeamConfig.teams) do\n";
            luaCode += "        for j, player in ipairs(team.players) do\n";
            luaCode += "            if player == playerName then\n";
            luaCode += "                return team\n";
            luaCode += "            end\n";
            luaCode += "        end\n";
            luaCode += "    end\n";
            luaCode += "    return nil\n";
            luaCode += "end\n";
            
            return luaCode;
        }

        private class TeamManagerConfig
        {
            public List<Team> Teams { get; set; } = new List<Team>();
            public List<Player> Players { get; set; } = new List<Player>();
            public int MaxTeams { get; set; } = 8;
            public int MaxPlayersPerTeam { get; set; } = 4;
            public bool AutoBalanceTeams { get; set; } = true;
            public bool AllowTeamSwitching { get; set; } = true;
            public int TeamSwitchCooldown { get; set; } = 300;
        }
    }
}
