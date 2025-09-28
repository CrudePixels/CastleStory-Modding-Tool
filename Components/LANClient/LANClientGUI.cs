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
            this.Text = "LAN Client";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            // Client info label
            serverInfoLabel = new Label
            {
                Text = "Castle Story LAN Client",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.LightBlue,
                Size = new Size(500, 30),
                Location = new Point(20, 20)
            };
            this.Controls.Add(serverInfoLabel);

            // Status label
            statusLabel = new Label
            {
                Text = "Status: Disconnected",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Orange,
                Size = new Size(200, 25),
                Location = new Point(20, 60)
            };
            this.Controls.Add(statusLabel);

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

            // Servers section
            var serversLabel = new Label
            {
                Text = "Discovered Servers:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(150, 25),
                Location = new Point(20, 150)
            };
            this.Controls.Add(serversLabel);

            serversListBox = new ListBox
            {
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                Size = new Size(400, 120),
                Location = new Point(20, 180)
            };
            serversListBox.SelectedIndexChanged += ServersListBox_SelectedIndexChanged;
            this.Controls.Add(serversListBox);

            // Server buttons
            discoverButton = new Button
            {
                Text = "🔍 Discover Servers",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 35),
                Location = new Point(20, 310)
            };
            discoverButton.Click += DiscoverButton_Click;
            this.Controls.Add(discoverButton);

            connectButton = new Button
            {
                Text = "🔗 Connect",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 35),
                Location = new Point(170, 310),
                Enabled = false
            };
            connectButton.Click += ConnectButton_Click;
            this.Controls.Add(connectButton);

            disconnectButton = new Button
            {
                Text = "❌ Disconnect",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 35),
                Location = new Point(280, 310),
                Enabled = false
            };
            disconnectButton.Click += DisconnectButton_Click;
            this.Controls.Add(disconnectButton);

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

            // Log section
            var logLabel = new Label
            {
                Text = "Client Log:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(100, 25),
                Location = new Point(20, 430)
            };
            this.Controls.Add(logLabel);

            logTextBox = new TextBox
            {
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                Size = new Size(650, 120),
                Location = new Point(20, 460),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };
            this.Controls.Add(logTextBox);

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
                var data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        private async Task HandleServerMessages()
        {
            var buffer = new byte[1024];
            
            while (isConnected && tcpClient?.Connected == true)
            {
                try
                {
                    var bytesRead = await stream!.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        this.Invoke(new Action(() => {
                            LogMessage($"Server: {message}");
                        }));
                    }
                }
                catch (Exception ex)
                {
                    if (isConnected)
                    {
                        this.Invoke(new Action(() => {
                            LogMessage($"Server message error: {ex.Message}");
                        }));
                    }
                }
            }
        }

        private void UpdateServersList()
        {
            serversListBox.Items.Clear();
            foreach (var server in discoveredServers)
            {
                serversListBox.Items.Add($"{server.Name} - {server.PlayerCount} players - v{server.Version}");
            }
        }

        private void UpdateUI()
        {
            if (isConnected)
            {
                statusLabel.Text = "Status: Connected";
                statusLabel.ForeColor = Color.LightGreen;
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                messageTextBox.Enabled = true;
                sendButton.Enabled = true;
                serverInfoLabel.Text = "LAN Client - Connected to server";
            }
            else
            {
                statusLabel.Text = "Status: Disconnected";
                statusLabel.ForeColor = Color.Orange;
                connectButton.Enabled = discoveredServers.Count > 0 && serversListBox.SelectedIndex >= 0;
                disconnectButton.Enabled = false;
                messageTextBox.Enabled = false;
                sendButton.Enabled = false;
                serverInfoLabel.Text = "LAN Client - Ready to discover servers";
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
