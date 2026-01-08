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

namespace CastleStoryLANServer
{
    public partial class LANServerGUI : Form
    {
        private TcpListener? tcpListener;
        private UdpClient? udpClient;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private readonly object clientsLock = new object();
        private bool isRunning = false;
        private string serverName = "Castle Story LAN Server";
        private string serverVersion = "1.0.0";
        private int port = 7777;
        private int discoveryPort = 7778;

        // UI Controls
        private Label? statusLabel;
        private ListBox? clientsListBox;
        private TextBox? logTextBox;
        private Button? startStopButton;
        private Button? broadcastButton;
        private TextBox? broadcastTextBox;
        private Label? serverInfoLabel;
        private Label? clientCountLabel;

        public LANServerGUI()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Castle Story LAN Server";
            this.Size = new Size(800, 650);
            this.MinimumSize = new Size(700, 550);
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

            // Server info label
            serverInfoLabel = new Label
            {
                Text = "🏰 Castle Story LAN Server",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.LightBlue,
                Size = new Size(400, 30),
                Location = new Point(15, 15)
            };
            headerPanel.Controls.Add(serverInfoLabel);

            // Status label
            statusLabel = new Label
            {
                Text = "Status: Stopped",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Orange,
                Size = new Size(200, 25),
                Location = new Point(15, 45)
            };
            headerPanel.Controls.Add(statusLabel);

            // Port configuration
            var portLabel = new Label
            {
                Text = "Port:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Size = new Size(40, 20),
                Location = new Point(450, 20)
            };
            headerPanel.Controls.Add(portLabel);

            var portTextBox = new TextBox
            {
                Text = port.ToString(),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Size = new Size(60, 25),
                Location = new Point(495, 18),
                BorderStyle = BorderStyle.FixedSingle
            };
            portTextBox.TextChanged += (s, e) => {
                if (int.TryParse(portTextBox.Text, out int newPort) && newPort > 0 && newPort < 65536)
                {
                    port = newPort;
                }
            };
            headerPanel.Controls.Add(portTextBox);

            // Start/Stop button
            startStopButton = new Button
            {
                Text = "▶️ Start Server",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(140, 40),
                Location = new Point(580, 15)
            };
            startStopButton.Click += StartStopButton_Click;
            headerPanel.Controls.Add(startStopButton);

            // Clients panel
            var clientsPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(360, 220),
                Location = new Point(10, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };
            this.Controls.Add(clientsPanel);

            var clientsLabel = new Label
            {
                Text = "👥 Connected Clients:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(200, 25),
                Location = new Point(10, 10)
            };
            clientsPanel.Controls.Add(clientsLabel);

            clientCountLabel = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.LightGreen,
                Size = new Size(50, 25),
                Location = new Point(210, 10)
            };
            clientsPanel.Controls.Add(clientCountLabel);

            clientsListBox = new ListBox
            {
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                Size = new Size(340, 160),
                Location = new Point(10, 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            clientsListBox.SelectedIndexChanged += (s, e) => {
                if (clientsListBox.SelectedIndex >= 0)
                {
                    // Show client details
                }
            };
            clientsPanel.Controls.Add(clientsListBox);

            // Broadcast panel
            var broadcastPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(380, 220),
                Location = new Point(380, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(broadcastPanel);

            var broadcastLabel = new Label
            {
                Text = "📢 Broadcast Message:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(200, 25),
                Location = new Point(10, 10)
            };
            broadcastPanel.Controls.Add(broadcastLabel);

            broadcastTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Size = new Size(360, 100),
                Location = new Point(10, 40),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };
            broadcastTextBox.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter && e.Control)
                {
                    BroadcastButton_Click(s, e);
                }
            };
            broadcastPanel.Controls.Add(broadcastTextBox);

            broadcastButton = new Button
            {
                Text = "📢 Broadcast to All",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(150, 35),
                Location = new Point(10, 150)
            };
            broadcastButton.Click += BroadcastButton_Click;
            broadcastPanel.Controls.Add(broadcastButton);

            // Statistics panel
            var statsPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(380, 100),
                Location = new Point(380, 330),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(statsPanel);

            var statsLabel = new Label
            {
                Text = "📊 Server Statistics:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(200, 25),
                Location = new Point(10, 10)
            };
            statsPanel.Controls.Add(statsLabel);

