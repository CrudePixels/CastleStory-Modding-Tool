using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CastleStoryLANClient
{
    public class LANClient
    {
        private UdpClient udpClient;
        private TcpClient tcpClient;
        private NetworkStream stream;
        private List<ServerInfo> discoveredServers = new List<ServerInfo>();
        private bool isRunning = false;
        private bool isConnected = false;
        private System.Threading.Timer? autoDiscoverTimer;

        public class ServerInfo
        {
            public string Name { get; set; } = "";
            public int Port { get; set; }
            public int PlayerCount { get; set; }
            public string Version { get; set; } = "";
            public IPEndPoint EndPoint { get; set; }
        }


        public void Start()
        {
            try
            {
                udpClient = new UdpClient(0); // Bind to any available port
                isRunning = true;

                Console.WriteLine("LAN Client started");
                Console.WriteLine("Commands: discover, connect, list, help, quit");
                Console.WriteLine("Auto-discovery: Every 5 seconds when not connected\n");

                Task.Run(HandleUserInput);
                Task.Run(HandleServerResponses);
                
                // Start auto-discovery timer
                StartAutoDiscovery();

                // Keep client running
                while (isRunning)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        private async Task HandleUserInput()
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
            switch (command.ToLower())
            {
                case "help":
                    ShowHelp();
                    break;
                case "discover":
                    await DiscoverServers();
                    break;
                case "list":
                    ListServers();
                    break;
                case "connect":
                    await ConnectToServer();
                    break;
                case "disconnect":
                    await Disconnect();
                    break;
                case "status":
                    ShowStatus();
                    break;
                case "quit":
                case "exit":
                    await Quit();
                    break;
                default:
                    Console.WriteLine("Unknown command. Type 'help' for available commands.");
                    break;
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("\n=== Available Commands ===");
            Console.WriteLine("discover  - Discover LAN servers");
            Console.WriteLine("list      - List discovered servers");
            Console.WriteLine("connect   - Connect to a server");
            Console.WriteLine("disconnect - Disconnect from current server");
            Console.WriteLine("status    - Show connection status");
            Console.WriteLine("help      - Show this help message");
            Console.WriteLine("quit      - Exit the client");
            Console.WriteLine("========================\n");
        }

        private async Task DiscoverServers()
        {
            Console.WriteLine("Discovering LAN servers...");
            int previousCount = discoveredServers.Count;
            discoveredServers.Clear();

            try
            {
                // Send discovery broadcast
                var message = "DISCOVER_SERVERS";
                var data = Encoding.UTF8.GetBytes(message);
                var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 7778); // Server discovery port

                await udpClient.SendAsync(data, data.Length, broadcastEndPoint);
                Console.WriteLine("Discovery request sent");

                // Wait for responses
                await Task.Delay(3000);
                Console.WriteLine($"Found {discoveredServers.Count} servers");
                
                if (discoveredServers.Count > 0)
                {
                    Console.WriteLine("Auto-discovery will continue every 5 seconds when not connected.\n");
                }
                else
                {
                    Console.WriteLine("No servers found. Auto-discovery will continue every 5 seconds.\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discovery error: {ex.Message}\n");
            }
        }

        private async Task HandleServerResponses()
        {
            while (isRunning)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
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

                            // Check if server already exists
                            bool serverExists = discoveredServers.Any(s => s.Name == serverInfo.Name && s.EndPoint.Address.Equals(serverInfo.EndPoint.Address));
                            
                            if (!serverExists)
                            {
                                discoveredServers.Add(serverInfo);
                                Console.WriteLine($"\n[Auto-Discovery] Found server: {serverInfo.Name} ({serverInfo.PlayerCount} players) - v{serverInfo.Version}");
                                Console.WriteLine("Type 'list' to see all servers or 'connect' to join one.\n");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Console.WriteLine($"Response handling error: {ex.Message}");
                }
            }
        }

        private void ListServers()
        {
            Console.WriteLine($"\n=== Discovered Servers ({discoveredServers.Count}) ===");
            for (int i = 0; i < discoveredServers.Count; i++)
            {
                var server = discoveredServers[i];
                Console.WriteLine($"{i}: {server.Name} - {server.PlayerCount} players - v{server.Version} - {server.EndPoint}");
            }
            Console.WriteLine("=====================================\n");
        }

        private async Task ConnectToServer()
        {
            if (discoveredServers.Count == 0)
            {
                Console.WriteLine("No servers discovered. Run 'discover' first.\n");
                return;
            }

            ListServers();
            Console.Write("Enter server number to connect: ");
            
            if (int.TryParse(Console.ReadLine(), out int serverIndex) && 
                serverIndex >= 0 && serverIndex < discoveredServers.Count)
            {
                var server = discoveredServers[serverIndex];
                await ConnectToServer(server);
            }
            else
            {
                Console.WriteLine("Invalid server number\n");
            }
        }

        private async Task ConnectToServer(ServerInfo server)
        {
            try
            {
                // Disconnect from current server first if connected
                if (isConnected)
                {
                    Console.WriteLine("Disconnecting from current server first...");
                    await Disconnect();
                }
                
                Console.WriteLine($"Connecting to {server.Name}...");
                
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(server.EndPoint.Address, server.Port);
                stream = tcpClient.GetStream();

                isConnected = true;
                Console.WriteLine($"Connected to {server.Name}!");
                Console.WriteLine("Commands: setname, join, leave, chat, ping, disconnect\n");

                // Start handling server messages
                Task.Run(HandleServerMessages);

                // Set default name
                await SendMessage("SET_NAME|LAN_Player");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}\n");
            }
        }

        private async Task HandleServerMessages()
        {
            try
            {
                byte[] buffer = new byte[4096];
                while (tcpClient?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessServerMessage(message.Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server message error: {ex.Message}");
            }
        }

        private void ProcessServerMessage(string message)
        {
            var parts = message.Split('|');
            if (parts.Length < 2) return;

            string command = parts[0];
            string data = parts[1];

            switch (command)
            {
                case "NAME_SET":
                    Console.WriteLine("Name set successfully");
                    break;
                case "GAME_JOINED":
                    Console.WriteLine("Joined game successfully");
                    break;
                case "GAME_LEFT":
                    Console.WriteLine("Left game successfully");
                    break;
                case "BROADCAST":
                    Console.WriteLine($"Server: {data}");
                    break;
                case "GAME_UPDATE":
                    if (parts.Length >= 3)
                    {
                        string updateType = parts[1];
                        string updateData = parts[2];
                        Console.WriteLine($"Game Update [{updateType}]: {updateData}");
                    }
                    break;
                case "CHAT":
                    if (parts.Length >= 3)
                    {
                        string player = parts[1];
                        string chatMessage = parts[2];
                        Console.WriteLine($"[{player}]: {chatMessage}");
                    }
                    break;
                case "PONG":
                    Console.WriteLine($"Pong received: {data}");
                    break;
                case "DISCONNECT":
                    Console.WriteLine($"Disconnected: {data}");
                    break;
                default:
                    Console.WriteLine($"Unknown server message: {command}");
                    break;
            }
        }

        private async Task SendMessage(string message)
        {
            try
            {
                if (tcpClient?.Connected == true)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send error: {ex.Message}");
            }
        }

        private async Task Disconnect()
        {
            if (tcpClient?.Connected == true)
            {
                await SendMessage("DISCONNECT|Client disconnect");
                tcpClient.Close();
                isConnected = false;
                Console.WriteLine("Disconnected from server\n");
            }
            else
            {
                Console.WriteLine("Not connected to any server\n");
            }
        }

        private void ShowStatus()
        {
            Console.WriteLine($"\n=== Client Status ===");
            Console.WriteLine($"Connected: {tcpClient?.Connected == true}");
            Console.WriteLine($"Discovered Servers: {discoveredServers.Count}");
            Console.WriteLine($"Running: {isRunning}");
            Console.WriteLine("===================\n");
        }

        private async Task Quit()
        {
            Console.WriteLine("Quitting...");
            await Disconnect();
            isRunning = false;
            autoDiscoverTimer?.Dispose();
            udpClient?.Close();
            Environment.Exit(0);
        }

        private void StartAutoDiscovery()
        {
            // Start auto-discovery timer that runs every 5 seconds
            autoDiscoverTimer = new System.Threading.Timer(async _ => await AutoDiscoverServers(), null, 0, 5000);
        }

        private async Task AutoDiscoverServers()
        {
            // Only auto-discover if not connected
            if (!isConnected && isRunning)
            {
                try
                {
                    // Send discovery broadcast
                    var message = "DISCOVER_SERVERS";
                    var data = Encoding.UTF8.GetBytes(message);
                    var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 7778); // Server discovery port

                    await udpClient.SendAsync(data, data.Length, broadcastEndPoint);
                    
                    // Show a subtle indicator that discovery is happening
                    Console.Write(".");
                }
                catch (Exception ex)
                {
                    // Silently handle auto-discovery errors to avoid spam
                    // Console.WriteLine($"Auto-discovery error: {ex.Message}");
                }
            }
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LANClientGUI());
        }
    }
}
