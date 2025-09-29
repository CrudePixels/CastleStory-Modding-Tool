using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Timers;

namespace CastleStoryLANServer
{
    /// <summary>
    /// Enhanced LAN Server with better configuration, client management, and features
    /// </summary>
    public class EnhancedServer
    {
        private TcpListener? tcpListener;
        private UdpClient? udpClient;
        private List<EnhancedClientHandler> clients = new List<EnhancedClientHandler>();
        private bool isRunning = false;
        private ServerConfig config;
        private System.Timers.Timer? heartbeatTimer;
        private System.Timers.Timer? discoveryTimer;
        private System.Timers.Timer? statsTimer;
        private ServerStats stats = new ServerStats();
        private readonly object clientsLock = new object();

        public EnhancedServer(ServerConfig? config = null)
        {
            this.config = config ?? new ServerConfig();
        }

        public void Start()
        {
            try
            {
                Console.WriteLine("=== Enhanced Castle Story LAN Server ===");
                Console.WriteLine($"Starting server with configuration:");
                Console.WriteLine($"  Port: {config.Port}");
                Console.WriteLine($"  Max Players: {config.MaxPlayers}");
                Console.WriteLine($"  Server Name: {config.ServerName}");
                Console.WriteLine($"  Password: {(string.IsNullOrEmpty(config.Password) ? "None" : "Set")}");
                Console.WriteLine($"  Auto Discovery: {config.EnableAutoDiscovery}");
                Console.WriteLine($"  Heartbeat Interval: {config.HeartbeatInterval}ms");
                Console.WriteLine();

                // Start TCP server for game connections
                tcpListener = new TcpListener(IPAddress.Any, config.Port);
                tcpListener.Start();
                isRunning = true;

                // Start UDP server for discovery
                udpClient = new UdpClient(config.Port + 1);

                Console.WriteLine($"✅ LAN Server started on port {config.Port}");
                Console.WriteLine($"✅ Discovery server on port {config.Port + 1}");
                Console.WriteLine($"✅ Server Name: {config.ServerName}");
                Console.WriteLine($"✅ Version: {config.ServerVersion}");
                Console.WriteLine($"✅ Max Players: {config.MaxPlayers}");
                Console.WriteLine();

                // Start background tasks
                Task.Run(AcceptConnections);
                Task.Run(HandleDiscovery);
                Task.Run(HandleServerCommands);
                
                // Start timers
                StartTimers();

                // Keep server running
                while (isRunning)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Server error: {ex.Message}");
            }
        }

        private void StartTimers()
        {
            // Heartbeat timer
            heartbeatTimer = new System.Timers.Timer(config.HeartbeatInterval);
            heartbeatTimer.Elapsed += (s, e) => SendHeartbeat();
            heartbeatTimer.Start();

            // Discovery timer (if auto-discovery is enabled)
            if (config.EnableAutoDiscovery)
            {
                discoveryTimer = new System.Timers.Timer(5000); // Every 5 seconds
                discoveryTimer.Elapsed += (s, e) => BroadcastServerInfo();
                discoveryTimer.Start();
            }

            // Stats timer
            statsTimer = new System.Timers.Timer(30000); // Every 30 seconds
            statsTimer.Elapsed += (s, e) => UpdateStats();
            statsTimer.Start();
        }

        private async Task AcceptConnections()
        {
            while (isRunning)
            {
                try
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync();
                    
                    // Check if server is full
                    lock (clientsLock)
                    {
                        if (clients.Count >= config.MaxPlayers)
                        {
                            Console.WriteLine($"❌ Server full! Rejecting connection from {tcpClient.Client.RemoteEndPoint}");
                            tcpClient.Close();
                            continue;
                        }
                    }

                    var clientHandler = new EnhancedClientHandler(tcpClient, this, config);
                    clientHandler.OnDisconnected += OnClientDisconnected;
                    clientHandler.OnNameChanged += OnClientNameChanged;
                    clientHandler.OnMessageReceived += OnClientMessageReceived;
                    
                    lock (clientsLock)
                    {
                        clients.Add(clientHandler);
                    }
                    
                    Console.WriteLine($"✅ New connection from {tcpClient.Client.RemoteEndPoint}");
                    Console.WriteLine($"📊 Total clients: {GetClientCount()}");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Console.WriteLine($"❌ Connection error: {ex.Message}");
                }
            }
        }

