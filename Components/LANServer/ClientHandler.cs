using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CastleStoryLANServer
{
    public class ClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private LANServer server;
        private bool isConnected = true;

        public string ClientName { get; private set; } = "Unknown";
        public string EndPoint { get; private set; }
        public string Status { get; private set; } = "Connected";
        public DateTime ConnectedAt { get; private set; }
        
        public event EventHandler? OnDisconnected;
        public event EventHandler? OnNameChanged;

        public ClientHandler(TcpClient client, LANServer server)
        {
            this.tcpClient = client;
            this.server = server;
            this.stream = client.GetStream();
            this.EndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            this.ConnectedAt = DateTime.Now;

            // Start handling client messages
            Task.Run(HandleClient);
        }

        private async Task HandleClient()
        {
            try
            {
                byte[] buffer = new byte[4096];
                while (isConnected && tcpClient.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break; // Client disconnected
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await ProcessMessage(message.Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client handler error: {ex.Message}");
            }
            finally
            {
                await Disconnect("Connection lost");
            }
        }

        private async Task ProcessMessage(string message)
        {
            try
            {
                var parts = message.Split('|');
                if (parts.Length < 2) return;

                string command = parts[0];
                string data = parts[1];

                switch (command)
                {
                    case "SET_NAME":
                        ClientName = data;
                        Status = "Ready";
                        Console.WriteLine($"Client {EndPoint} set name to: {ClientName}");
                        await SendMessage("NAME_SET|OK");
                        OnNameChanged?.Invoke(this, EventArgs.Empty);
                        break;

                    case "JOIN_GAME":
                        Status = "In Game";
                        Console.WriteLine($"Client {ClientName} joined game");
                        await SendMessage("GAME_JOINED|OK");
                        if (server != null)
                        {
                            await server.BroadcastGameUpdate($"PLAYER_JOINED|{ClientName}");
                        }
                        break;

                    case "LEAVE_GAME":
                        Status = "Ready";
                        Console.WriteLine($"Client {ClientName} left game");
                        await SendMessage("GAME_LEFT|OK");
                        if (server != null)
                        {
                            await server.BroadcastGameUpdate($"PLAYER_LEFT|{ClientName}");
                        }
                        break;

                    case "GAME_DATA":
                        // Forward game data to other clients
                        if (server != null)
                        {
                            await server.BroadcastGameUpdate($"PLAYER_DATA|{ClientName}|{data}");
                        }
                        break;

                    case "CHAT_MESSAGE":
                        Console.WriteLine($"Chat from {ClientName}: {data}");
                        if (server != null)
                        {
                            await server.BroadcastGameUpdate($"CHAT|{ClientName}|{data}");
                        }
                        break;

                    case "PING":
                        await SendMessage("PONG|" + DateTime.Now.Ticks);
                        break;

                    default:
                        Console.WriteLine($"Unknown command from {ClientName}: {command}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message from {ClientName}: {ex.Message}");
            }
        }

        public async Task SendMessage(string message)
        {
            try
            {
                if (isConnected && tcpClient.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to {ClientName}: {ex.Message}");
            }
        }

        public async Task Disconnect(string reason)
        {
            if (isConnected)
            {
                isConnected = false;
                Console.WriteLine($"Disconnecting {ClientName}: {reason}");
                
                try
                {
                    await SendMessage($"DISCONNECT|{reason}");
                    if (server != null)
                    {
                        await server.RemoveClient(this);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during disconnect: {ex.Message}");
                }
                finally
                {
                    stream?.Close();
                    tcpClient?.Close();
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