            var uptimeLabel = new Label
            {
                Text = "Uptime: 00:00:00",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                Size = new Size(180, 20),
                Location = new Point(10, 40)
            };
            statsPanel.Controls.Add(uptimeLabel);

            var messagesLabel = new Label
            {
                Text = "Messages Sent: 0",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                Size = new Size(180, 20),
                Location = new Point(10, 65)
            };
            statsPanel.Controls.Add(messagesLabel);

            // Log panel
            var logPanel = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                Size = new Size(this.Width - 20, 180),
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
                Text = "📋 Server Log:",
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
                Size = new Size(logPanel.Width - 20, 140),
                Location = new Point(10, 40),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            logPanel.Controls.Add(logTextBox);
        }

        private async void StartStopButton_Click(object? sender, EventArgs e)
        {
            if (!isRunning)
            {
                await StartServer();
            }
            else
            {
                await StopServer();
            }
        }

        private async void BroadcastButton_Click(object? sender, EventArgs e)
        {
            if (isRunning && !string.IsNullOrEmpty(broadcastTextBox.Text))
            {
                await BroadcastMessage(broadcastTextBox.Text);
                broadcastTextBox.Clear();
            }
        }

        private async Task StartServer()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                udpClient = new UdpClient(discoveryPort);
                
                tcpListener.Start();
                udpClient.BeginReceive(OnUdpDataReceived, null);
                
                isRunning = true;
                
                UpdateUI();
                LogMessage($"Server started on port {port}");
                LogMessage($"Discovery server on port {discoveryPort}");
                
                // Start accepting connections
                Task.Run(AcceptConnections);
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to start server: {ex.Message}");
            }
        }

        private async Task StopServer()
        {
            try
            {
                isRunning = false;
                
                // Disconnect all clients
                List<ClientHandler> clientsToDisconnect;
                lock (clientsLock)
                {
                    clientsToDisconnect = clients.ToList();
                    clients.Clear();
                }
                foreach (var client in clientsToDisconnect)
                {
                    await client.Disconnect("Server shutdown");
                }
                
                tcpListener?.Stop();
                udpClient?.Close();
                
                UpdateUI();
                LogMessage("Server stopped");
            }
            catch (Exception ex)
            {
                LogMessage($"Error stopping server: {ex.Message}");
            }
        }

        private async Task AcceptConnections()
        {
            while (isRunning)
            {
                try
                {
                    var tcpClient = await tcpListener!.AcceptTcpClientAsync();
                    var clientHandler = new ClientHandler(tcpClient, null);
                    
                    lock (clientsLock)
                    {
                        clients.Add(clientHandler);
                    }
                    
                    // Set up client disconnection handling
                    clientHandler.OnDisconnected += async (sender, e) => {
                        this.Invoke(new Action(() => {
                            lock (clientsLock)
                            {
                                if (clients.Contains(clientHandler))
                                {
                                    clients.Remove(clientHandler);
                                }
                            }
                            LogMessage($"Client {clientHandler.ClientName} disconnected");
                            UpdateClientsList();
                        }));
                    };
                    
                    // Set up client name change handling
                    clientHandler.OnNameChanged += (sender, e) => {
                        this.Invoke(new Action(() => {
                            LogMessage($"Client {clientHandler.EndPoint} set name to: {clientHandler.ClientName}");
                            UpdateClientsList();
                        }));
                    };
                    
                    // Set up broadcast handling for client messages
                    clientHandler.OnBroadcastNeeded += async (sender, message) => {
                        var broadcastMessage = $"GAME_UPDATE|{message}";
                        List<ClientHandler> clientsCopy;
                        lock (clientsLock)
                        {
                            clientsCopy = clients.Where(c => c != clientHandler).ToList();
                        }
                        
                        var clientsToRemove = new List<ClientHandler>();
                        foreach (var client in clientsCopy)
                        {
                            try
                            {
                                await client.SendMessage(broadcastMessage);
                            }
                            catch
                            {
                                // Mark disconnected clients for removal
                                clientsToRemove.Add(client);
                            }
                        }
                        
                        // Remove disconnected clients
                        if (clientsToRemove.Count > 0)
                        {
                            this.Invoke(new Action(() => {
                                lock (clientsLock)
                                {
                                    foreach (var client in clientsToRemove)
                                    {
                                        clients.Remove(client);
                                    }
                                }
                                UpdateClientsList();
                            }));
                        }
                    };
                    
                    this.Invoke(new Action(() => {
                        LogMessage($"New connection from {tcpClient.Client.RemoteEndPoint}");
                        UpdateClientsList();
                    }));
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        LogMessage($"Error accepting connection: {ex.Message}");
                }
            }
        }

        private void OnUdpDataReceived(IAsyncResult result)
        {
            try
            {
                if (udpClient == null) return;
                
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var data = udpClient.EndReceive(result, ref remoteEndPoint);
                var message = Encoding.UTF8.GetString(data);
                
                if (message == "DISCOVER_SERVERS")
                {
                    int clientCount;
                    lock (clientsLock)
                    {
                        clientCount = clients.Count;
                    }
                    
                    var response = $"SERVER_INFO|{serverName}|{port}|{clientCount}|{serverVersion}";
                    var responseData = Encoding.UTF8.GetBytes(response);
                    udpClient.Send(responseData, responseData.Length, remoteEndPoint);
                    
                    this.Invoke(new Action(() => {
                        LogMessage($"Discovery request from {remoteEndPoint}");
                    }));
                }
                
                if (isRunning)
                    udpClient.BeginReceive(OnUdpDataReceived, null);
            }
            catch (Exception ex)
            {
                if (isRunning)
                    LogMessage($"UDP error: {ex.Message}");
            }
        }

        public async Task BroadcastMessage(string message)
        {
            var fullMessage = $"BROADCAST|{message}";
            
            List<ClientHandler> clientsCopy;
            lock (clientsLock)
            {
                clientsCopy = clients.ToList();
            }
            
            var clientsToRemove = new List<ClientHandler>();
            foreach (var client in clientsCopy)
            {
                try
                {
                    if (client != null)
                    {
                        await client.SendMessage(fullMessage);
                    }
                }
                catch
                {
                    // Mark disconnected clients for removal
                    clientsToRemove.Add(client);
                }
            }
            
            // Remove disconnected clients
            if (clientsToRemove.Count > 0)
            {
                lock (clientsLock)
                {
                    foreach (var client in clientsToRemove)
                    {
                        clients.Remove(client);
                    }
                }
            }
            
            this.Invoke(new Action(() => {
                UpdateClientsList();
                LogMessage($"Broadcast: {message}");
            }));
        }

        public void RemoveClient(ClientHandler client)
        {
            int count;
            lock (clientsLock)
            {
                clients.Remove(client);
                count = clients.Count;
            }
            this.Invoke(new Action(() => {
                UpdateClientsList();
                LogMessage($"Client disconnected. Total clients: {count}");
            }));
        }

        private void UpdateUI()
        {
            if (isRunning)
            {
                statusLabel.Text = "Status: Running";
                statusLabel.ForeColor = Color.LightGreen;
                startStopButton.Text = "⏹️ Stop Server";
                startStopButton.BackColor = Color.FromArgb(200, 0, 0);
                serverInfoLabel.Text = $"Server: {serverName} v{serverVersion} - Port {port}";
            }
            else
            {
                statusLabel.Text = "Status: Stopped";
                statusLabel.ForeColor = Color.Orange;
                startStopButton.Text = "▶️ Start Server";
                startStopButton.BackColor = Color.FromArgb(0, 120, 0);
                serverInfoLabel.Text = $"Server: {serverName} v{serverVersion}";
            }
        }

        private void UpdateClientsList()
        {
            clientsListBox.Items.Clear();
            List<ClientHandler> clientsCopy;
            int count;
            lock (clientsLock)
            {
                clientsCopy = clients.ToList();
                count = clients.Count;
            }
            
            foreach (var client in clientsCopy)
            {
                var timeConnected = DateTime.Now - client.ConnectedAt;
                var timeStr = timeConnected.TotalMinutes < 1 
                    ? $"{(int)timeConnected.TotalSeconds}s" 
                    : $"{(int)timeConnected.TotalMinutes}m";
                clientsListBox.Items.Add($"{client.ClientName} | {client.Status} | {timeStr} | {client.EndPoint}");
            }

            // Update client count
            if (clientCountLabel != null)
            {
                clientCountLabel.Text = count.ToString();
                clientCountLabel.ForeColor = count > 0 ? Color.LightGreen : Color.Orange;
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
            if (isRunning)
            {
                StopServer();
            }
            base.OnFormClosing(e);
        }
    }
}
