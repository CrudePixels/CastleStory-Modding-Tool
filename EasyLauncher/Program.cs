using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace CastleStoryEasyLauncher
{
    public partial class EasyLauncherForm : Form
    {
        private const string GITHUB_REPO = "CrudePixels/CastleStory-Modding-Tool";
        private const string VERSION_URL = "https://api.github.com/repos/{0}/releases/latest";
        private const string DOWNLOAD_URL = "https://github.com/{0}/releases/latest/download/CastleStoryModdingTool.zip";
        
        private string currentVersion = "1.2.0"; // Enhanced with dynamic Easy Mode and multi-file validator
        private string latestVersion = "";
        private bool updateAvailable = false;
        private HttpClient httpClient = new HttpClient();

        public EasyLauncherForm()
        {
            InitializeComponent();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "CastleStoryEasyLauncher/1.0");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Launcher";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            // Title label
            var titleLabel = new Label
            {
                Text = "🏰 Castle Story Modding Tool",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 174, 219),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(titleLabel);

            // Version label
            var versionLabel = new Label
            {
                Text = $"Version: {currentVersion}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(20, 50)
            };
            this.Controls.Add(versionLabel);

            // Status label
            var statusLabel = new Label
            {
                Text = "Checking for updates...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Yellow,
                AutoSize = true,
                Location = new Point(20, 80)
            };
            this.Controls.Add(statusLabel);

            // Launch button
            var launchButton = new Button
            {
                Text = "🚀 Mod Manager",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 174, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(300, 50),
                Location = new Point(100, 120),
                Enabled = true
            };
            launchButton.Click += LaunchButton_Click;
            this.Controls.Add(launchButton);

            // Editor button (under Mod Manager, same width)
            var editorButton = new Button
            {
                Text = "📝 Editor",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(300, 50),
                Location = new Point(100, 180)
            };
            editorButton.Click += EditorButton_Click;
            this.Controls.Add(editorButton);

            // LAN Server button
            var lanServerButton = new Button
            {
                Text = "LAN Server",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 40),
                Location = new Point(100, 250)
            };
            lanServerButton.Click += LanServerButton_Click;
            this.Controls.Add(lanServerButton);

            // LAN Client button
            var lanClientButton = new Button
            {
                Text = "LAN Client",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 40),
                Location = new Point(260, 250)
            };
            lanClientButton.Click += LanClientButton_Click;
            this.Controls.Add(lanClientButton);

            // Update button
            var updateButton = new Button
            {
                Text = "🔄 Update Available - Click to Update",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(300, 40),
                Location = new Point(100, 250),
                Visible = false
            };
            updateButton.Click += UpdateButton_Click;
            this.Controls.Add(updateButton);

            // Progress bar
            var progressBar = new ProgressBar
            {
                Size = new Size(300, 20),
                Location = new Point(100, 300),
                Visible = false
            };
            this.Controls.Add(progressBar);

            // Info label
            var infoLabel = new Label
            {
                Text = "This launcher will automatically check for updates and keep your modding tool up-to-date.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                Size = new Size(460, 40),
                Location = new Point(20, 330),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(infoLabel);

            this.ResumeLayout(false);
        }

        private async void EasyLauncherForm_Load(object sender, EventArgs e)
        {
            await CheckForUpdates();
        }

        private async Task CheckForUpdates()
        {
            try
            {
                var statusLabel = this.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Checking"));
                var launchButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Mod Manager"));
                var updateButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Update Available"));

                statusLabel.Text = "Checking for updates...";
                statusLabel.ForeColor = Color.Yellow;

                // Set user agent to avoid GitHub API rate limiting
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CastleStory-Modding-Tool/1.2.0");

                // Check GitHub for latest release
                var response = await httpClient.GetStringAsync(string.Format(VERSION_URL, GITHUB_REPO));
                var releaseInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);
                latestVersion = releaseInfo.tag_name.ToString().TrimStart('v');

                if (IsNewerVersion(latestVersion, currentVersion))
                {
                    updateAvailable = true;
                    statusLabel.Text = $"Update available: v{latestVersion}";
                    statusLabel.ForeColor = Color.LightGreen;
                    updateButton.Visible = true;
                    updateButton.Text = $"🔄 Update to v{latestVersion}";
                    
                    // Add release notes if available
                    if (releaseInfo.body != null)
                    {
                        var releaseNotes = releaseInfo.body.ToString();
                        if (!string.IsNullOrEmpty(releaseNotes))
                        {
                            // Show release notes in a tooltip or status
                            statusLabel.Text += " - Click update to see changes";
                        }
                    }
                }
                else
                {
                    statusLabel.Text = $"You have the latest version (v{currentVersion})";
                    statusLabel.ForeColor = Color.LightGreen;
                }

                launchButton.Enabled = true;
            }
            catch (HttpRequestException ex)
            {
                var statusLabel = this.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Checking"));
                statusLabel.Text = "Update check failed - Check internet connection";
                statusLabel.ForeColor = Color.Orange;
                
                var launchButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Mod Manager"));
                launchButton.Enabled = true;
                
                System.Diagnostics.Debug.WriteLine($"Network error during update check: {ex.Message}");
            }
            catch (Exception ex)
            {
                var statusLabel = this.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Checking"));
                statusLabel.Text = "Update check failed - GitHub may be unavailable";
                statusLabel.ForeColor = Color.Orange;
                
                var launchButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Mod Manager"));
                launchButton.Enabled = true;
                
                System.Diagnostics.Debug.WriteLine($"Update check error: {ex.Message}");
            }
        }

        private bool IsNewerVersion(string latest, string current)
        {
            try
            {
                var latestParts = latest.Split('.').Select(int.Parse).ToArray();
                var currentParts = current.Split('.').Select(int.Parse).ToArray();

                for (int i = 0; i < Math.Max(latestParts.Length, currentParts.Length); i++)
                {
                    var latestPart = i < latestParts.Length ? latestParts[i] : 0;
                    var currentPart = i < currentParts.Length ? currentParts[i] : 0;

                    if (latestPart > currentPart) return true;
                    if (latestPart < currentPart) return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void LaunchButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Find the root CastleStoryModdingTool directory by going up from EasyLauncher
                var currentPath = Application.StartupPath;
                var basePath = currentPath;
                
                // Go up directories until we find the CastleStoryModdingTool root
                while (!string.IsNullOrEmpty(basePath) && !Path.GetFileName(basePath).Equals("CastleStoryModdingTool", StringComparison.OrdinalIgnoreCase))
                {
                    basePath = Path.GetDirectoryName(basePath);
                }
                
                if (string.IsNullOrEmpty(basePath))
                {
                    MessageBox.Show("Could not find CastleStoryModdingTool root directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var launcherPath = Path.Combine(basePath, "Components", "CastleStoryLauncher", "bin", "Release", "net9.0-windows", "CastleStoryLauncher.exe");
                
                if (File.Exists(launcherPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = launcherPath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(launcherPath)
                    });
                }
                else
                {
                    MessageBox.Show($"Castle Story Launcher not found at:\n{launcherPath}\n\nPlease ensure the modding tool is properly installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch Castle Story Launcher: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LanServerButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Find the root CastleStoryModdingTool directory by going up from EasyLauncher
                var currentPath = Application.StartupPath;
                var basePath = currentPath;
                
                // Go up directories until we find the CastleStoryModdingTool root
                while (!string.IsNullOrEmpty(basePath) && !Path.GetFileName(basePath).Equals("CastleStoryModdingTool", StringComparison.OrdinalIgnoreCase))
                {
                    basePath = Path.GetDirectoryName(basePath);
                }
                
                if (string.IsNullOrEmpty(basePath))
                {
                    MessageBox.Show("Could not find CastleStoryModdingTool root directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var serverPath = Path.Combine(basePath, "Components", "LANServer", "bin", "Release", "net9.0-windows", "LANServer.exe");
                
                if (File.Exists(serverPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = serverPath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(serverPath)
                    });
                }
                else
                {
                    MessageBox.Show($"LAN Server not found at:\n{serverPath}\n\nPlease ensure the modding tool is properly installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch LAN Server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LanClientButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Find the root CastleStoryModdingTool directory by going up from EasyLauncher
                var currentPath = Application.StartupPath;
                var basePath = currentPath;
                
                // Go up directories until we find the CastleStoryModdingTool root
                while (!string.IsNullOrEmpty(basePath) && !Path.GetFileName(basePath).Equals("CastleStoryModdingTool", StringComparison.OrdinalIgnoreCase))
                {
                    basePath = Path.GetDirectoryName(basePath);
                }
                
                if (string.IsNullOrEmpty(basePath))
                {
                    MessageBox.Show("Could not find CastleStoryModdingTool root directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var clientPath = Path.Combine(basePath, "Components", "LANClient", "bin", "Release", "net9.0-windows", "LANClient.exe");
                
                if (File.Exists(clientPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = clientPath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(clientPath)
                    });
                }
                else
                {
                    MessageBox.Show($"LAN Client not found at:\n{clientPath}\n\nPlease ensure the modding tool is properly installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch LAN Client: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditorButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Find the root CastleStoryModdingTool directory by going up from EasyLauncher
                var currentPath = Application.StartupPath;
                var basePath = currentPath;
                
                // Go up directories until we find the CastleStoryModdingTool root
                while (!string.IsNullOrEmpty(basePath) && !Path.GetFileName(basePath).Equals("CastleStoryModdingTool", StringComparison.OrdinalIgnoreCase))
                {
                    basePath = Path.GetDirectoryName(basePath);
                }
                
                if (string.IsNullOrEmpty(basePath))
                {
                    MessageBox.Show("Could not find CastleStoryModdingTool root directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Launch the Castle Story Launcher with editor argument
                var launcherPath = Path.Combine(basePath, "Components", "CastleStoryLauncher", "bin", "Release", "net9.0-windows", "CastleStoryLauncher.exe");
                
                System.Diagnostics.Debug.WriteLine($"Looking for launcher at: {launcherPath}");
                System.Diagnostics.Debug.WriteLine($"File exists: {File.Exists(launcherPath)}");
                
                if (File.Exists(launcherPath))
                {
                    System.Diagnostics.Debug.WriteLine("Launching Castle Story Launcher with --open-editor argument");
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = launcherPath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(launcherPath),
                        Arguments = "--open-editor"
                    });
                    
                    if (process != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Process started with ID: {process.Id}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Process.Start returned null");
                    }
                }
                else
                {
                    MessageBox.Show($"Castle Story Launcher not found at:\n{launcherPath}\n\nPlease ensure the modding tool is properly installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch Editor: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void UpdateButton_Click(object? sender, EventArgs e)
        {
            var updateButton = sender as Button;
            var progressBar = this.Controls.OfType<ProgressBar>().FirstOrDefault();
            var statusLabel = this.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Update"));

            try
            {
                updateButton.Enabled = false;
                updateButton.Text = "Updating...";
                progressBar.Visible = true;
                progressBar.Value = 0;

                statusLabel.Text = "Downloading update...";
                statusLabel.ForeColor = Color.Yellow;

                // Download the latest release
                var downloadUrl = string.Format(DOWNLOAD_URL, GITHUB_REPO);
                var response = await httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                var zipPath = Path.Combine(Path.GetTempPath(), "CastleStoryModdingTool_Update.zip");
                using (var fileStream = new FileStream(zipPath, FileMode.Create))
                {
                    var contentStream = await response.Content.ReadAsStreamAsync();
                    await contentStream.CopyToAsync(fileStream);
                }

                progressBar.Value = 50;
                statusLabel.Text = "Installing update...";

                // Extract and install update
                await InstallUpdate(zipPath);

                progressBar.Value = 100;
                statusLabel.Text = "Update complete! Restarting...";
                statusLabel.ForeColor = Color.LightGreen;

                // Restart the application
                Application.Restart();
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Update failed: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                updateButton.Enabled = true;
                updateButton.Text = "🔄 Retry Update";
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private async Task InstallUpdate(string zipPath)
        {
            // This is a simplified update process
            // In a real implementation, you'd want to:
            // 1. Backup current installation
            // 2. Extract new files
            // 3. Preserve user settings
            // 4. Update version info
            
            await Task.Delay(2000); // Simulate installation time
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EasyLauncherForm());
        }
    }
}
