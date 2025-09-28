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
using System.Windows.Forms;

namespace CastleStoryLANServer
{
    public class LANServer
    {
        private TcpListener? tcpListener;
        private UdpClient? udpClient;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private bool isRunning = false;
        private int port = 7777;
        private string serverName = "Castle Story LAN Server";
        private string serverVersion = "1.0.0";


        public void Start()
        {
            try
            {
                // Start TCP server for game connections
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                isRunning = true;

                // Start UDP server for discovery
                udpClient = new UdpClient(port + 1);

                Console.WriteLine($"LAN Server started on port {port}");
                Console.WriteLine($"Discovery server on port {port + 1}");
                Console.WriteLine($"Server Name: {serverName}");
                Console.WriteLine($"Version: {serverVersion}");
                Console.WriteLine("\nWaiting for connections...\n");

                // Start accepting connections
                Task.Run(AcceptConnections);
                Task.Run(HandleDiscovery);
                Task.Run(HandleServerCommands);

                // Keep server running
                while (isRunning)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }

        private async Task AcceptConnections()
        {
            while (isRunning)
            {
                try
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync();
                    var clientHandler = new ClientHandler(tcpClient, this);
                    clients.Add(clientHandler);
                    
                    Console.WriteLine($"New connection from {tcpClient.Client.RemoteEndPoint}");
                    Console.WriteLine($"Total clients: {clients.Count}\n");
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Console.WriteLine($"Connection error: {ex.Message}");
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

                    Console.WriteLine($"Discovery request from {clientEndPoint}: {message}");

                    if (message == "DISCOVER_SERVERS")
                    {
                        var response = $"SERVER_INFO|{serverName}|{port}|{clients.Count}|{serverVersion}";
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        await udpClient.SendAsync(responseBytes, responseBytes.Length, clientEndPoint);
                        Console.WriteLine($"Sent server info to {clientEndPoint}");
                    }
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Console.WriteLine($"Discovery error: {ex.Message}");
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
            switch (command.ToLower())
            {
                case "help":
                    ShowHelp();
                    break;
                case "list":
                    ListClients();
                    break;
                case "kick":
                    Console.Write("Enter client ID to kick: ");
                    if (int.TryParse(Console.ReadLine(), out int kickId))
                    {
                        await KickClient(kickId);
                    }
                    break;
                case "broadcast":
                    Console.Write("Enter message to broadcast: ");
                    var message = Console.ReadLine();
                    if (message != null)
                {
                    await BroadcastMessage(message);
                }
                    break;
                case "status":
                    ShowStatus();
                    break;
                case "restart":
                    await RestartServer();
                    break;
                case "stop":
                case "quit":
                case "exit":
                    await StopServer();
                    break;
                default:
                    Console.WriteLine("Unknown command. Type 'help' for available commands.");
                    break;
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("\n=== Available Commands ===");
            Console.WriteLine("help     - Show this help message");
            Console.WriteLine("list     - List connected clients");
            Console.WriteLine("kick     - Kick a client by ID");
            Console.WriteLine("broadcast - Send message to all clients");
            Console.WriteLine("status   - Show server status");
            Console.WriteLine("restart  - Restart the server");
            Console.WriteLine("stop     - Stop the server");
            Console.WriteLine("========================\n");
        }

        private void ListClients()
        {
            Console.WriteLine($"\n=== Connected Clients ({clients.Count}) ===");
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                Console.WriteLine($"{i}: {client.ClientName} ({client.EndPoint}) - {client.Status}");
            }
            Console.WriteLine("===============================\n");
        }

        private async Task KickClient(int clientId)
        {
            if (clientId >= 0 && clientId < clients.Count)
            {
                var client = clients[clientId];
                Console.WriteLine($"Kicking client {clientId}: {client.ClientName}");
                await client.Disconnect("Kicked by server");
                clients.RemoveAt(clientId);
            }
            else
            {
                Console.WriteLine("Invalid client ID");
            }
        }

        private async Task BroadcastMessage(string message)
        {
            var broadcastMessage = $"BROADCAST|{message}";
            foreach (var client in clients)
            {
                await client.SendMessage(broadcastMessage);
            }
            Console.WriteLine($"Broadcasted: {message}");
        }

        private void ShowStatus()
        {
            Console.WriteLine($"\n=== Server Status ===");
            Console.WriteLine($"Server Name: {serverName}");
            Console.WriteLine($"Version: {serverVersion}");
            Console.WriteLine($"Port: {port}");
            Console.WriteLine($"UDP Discovery Port: {port + 1}");
            Console.WriteLine($"Status: {(isRunning ? "Running" : "Stopped")}");
            Console.WriteLine($"Connected Clients: {clients.Count}");
            Console.WriteLine($"Uptime: {DateTime.Now - Process.GetCurrentProcess().StartTime}");
            Console.WriteLine("====================\n");
        }

        private async Task RestartServer()
        {
            Console.WriteLine("Restarting server...");
            await StopServer();
            await Task.Delay(2000);
            Start();
        }

        private async Task StopServer()
        {
            Console.WriteLine("Stopping server...");
            isRunning = false;

            // Disconnect all clients
            foreach (var client in clients)
            {
                await client.Disconnect("Server shutting down");
            }
            clients.Clear();

            // Stop listeners
            tcpListener?.Stop();
            udpClient?.Close();

            Console.WriteLine("Server stopped.");
            Environment.Exit(0);
        }

        public async Task BroadcastGameUpdate(string gameData)
        {
            var message = $"GAME_UPDATE|{gameData}";
            foreach (var client in clients)
            {
                await client.SendMessage(message);
            }
        }

        public async Task RemoveClient(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine($"Client disconnected: {client.ClientName}");
            Console.WriteLine($"Remaining clients: {clients.Count}\n");
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LANServerGUI());
        }
    }
}
