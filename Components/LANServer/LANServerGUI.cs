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

        public LANServerGUI()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "LAN Server";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            // Server info label
            serverInfoLabel = new Label
            {
                Text = "Castle Story LAN Server",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.LightBlue,
                Size = new Size(400, 30),
                Location = new Point(20, 20)
            };
            this.Controls.Add(serverInfoLabel);

            // Status label
            statusLabel = new Label
            {
                Text = "Status: Stopped",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Orange,
                Size = new Size(200, 25),
                Location = new Point(20, 60)
            };
            this.Controls.Add(statusLabel);

            // Start/Stop button
            startStopButton = new Button
            {
                Text = "▶️ Start Server",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 35),
                Location = new Point(20, 100)
            };
            startStopButton.Click += StartStopButton_Click;
            this.Controls.Add(startStopButton);

            // Clients list
            var clientsLabel = new Label
            {
                Text = "Connected Clients:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(150, 25),
                Location = new Point(20, 150)
            };
            this.Controls.Add(clientsLabel);

            clientsListBox = new ListBox
            {
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                Size = new Size(250, 150),
                Location = new Point(20, 180)
            };
            this.Controls.Add(clientsListBox);

            // Broadcast section
            var broadcastLabel = new Label
            {
                Text = "Broadcast Message:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(150, 25),
                Location = new Point(300, 150)
            };
            this.Controls.Add(broadcastLabel);

            broadcastTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Size = new Size(250, 25),
                Location = new Point(300, 180)
            };
            this.Controls.Add(broadcastTextBox);

            broadcastButton = new Button
            {
                Text = "📢 Broadcast",
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 30),
                Location = new Point(300, 210)
            };
            broadcastButton.Click += BroadcastButton_Click;
            this.Controls.Add(broadcastButton);

            // Log section
            var logLabel = new Label
            {
                Text = "Server Log:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(100, 25),
                Location = new Point(20, 350)
            };
            this.Controls.Add(logLabel);

            logTextBox = new TextBox
            {
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                Size = new Size(530, 100),
                Location = new Point(20, 380),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };
            this.Controls.Add(logTextBox);
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
                foreach (var client in clients.ToList())
                {
                    client.Disconnect("Server shutdown");
                }
                clients.Clear();
                
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
                    clients.Add(clientHandler);
                    
                    // Set up client disconnection handling
                    clientHandler.OnDisconnected += (sender, e) => {
                        this.Invoke(new Action(() => {
                            clients.Remove(clientHandler);
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
                    var response = $"SERVER_INFO|{serverName}|{port}|{clients.Count}|{serverVersion}";
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
            var data = Encoding.UTF8.GetBytes(fullMessage);
            
            foreach (var client in clients.ToList())
            {
                try
                {
                    await client.SendMessage(fullMessage);
                }
                catch
                {
                    // Remove disconnected clients
                    clients.Remove(client);
                }
            }
            
            this.Invoke(new Action(() => {
                UpdateClientsList();
                LogMessage($"Broadcast: {message}");
            }));
        }

        public void RemoveClient(ClientHandler client)
        {
            clients.Remove(client);
            this.Invoke(new Action(() => {
                UpdateClientsList();
                LogMessage($"Client disconnected. Total clients: {clients.Count}");
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
            foreach (var client in clients)
            {
                clientsListBox.Items.Add($"{client.ClientName} ({client.EndPoint})");
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
