using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CastleStoryMultiplayerServer
{
    class Program
    {
        private static TcpListener? listener;
        public static List<ClientHandler> clients = new List<ClientHandler>();
        private static bool isRunning = false;
        private static int port = 7777;
        private static string mapsDirectory = "Maps";
        private static string gamemodesDirectory = "Gamemodes";

        static void Main(string[] args)
        {
            Console.WriteLine("=== Castle Story Multiplayer Server ===");
            Console.WriteLine("Enhanced multiplayer server with map/gamemode sync");
            Console.WriteLine();

            // Parse command line arguments
            ParseArguments(args);

            // Create directories
            Directory.CreateDirectory(mapsDirectory);
            Directory.CreateDirectory(gamemodesDirectory);

            // Start server
            StartServer();

            // Keep server running
            Console.WriteLine("Server is running. Press 'q' to quit, 's' for status, 'c' for clients");
            while (isRunning)
            {
                var key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case 'q':
                    case 'Q':
                        StopServer();
                        return;
                    case 's':
                    case 'S':
                        ShowStatus();
                        break;
                    case 'c':
                    case 'C':
                        ShowClients();
                        break;
                }
            }
        }

        private static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-port":
                    case "-p":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int portValue))
                        {
                            port = portValue;
                            i++;
                        }
                        break;
                    case "-maps":
                    case "-m":
                        if (i + 1 < args.Length)
                        {
                            mapsDirectory = args[i + 1];
                            i++;
                        }
                        break;
                    case "-help":
                    case "-h":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: CastleStoryMultiplayerServer.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -port <number>    Set server port (default: 7777)");
            Console.WriteLine("  -maps <path>      Set maps directory (default: Maps)");
            Console.WriteLine("  -help             Show this help message");
            Console.WriteLine();
        }

        private static void StartServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;

                Console.WriteLine($"Server started on port {port}");
                Console.WriteLine($"Maps directory: {Path.GetFullPath(mapsDirectory)}");
                Console.WriteLine($"Gamemodes directory: {Path.GetFullPath(gamemodesDirectory)}");
                Console.WriteLine();

                // Start accepting clients
                Task.Run(AcceptClients);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static void StopServer()
        {
            isRunning = false;
            listener?.Stop();

            foreach (var client in clients)
            {
                client.Disconnect();
            }
            clients.Clear();

            Console.WriteLine("Server stopped.");
        }

        private static async Task AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    var tcpClient = await listener!.AcceptTcpClientAsync();
                    var clientHandler = new ClientHandler(tcpClient, OnClientDisconnected);
                    clients.Add(clientHandler);

                    Console.WriteLine($"Client connected from {tcpClient.Client.RemoteEndPoint}. Total clients: {clients.Count}");
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Console.WriteLine($"Error accepting client: {ex.Message}");
                    }
                }
            }
        }

        private static void OnClientDisconnected(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine($"Client disconnected. Total clients: {clients.Count}");
        }

        private static void ShowStatus()
        {
            Console.WriteLine($"Server Status:");
            Console.WriteLine($"  Running: {isRunning}");
            Console.WriteLine($"  Port: {port}");
            Console.WriteLine($"  Connected Clients: {clients.Count}");
            Console.WriteLine($"  Maps Available: {Directory.GetFiles(mapsDirectory, "*.map").Length}");
            Console.WriteLine($"  Gamemodes Available: {Directory.GetFiles(gamemodesDirectory, "*.json").Length}");
            Console.WriteLine();
        }

        private static void ShowClients()
        {
            Console.WriteLine($"Connected Clients ({clients.Count}):");
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                Console.WriteLine($"  {i + 1}. {client.RemoteEndPoint} - {client.Username}");
            }
            Console.WriteLine();
        }

        public static void BroadcastMessage(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            var data = System.Text.Encoding.UTF8.GetBytes(json);

            foreach (var client in clients)
            {
                client.SendMessage(data);
            }
        }

        public static string GetMapsDirectory() => mapsDirectory;
        public static string GetGamemodesDirectory() => gamemodesDirectory;
    }

    public class ClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream? stream;
        private bool isConnected = false;
        private Action<ClientHandler>? onDisconnected;
        private string username = "Unknown";

        public string RemoteEndPoint => tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        public string Username => username;

        public ClientHandler(TcpClient tcpClient, Action<ClientHandler>? onDisconnected = null)
        {
            this.tcpClient = tcpClient;
            this.onDisconnected = onDisconnected;
            stream = tcpClient.GetStream();
            isConnected = true;

            // Start handling client
            Task.Run(HandleClient);
        }

        private async Task HandleClient()
        {
            try
            {
                while (isConnected && stream != null)
                {
                    // Read message length
                    var lengthBytes = new byte[4];
                    var bytesRead = await stream.ReadAsync(lengthBytes, 0, 4);
                    if (bytesRead == 0) break;

                    var messageLength = BitConverter.ToInt32(lengthBytes, 0);

                    // Read message data
                    var messageBytes = new byte[messageLength];
                    var totalBytesRead = 0;

                    while (totalBytesRead < messageLength)
                    {
                        var read = await stream.ReadAsync(messageBytes, totalBytesRead, 
                            messageLength - totalBytesRead);
                        if (read == 0) break;
                        totalBytesRead += read;
                    }

                    // Process message
                    var json = System.Text.Encoding.UTF8.GetString(messageBytes);
                    await ProcessMessage(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        private async Task ProcessMessage(string json)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<ServerMessage>(json);
                if (message == null) return;

                switch (message.Type)
                {
                    case "join":
                        await HandleJoinMessage(message);
                        break;
                    case "request_map":
                        await HandleMapRequest(message);
                        break;
                    case "request_gamemode":
                        await HandleGamemodeRequest(message);
                        break;
                    case "chat":
                        await HandleChatMessage(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        private async Task HandleJoinMessage(ServerMessage message)
        {
            username = message.Data?.ToString() ?? "Unknown";
            Console.WriteLine($"Client {RemoteEndPoint} joined as '{username}'");

            // Send welcome message
            var welcomeMessage = new ServerMessage
            {
                Type = "welcome",
                Data = new { 
                    message = "Welcome to Castle Story Multiplayer Server!",
                    maxPlayers = 32,
                    currentPlayers = Program.clients.Count
                }
            };
            await SendMessage(welcomeMessage);
        }

        private async Task HandleMapRequest(ServerMessage message)
        {
            var mapName = message.Data?.ToString();
            if (string.IsNullOrEmpty(mapName)) return;

            var mapPath = Path.Combine(Program.GetMapsDirectory(), mapName + ".map");
            if (File.Exists(mapPath))
            {
                // Send map file
                var mapData = await File.ReadAllBytesAsync(mapPath);
                var mapMessage = new ServerMessage
                {
                    Type = "map_data",
                    Data = new { 
                        mapName = mapName,
                        data = Convert.ToBase64String(mapData),
                        size = mapData.Length
                    }
                };
                await SendMessage(mapMessage);
            }
            else
            {
                // Send error
                var errorMessage = new ServerMessage
                {
                    Type = "error",
                    Data = $"Map '{mapName}' not found"
                };
                await SendMessage(errorMessage);
            }
        }

        private async Task HandleGamemodeRequest(ServerMessage message)
        {
            var gamemodeName = message.Data?.ToString();
            if (string.IsNullOrEmpty(gamemodeName)) return;

            var gamemodePath = Path.Combine(Program.GetGamemodesDirectory(), gamemodeName + ".json");
            if (File.Exists(gamemodePath))
            {
                // Send gamemode data
                var gamemodeData = await File.ReadAllTextAsync(gamemodePath);
                var gamemodeMessage = new ServerMessage
                {
                    Type = "gamemode_data",
                    Data = gamemodeData
                };
                await SendMessage(gamemodeMessage);
            }
            else
            {
                // Send error
                var errorMessage = new ServerMessage
                {
                    Type = "error",
                    Data = $"Gamemode '{gamemodeName}' not found"
                };
                await SendMessage(errorMessage);
            }
        }

        private async Task HandleChatMessage(ServerMessage message)
        {
            // Broadcast chat message to all clients
            var chatMessage = new ServerMessage
            {
                Type = "chat",
                Data = new { 
                    username = username,
                    message = message.Data?.ToString()
                }
            };
            Program.BroadcastMessage(chatMessage);
        }

        public async Task SendMessage(object message)
        {
            if (!isConnected || stream == null) return;

            try
            {
                var json = JsonConvert.SerializeObject(message);
                var data = System.Text.Encoding.UTF8.GetBytes(json);

                // Send message length first
                var lengthBytes = BitConverter.GetBytes(data.Length);
                await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

                // Send message data
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to {RemoteEndPoint}: {ex.Message}");
            }
        }

        public void SendMessage(byte[] data)
        {
            if (!isConnected || stream == null) return;

            try
            {
                // Send message length first
                var lengthBytes = BitConverter.GetBytes(data.Length);
                stream.Write(lengthBytes, 0, lengthBytes.Length);

                // Send message data
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {RemoteEndPoint}: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            stream?.Close();
            tcpClient.Close();
            onDisconnected?.Invoke(this);
        }
    }

    public class ServerMessage
    {
        public string Type { get; set; } = "";
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
