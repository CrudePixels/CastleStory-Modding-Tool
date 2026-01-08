using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastleStoryLANClient
{
    public class ServerInfo
    {
        public string Name { get; set; } = "";
        public int Port { get; set; }
        public int PlayerCount { get; set; }
        public string Version { get; set; } = "";
        public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 0);
    }

    public partial class LANClientGUI : Form
    {
        private UdpClient? udpClient;
        private TcpClient? tcpClient;
        private NetworkStream? stream;
        private List<ServerInfo> discoveredServers = new List<ServerInfo>();
        private bool isRunning = false;
        private bool isConnected = false;
        private System.Threading.Timer? autoDiscoverTimer;
        private string clientName = "LAN_Player";

        // UI Controls
        private Label? statusLabel;
        private ListBox? serversListBox;
        private TextBox? logTextBox;
        private Button? discoverButton;
        private Button? connectButton;
        private Button? disconnectButton;
        private TextBox? nameTextBox;
        private Button? setNameButton;
        private TextBox? messageTextBox;
        private Button? sendButton;
        private Label? serverInfoLabel;

        public LANClientGUI()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Castle Story LAN Client";
            this.Size = new Size(800, 700);
            this.MinimumSize = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Header panel
            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(this.Width - 20, 80),
                Location = new Point(10, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(headerPanel);

            // Client info label
            serverInfoLabel = new Label
            {
                Text = "🔗 Castle Story LAN Client",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.LightBlue,
                Size = new Size(400, 30),
                Location = new Point(15, 15)
            };
            headerPanel.Controls.Add(serverInfoLabel);

            // Status label
            statusLabel = new Label
            {
                Text = "Status: Disconnected",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Orange,
                Size = new Size(250, 25),
                Location = new Point(15, 45)
            };
            headerPanel.Controls.Add(statusLabel);

            // Connection quality indicator
            var qualityLabel = new Label
            {
                Text = "Quality: --",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Size = new Size(120, 20),
                Location = new Point(280, 48)
            };
            headerPanel.Controls.Add(qualityLabel);

            // Name section
            var nameLabel = new Label
            {
                Text = "Player Name:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(100, 25),
                Location = new Point(20, 100)
            };
            this.Controls.Add(nameLabel);

            nameTextBox = new TextBox
            {
                Text = clientName,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Size = new Size(150, 25),
                Location = new Point(130, 100)
            };
            this.Controls.Add(nameTextBox);

            setNameButton = new Button
            {
                Text = "✏️ Set Name",
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 30),
                Location = new Point(290, 98)
            };
            setNameButton.Click += SetNameButton_Click;
            this.Controls.Add(setNameButton);

            // Servers panel
            var serversPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(760, 200),
                Location = new Point(10, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(serversPanel);

            var serversLabel = new Label
            {
                Text = "🌐 Discovered Servers:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(200, 25),
                Location = new Point(10, 10)
            };
            serversPanel.Controls.Add(serversLabel);

            var serverCountLabel = new Label
            {
                Text = "0 servers",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                Size = new Size(100, 20),
                Location = new Point(220, 12)
            };
            serversPanel.Controls.Add(serverCountLabel);

            serversListBox = new ListBox
            {
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                Size = new Size(740, 140),
                Location = new Point(10, 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            serversListBox.SelectedIndexChanged += (s, e) => {
                ServersListBox_SelectedIndexChanged(s, e);
                serverCountLabel.Text = $"{discoveredServers.Count} server(s)";
            };
            serversPanel.Controls.Add(serversListBox);

            // Control panel
            var controlPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(760, 120),
                Location = new Point(10, 310),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(controlPanel);

            // Server buttons
            discoverButton = new Button
            {
                Text = "🔍 Discover Servers",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(150, 40),
                Location = new Point(10, 10)
            };
            discoverButton.Click += DiscoverButton_Click;
            controlPanel.Controls.Add(discoverButton);

            connectButton = new Button
            {
                Text = "🔗 Connect",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(120, 40),
                Location = new Point(170, 10),
                Enabled = false
            };
            connectButton.Click += ConnectButton_Click;
            controlPanel.Controls.Add(connectButton);

            disconnectButton = new Button
            {
                Text = "❌ Disconnect",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(120, 40),
                Location = new Point(300, 10),
                Enabled = false
            };
            disconnectButton.Click += DisconnectButton_Click;
            controlPanel.Controls.Add(disconnectButton);

            // Auto-reconnect checkbox
            var autoReconnectCheck = new CheckBox
            {
                Text = "Auto-reconnect on disconnect",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Size = new Size(200, 25),
                Location = new Point(10, 60),
                Checked = false
            };
            controlPanel.Controls.Add(autoReconnectCheck);

            // Message section
            var messageLabel = new Label
            {
                Text = "Send Message:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(100, 25),
                Location = new Point(20, 360)
            };
            this.Controls.Add(messageLabel);

            messageTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Size = new Size(300, 25),
                Location = new Point(20, 390),
                Enabled = false
            };
            this.Controls.Add(messageTextBox);

            sendButton = new Button
            {
                Text = "📤 Send",
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(330, 388),
                Enabled = false
            };
            sendButton.Click += SendButton_Click;
            this.Controls.Add(sendButton);

            // Log panel
            var logPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(this.Width - 20, 200),
                Location = new Point(10, 440),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(logPanel);

            var logHeader = new Panel
            {
                BackColor = Color.FromArgb(30, 30, 30),
                Size = new Size(logPanel.Width, 30),
                Location = new Point(0, 0)
            };
            logPanel.Controls.Add(logHeader);

            var logLabel = new Label
            {
                Text = "📋 Client Log:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(150, 25),
                Location = new Point(10, 3)
            };
            logHeader.Controls.Add(logLabel);

            var clearLogButton = new Button
            {
                Text = "Clear",
                Font = new Font("Segoe UI", 8),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(60, 25),
                Location = new Point(logPanel.Width - 80, 3)
            };
            clearLogButton.Click += (s, e) => logTextBox.Clear();
            logHeader.Controls.Add(clearLogButton);

            logTextBox = new TextBox
            {
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGray,
                Size = new Size(logPanel.Width - 20, 160),
                Location = new Point(10, 40),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            logPanel.Controls.Add(logTextBox);

            // Start auto-discovery
            StartAutoDiscovery();
        }

        private void SetNameButton_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(nameTextBox.Text))
            {
                clientName = nameTextBox.Text;
                LogMessage($"Name set to: {clientName}");
            }
        }

        private void ServersListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        private async void DiscoverButton_Click(object? sender, EventArgs e)
        {
            await DiscoverServers();
        }

        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            if (serversListBox.SelectedIndex >= 0 && serversListBox.SelectedIndex < discoveredServers.Count)
            {
                var server = discoveredServers[serversListBox.SelectedIndex];
                await ConnectToServer(server);
            }
            else
            {
                LogMessage("Please select a server to connect to.");
            }
        }

        private async void DisconnectButton_Click(object? sender, EventArgs e)
        {
            await Disconnect();
        }

        private async void SendButton_Click(object? sender, EventArgs e)
        {
            if (isConnected && !string.IsNullOrEmpty(messageTextBox.Text))
            {
                await SendMessage($"CHAT|{messageTextBox.Text}");
                messageTextBox.Clear();
            }
        }

        private void StartAutoDiscovery()
        {
            try
            {
                udpClient = new UdpClient(0);
                isRunning = true;
                
                LogMessage("Client started - Use Discover button to find servers");
                
                // Auto-discovery disabled - user can use Discover button
                
                // Start handling server responses
                Task.Run(HandleServerResponses);
            }
            catch (Exception ex)
            {
                LogMessage($"Client startup error: {ex.Message}");
            }
        }

        private async Task AutoDiscoverServers()
        {
            if (!isConnected && isRunning)
            {
                try
                {
                    var message = "DISCOVER_SERVERS";
                    var data = Encoding.UTF8.GetBytes(message);
                    var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 7778);
                    
                    await udpClient!.SendAsync(data, data.Length, broadcastEndPoint);
                }
                catch (Exception ex)
                {
                    LogMessage($"Auto-discovery error: {ex.Message}");
                }
            }
        }

        private async Task DiscoverServers()
        {
            LogMessage("Discovering LAN servers...");
            discoveredServers.Clear();
            UpdateServersList();
            
            try
            {
                var message = "DISCOVER_SERVERS";
                var data = Encoding.UTF8.GetBytes(message);
                var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 7778);
                
                await udpClient!.SendAsync(data, data.Length, broadcastEndPoint);
                LogMessage("Discovery request sent");
                
                await Task.Delay(3000);
                LogMessage($"Found {discoveredServers.Count} servers");
            }
            catch (Exception ex)
            {
                LogMessage($"Discovery error: {ex.Message}");
            }
        }

        private async Task HandleServerResponses()
        {
            while (isRunning)
            {
                try
                {
                    var result = await udpClient!.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    var serverEndPoint = result.RemoteEndPoint;
                    
                    if (message.StartsWith("SERVER_INFO|"))
                    {
                        var parts = message.Split('|');
                        if (parts.Length >= 5)
                        {
                            var serverInfo = new ServerInfo
                            {
                                Name = parts[1],
                                Port = int.Parse(parts[2]),
                                PlayerCount = int.Parse(parts[3]),
                                Version = parts[4],
                                EndPoint = new IPEndPoint(((IPEndPoint)serverEndPoint).Address, int.Parse(parts[2]))
                            };
                            
                            if (!discoveredServers.Any(s => s.Name == serverInfo.Name && s.EndPoint.Address.Equals(serverInfo.EndPoint.Address)))
                            {
                                discoveredServers.Add(serverInfo);
                                this.Invoke(new Action(() => {
                                    UpdateServersList();
                                    LogMessage($"Found server: {serverInfo.Name} ({serverInfo.PlayerCount} players)");
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        LogMessage($"Response handling error: {ex.Message}");
                }
            }
        }

        private async Task ConnectToServer(ServerInfo server)
        {
            try
            {
                // Disconnect from current server first if connected
                if (isConnected)
                {
                    LogMessage("Disconnecting from current server first...");
                    await Disconnect();
                }
                
                LogMessage($"Connecting to {server.Name}...");
                
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(server.EndPoint.Address, server.Port);
                stream = tcpClient.GetStream();
                
                isConnected = true;
                UpdateUI();
                
                LogMessage($"Connected to {server.Name}!");
                
                // Send client name
                await SendMessage($"SET_NAME|{clientName}");
                
                // Start handling server messages
                Task.Run(HandleServerMessages);
            }
            catch (Exception ex)
            {
                LogMessage($"Connection error: {ex.Message}");
            }
        }

        private async Task Disconnect()
        {
            if (tcpClient?.Connected == true)
            {
                await SendMessage("DISCONNECT|Client disconnect");
                tcpClient.Close();
                isConnected = false;
                UpdateUI();
                LogMessage("Disconnected from server");
            }
        }

        private async Task SendMessage(string message)
        {
            if (stream != null)
            {
                var data = Encoding.UTF8.GetBytes(message + "\n");
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
        }

        private async Task HandleServerMessages()
        {
            var buffer = new byte[4096];
            
            while (isConnected && tcpClient?.Connected == true)
            {
                try
                {
                    var bytesRead = await stream!.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                    
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessServerMessage(message.Trim());
                }
                catch (Exception ex)
                {
                    if (isConnected)
                    {
                        this.Invoke(new Action(() => {
                            LogMessage($"Server message error: {ex.Message}");
                        }));
                    }
                    break;
                }
            }
        }

        private void ProcessServerMessage(string message)
        {
            var parts = message.Split('|');
            if (parts.Length < 1) return;

            string command = parts[0];
            string data = parts.Length > 1 ? parts[1] : "";

            this.Invoke(new Action(() => {
                switch (command)
                {
                    case "NAME_SET":
                        LogMessage("Name set successfully");
                        break;
                    case "GAME_JOINED":
                        LogMessage("Joined game successfully");
                        break;
                    case "GAME_LEFT":
                        LogMessage("Left game successfully");
                        break;
                    case "BROADCAST":
                        LogMessage($"Server: {data}");
                        break;
                    case "GAME_UPDATE":
                        if (parts.Length >= 3)
                        {
                            string updateType = parts[1];
                            string updateData = parts[2];
                            LogMessage($"Game Update [{updateType}]: {updateData}");
                        }
                        break;
                    case "CHAT":
                        if (parts.Length >= 3)
                        {
                            string player = parts[1];
                            string chatMessage = parts[2];
                            LogMessage($"[{player}]: {chatMessage}");
                        }
                        break;
                    case "PONG":
                        LogMessage($"Pong received: {data}");
                        break;
                    case "HEARTBEAT":
                    case "HEARTBEAT_ACK":
                        // Silently handle heartbeat
                        break;
                    case "DISCONNECT":
                        LogMessage($"Disconnected: {data}");
                        isConnected = false;
                        UpdateUI();
                        break;
                    default:
                        LogMessage($"Server: {message}");
                        break;
                }
            }));
        }

        private void UpdateServersList()
        {
            serversListBox.Items.Clear();
            foreach (var server in discoveredServers)
            {
                var ping = "---";
                var quality = "---";
                serversListBox.Items.Add($"{server.Name} | {server.PlayerCount} players | v{server.Version} | {server.EndPoint.Address} | Ping: {ping}ms | {quality}");
            }
        }

        private void UpdateUI()
        {
            if (isConnected)
            {
                statusLabel.Text = "Status: ✅ Connected";
                statusLabel.ForeColor = Color.LightGreen;
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                messageTextBox.Enabled = true;
                sendButton.Enabled = true;
                discoverButton.Enabled = false;
                serverInfoLabel.Text = "🔗 LAN Client - Connected to server";
            }
            else
            {
                statusLabel.Text = "Status: ⚠️ Disconnected";
                statusLabel.ForeColor = Color.Orange;
                connectButton.Enabled = discoveredServers.Count > 0 && serversListBox.SelectedIndex >= 0;
                disconnectButton.Enabled = false;
                messageTextBox.Enabled = false;
                sendButton.Enabled = false;
                discoverButton.Enabled = true;
                serverInfoLabel.Text = "🔗 LAN Client - Ready to discover servers";
            }
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {message}";
            
            logTextBox.AppendText(logEntry + Environment.NewLine);
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            autoDiscoverTimer?.Dispose();
            udpClient?.Close();
            tcpClient?.Close();
            base.OnFormClosing(e);
        }
    }
}