        private async Task HandleDiscovery()
        {
            while (isRunning)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    var clientEndPoint = result.RemoteEndPoint;

                    if (message == "DISCOVER_SERVERS")
                    {
                        var serverInfo = new ServerInfo
                        {
                            Name = config.ServerName,
                            Port = config.Port,
                            PlayerCount = GetClientCount(),
                            MaxPlayers = config.MaxPlayers,
                            Version = config.ServerVersion,
                            HasPassword = !string.IsNullOrEmpty(config.Password),
                            Ping = 0 // Could implement actual ping measurement
                        };

                        var response = JsonSerializer.Serialize(serverInfo);
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        await udpClient.SendAsync(responseBytes, responseBytes.Length, clientEndPoint);
                        
                        Console.WriteLine($"📡 Sent server info to {clientEndPoint}");
                    }
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Console.WriteLine($"❌ Discovery error: {ex.Message}");
                }
            }
        }

        private async Task HandleServerCommands()
        {
            while (isRunning)
            {
                var input = Console.ReadLine();
                if (input != null)
                {
                    await ProcessCommand(input.Trim());
                }
            }
        }

        private async Task ProcessCommand(string command)
        {
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "help":
                    ShowHelp();
                    break;
                case "list":
                    ListClients();
                    break;
                case "kick":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int kickId))
                    {
                        await KickClient(kickId);
                    }
                    else
                    {
                        Console.WriteLine("Usage: kick <client_id>");
                    }
                    break;
                case "broadcast":
                    if (parts.Length > 1)
                    {
                        var message = string.Join(" ", parts.Skip(1));
                        await BroadcastMessage(message);
                    }
                    else
                    {
                        Console.WriteLine("Usage: broadcast <message>");
                    }
                    break;
                case "status":
                    ShowStatus();
                    break;
                case "config":
                    ShowConfig();
                    break;
                case "stats":
                    ShowStats();
                    break;
                case "restart":
                    await RestartServer();
                    break;
                case "stop":
                    Stop();
                    break;
                case "set":
                    if (parts.Length > 2)
                    {
                        await SetConfig(parts[1], string.Join(" ", parts.Skip(2)));
                    }
                    else
                    {
                        Console.WriteLine("Usage: set <setting> <value>");
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown command: {cmd}. Type 'help' for available commands.");
                    break;
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("\n=== Available Commands ===");
            Console.WriteLine("help          - Show this help message");
            Console.WriteLine("list          - List connected clients");
            Console.WriteLine("kick <id>     - Kick a client by ID");
            Console.WriteLine("broadcast <msg> - Broadcast message to all clients");
            Console.WriteLine("status        - Show server status");
            Console.WriteLine("config        - Show server configuration");
            Console.WriteLine("stats         - Show server statistics");
            Console.WriteLine("restart       - Restart the server");
            Console.WriteLine("stop          - Stop the server");
            Console.WriteLine("set <key> <value> - Set configuration value");
            Console.WriteLine();
        }

        private void ListClients()
        {
            lock (clientsLock)
            {
                Console.WriteLine($"\n=== Connected Clients ({clients.Count}) ===");
                for (int i = 0; i < clients.Count; i++)
                {
                    var client = clients[i];
                    Console.WriteLine($"{i + 1}. {client.ClientName} ({client.EndPoint}) - {client.Status} - Connected: {client.ConnectedAt:HH:mm:ss}");
                }
                Console.WriteLine();
            }
        }

        private async Task KickClient(int clientId)
        {
            lock (clientsLock)
            {
                if (clientId > 0 && clientId <= clients.Count)
                {
                    var client = clients[clientId - 1];
                    Console.WriteLine($"Kicking client: {client.ClientName} ({client.EndPoint})");
                    client.Disconnect("Kicked by server administrator");
                    clients.RemoveAt(clientId - 1);
                }
                else
                {
                    Console.WriteLine("Invalid client ID");
                }
            }
        }

        private async Task BroadcastMessage(string message)
        {
            lock (clientsLock)
            {
                Console.WriteLine($"Broadcasting: {message}");
                foreach (var client in clients)
                {
                    client.SendMessage($"BROADCAST|{message}");
                }
            }
        }

        private void ShowStatus()
        {
            Console.WriteLine($"\n=== Server Status ===");
            Console.WriteLine($"Running: {isRunning}");
            Console.WriteLine($"Port: {config.Port}");
            Console.WriteLine($"Clients: {GetClientCount()}/{config.MaxPlayers}");
            Console.WriteLine($"Uptime: {DateTime.Now - stats.StartTime:hh\\:mm\\:ss}");
            Console.WriteLine($"Total Connections: {stats.TotalConnections}");
            Console.WriteLine($"Messages Sent: {stats.MessagesSent}");
            Console.WriteLine($"Messages Received: {stats.MessagesReceived}");
            Console.WriteLine();
        }

        private void ShowConfig()
        {
            Console.WriteLine($"\n=== Server Configuration ===");
            Console.WriteLine($"Server Name: {config.ServerName}");
            Console.WriteLine($"Port: {config.Port}");
            Console.WriteLine($"Max Players: {config.MaxPlayers}");
            Console.WriteLine($"Password: {(string.IsNullOrEmpty(config.Password) ? "None" : "Set")}");
            Console.WriteLine($"Heartbeat Interval: {config.HeartbeatInterval}ms");
            Console.WriteLine($"Enable Auto Discovery: {config.EnableAutoDiscovery}");
            Console.WriteLine($"Enable Logging: {config.EnableLogging}");
            Console.WriteLine($"Log Level: {config.LogLevel}");
            Console.WriteLine();
        }

        private void ShowStats()
        {
            Console.WriteLine($"\n=== Server Statistics ===");
            Console.WriteLine($"Uptime: {DateTime.Now - stats.StartTime:hh\\:mm\\:ss}");
            Console.WriteLine($"Total Connections: {stats.TotalConnections}");
            Console.WriteLine($"Current Connections: {GetClientCount()}");
            Console.WriteLine($"Messages Sent: {stats.MessagesSent}");
            Console.WriteLine($"Messages Received: {stats.MessagesReceived}");
            Console.WriteLine($"Bytes Sent: {stats.BytesSent:N0}");
            Console.WriteLine($"Bytes Received: {stats.BytesReceived:N0}");
            Console.WriteLine($"Peak Connections: {stats.PeakConnections}");
            Console.WriteLine();
        }

        private async Task RestartServer()
        {
            Console.WriteLine("Restarting server...");
            Stop();
            await Task.Delay(2000);
            Start();
        }

        public void Stop()
        {
            Console.WriteLine("Stopping server...");
            isRunning = false;
            
            // Stop timers
            heartbeatTimer?.Stop();
            discoveryTimer?.Stop();
            statsTimer?.Stop();
            
            // Disconnect all clients
            lock (clientsLock)
            {
                foreach (var client in clients)
                {
                    client.Disconnect("Server shutting down");
                }
                clients.Clear();
            }
            
            // Close listeners
            tcpListener?.Stop();
            udpClient?.Close();
            
            Console.WriteLine("Server stopped.");
        }

        private async Task SetConfig(string key, string value)
        {
            switch (key.ToLower())
            {
                case "name":
                    config.ServerName = value;
                    Console.WriteLine($"Server name set to: {value}");
                    break;
                case "maxplayers":
                    if (int.TryParse(value, out int maxPlayers) && maxPlayers > 0)
                    {
                        config.MaxPlayers = maxPlayers;
                        Console.WriteLine($"Max players set to: {maxPlayers}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid max players value");
                    }
                    break;
                case "password":
                    config.Password = value;
                    Console.WriteLine($"Password {(string.IsNullOrEmpty(value) ? "removed" : "set")}");
                    break;
                default:
                    Console.WriteLine($"Unknown setting: {key}");
                    break;
            }
        }

        private void SendHeartbeat()
        {
            lock (clientsLock)
            {
                foreach (var client in clients)
                {
                    client.SendMessage("HEARTBEAT");
                }
            }
        }

        private void BroadcastServerInfo()
        {
            // This would broadcast server info to the network
            // Implementation depends on specific discovery protocol
        }

        private void UpdateStats()
        {
            stats.PeakConnections = Math.Max(stats.PeakConnections, GetClientCount());
        }

        public int GetClientCount()
        {
            lock (clientsLock)
            {
                return clients.Count;
            }
        }

        public DateTime GetStartTime()
        {
            return stats.StartTime;
        }

        // Event handlers
        private void OnClientDisconnected(object? sender, EventArgs e)
        {
            if (sender is EnhancedClientHandler client)
            {
                lock (clientsLock)
                {
                    clients.Remove(client);
                }
                Console.WriteLine($"❌ Client disconnected: {client.ClientName} ({client.EndPoint})");
                Console.WriteLine($"📊 Total clients: {GetClientCount()}");
            }
        }

        private void OnClientNameChanged(object? sender, EventArgs e)
        {
            if (sender is EnhancedClientHandler client)
            {
                Console.WriteLine($"📝 Client name changed: {client.ClientName} ({client.EndPoint})");
            }
        }

        private void OnClientMessageReceived(object? sender, string message)
        {
            if (sender is EnhancedClientHandler client)
            {
                stats.MessagesReceived++;
                stats.BytesReceived += message.Length;
                
                // Process different message types
                var parts = message.Split('|');
                if (parts.Length > 0)
                {
                    switch (parts[0].ToUpper())
                    {
                        case "CHAT":
                            if (parts.Length > 1)
                            {
                                var chatMessage = parts[1];
                                Console.WriteLine($"💬 {client.ClientName}: {chatMessage}");
                                // Broadcast chat to all clients
                                BroadcastMessage($"CHAT|{client.ClientName}|{chatMessage}");
                            }
                            break;
                        case "PING":
                            client.SendMessage("PONG");
                            break;
                        default:
                            Console.WriteLine($"📨 Message from {client.ClientName}: {message}");
                            break;
                    }
                }
            }
        }
    }

    public class ServerConfig
    {
        public string ServerName { get; set; } = "Castle Story LAN Server";
        public int Port { get; set; } = 7777;
        public int MaxPlayers { get; set; } = 32;
        public string Password { get; set; } = "";
        public int HeartbeatInterval { get; set; } = 5000;
        public bool EnableAutoDiscovery { get; set; } = true;
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info";
        public string ServerVersion { get; set; } = "1.0.0";
    }

    public class ServerStats
    {
        public DateTime StartTime { get; set; } = DateTime.Now;
        public int TotalConnections { get; set; } = 0;
        public int MessagesSent { get; set; } = 0;
        public int MessagesReceived { get; set; } = 0;
        public long BytesSent { get; set; } = 0;
        public long BytesReceived { get; set; } = 0;
        public int PeakConnections { get; set; } = 0;
    }

    public class ServerInfo
    {
        public string Name { get; set; } = "";
        public int Port { get; set; } = 7777;
        public int PlayerCount { get; set; } = 0;
        public int MaxPlayers { get; set; } = 32;
        public string Version { get; set; } = "1.0.0";
        public bool HasPassword { get; set; } = false;
        public int Ping { get; set; } = 0;
    }
}
