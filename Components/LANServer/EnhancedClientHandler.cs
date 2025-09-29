using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CastleStoryLANServer
{
    /// <summary>
    /// Enhanced client handler with better connection management and features
    /// </summary>
    public class EnhancedClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private EnhancedServer server;
        private ServerConfig config;
        private bool isConnected = true;
        private DateTime lastHeartbeat = DateTime.Now;
        private System.Timers.Timer? heartbeatTimer;

        public string ClientName { get; private set; } = "Unknown";
        public string EndPoint { get; private set; }
        public string Status { get; private set; } = "Connected";
        public DateTime ConnectedAt { get; private set; }
        public int ClientId { get; private set; }
        public bool IsAuthenticated { get; private set; } = false;
        public long BytesSent { get; private set; } = 0;
        public long BytesReceived { get; private set; } = 0;
        public int MessagesSent { get; private set; } = 0;
        public int MessagesReceived { get; private set; } = 0;
        
        public event EventHandler? OnDisconnected;
        public event EventHandler? OnNameChanged;
        public event EventHandler<string>? OnMessageReceived;

        public EnhancedClientHandler(TcpClient client, EnhancedServer server, ServerConfig config)
        {
            this.tcpClient = client;
            this.server = server;
            this.config = config;
            this.stream = client.GetStream();
            this.EndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            this.ConnectedAt = DateTime.Now;
            this.ClientId = GenerateClientId();

            // Start heartbeat timer
            StartHeartbeatTimer();

            // Start handling client messages
            Task.Run(HandleClient);
        }

        private int GenerateClientId()
        {
            return new Random().Next(1000, 9999);
        }

        private void StartHeartbeatTimer()
        {
            heartbeatTimer = new System.Timers.Timer(config.HeartbeatInterval * 2); // 2x server interval
            heartbeatTimer.Elapsed += (s, e) => CheckHeartbeat();
            heartbeatTimer.Start();
        }

        private void CheckHeartbeat()
        {
            var timeSinceLastHeartbeat = DateTime.Now - lastHeartbeat;
            if (timeSinceLastHeartbeat.TotalMilliseconds > config.HeartbeatInterval * 3)
            {
                Console.WriteLine($"⚠️ Client {ClientName} ({EndPoint}) timed out - no heartbeat for {timeSinceLastHeartbeat.TotalSeconds:F1}s");
                Disconnect("Connection timeout");
            }
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
                    BytesReceived += bytesRead;
                    MessagesReceived++;
                    
                    await ProcessMessage(message.Trim());
                }
            }
            catch (Exception ex)
            {
                if (isConnected)
                {
                    Console.WriteLine($"❌ Error handling client {ClientName}: {ex.Message}");
                }
            }
            finally
            {
                Disconnect("Connection lost");
            }
        }

        private async Task ProcessMessage(string message)
        {
            try
            {
                lastHeartbeat = DateTime.Now; // Update heartbeat on any message
                
                var parts = message.Split('|');
                if (parts.Length == 0) return;

                var command = parts[0].ToUpper();
                
                switch (command)
                {
                    case "AUTH":
                        await HandleAuthentication(parts);
                        break;
                    case "SET_NAME":
                        await HandleSetName(parts);
                        break;
                    case "CHAT":
                        await HandleChat(parts);
                        break;
                    case "PING":
                        await HandlePing();
                        break;
                    case "PONG":
                        await HandlePong();
                        break;
                    case "HEARTBEAT":
                        await HandleHeartbeat();
                        break;
                    case "GAME_DATA":
                        await HandleGameData(parts);
                        break;
                    case "PLAYER_UPDATE":
                        await HandlePlayerUpdate(parts);
                        break;
                    case "REQUEST_INFO":
                        await HandleRequestInfo();
                        break;
                    default:
                        Console.WriteLine($"❓ Unknown command from {ClientName}: {command}");
                        break;
                }

                OnMessageReceived?.Invoke(this, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing message from {ClientName}: {ex.Message}");
            }
        }

        private async Task HandleAuthentication(string[] parts)
        {
            if (parts.Length < 2)
            {
                SendMessage("AUTH_FAIL|Invalid authentication format");
                return;
            }

            var password = parts[1];
            
            if (string.IsNullOrEmpty(config.Password) || password == config.Password)
            {
                IsAuthenticated = true;
                Status = "Authenticated";
                SendMessage("AUTH_SUCCESS|Authentication successful");
                Console.WriteLine($"✅ Client {EndPoint} authenticated successfully");
            }
            else
            {
                SendMessage("AUTH_FAIL|Invalid password");
                Console.WriteLine($"❌ Authentication failed for {EndPoint} - invalid password");
                Disconnect("Authentication failed");
            }
        }

        private async Task HandleSetName(string[] parts)
        {
            if (parts.Length < 2)
            {
                SendMessage("SET_NAME_FAIL|Invalid name format");
                return;
            }

            var newName = parts[1].Trim();
            if (string.IsNullOrEmpty(newName) || newName.Length > 32)
            {
                SendMessage("SET_NAME_FAIL|Invalid name length");
                return;
            }

            var oldName = ClientName;
            ClientName = newName;
            Status = IsAuthenticated ? "Authenticated" : "Connected";
            
            SendMessage("SET_NAME_SUCCESS|Name set successfully");
            OnNameChanged?.Invoke(this, EventArgs.Empty);
            
            Console.WriteLine($"📝 Client {EndPoint} changed name from '{oldName}' to '{newName}'");
        }

        private async Task HandleChat(string[] parts)
        {
            if (parts.Length < 2)
            {
                SendMessage("CHAT_FAIL|Invalid chat format");
                return;
            }

            var chatMessage = parts[1];
            if (string.IsNullOrEmpty(chatMessage) || chatMessage.Length > 500)
            {
                SendMessage("CHAT_FAIL|Invalid message length");
                return;
            }

            // Echo the chat message back to the client
            SendMessage($"CHAT_SUCCESS|Message sent");
            
            // The server will handle broadcasting to other clients
            Console.WriteLine($"💬 {ClientName}: {chatMessage}");
        }

        private async Task HandlePing()
        {
            SendMessage("PONG");
        }

        private async Task HandlePong()
        {
            // Client responded to ping, update heartbeat
            lastHeartbeat = DateTime.Now;
        }

        private async Task HandleHeartbeat()
        {
            // Client sent heartbeat, update timestamp
            lastHeartbeat = DateTime.Now;
            SendMessage("HEARTBEAT_ACK");
        }

        private async Task HandleGameData(string[] parts)
        {
            if (parts.Length < 2)
            {
                SendMessage("GAME_DATA_FAIL|Invalid game data format");
                return;
            }

            var gameData = parts[1];
            Console.WriteLine($"🎮 Game data from {ClientName}: {gameData.Length} bytes");
            
            // Process game data (this would be game-specific)
            // For now, just acknowledge receipt
            SendMessage("GAME_DATA_ACK|Data received");
        }

        private async Task HandlePlayerUpdate(string[] parts)
        {
            if (parts.Length < 2)
            {
                SendMessage("PLAYER_UPDATE_FAIL|Invalid player update format");
                return;
            }

            var playerData = parts[1];
            Console.WriteLine($"👤 Player update from {ClientName}: {playerData.Length} bytes");
            
            // Process player update (this would be game-specific)
            SendMessage("PLAYER_UPDATE_ACK|Update received");
        }

        private async Task HandleRequestInfo()
        {
            var serverInfo = new
            {
                serverName = config.ServerName,
                version = config.ServerVersion,
                maxPlayers = config.MaxPlayers,
                currentPlayers = server.GetClientCount(),
                hasPassword = !string.IsNullOrEmpty(config.Password),
                uptime = DateTime.Now - server.GetStartTime()
            };

            var infoJson = System.Text.Json.JsonSerializer.Serialize(serverInfo);
            SendMessage($"SERVER_INFO|{infoJson}");
        }

        public void SendMessage(string message)
        {
            if (!isConnected || !tcpClient.Connected)
                return;

            try
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(messageBytes, 0, messageBytes.Length);
                stream.Flush();
                
                BytesSent += messageBytes.Length;
                MessagesSent++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending message to {ClientName}: {ex.Message}");
                Disconnect("Send error");
            }
        }

        public void Disconnect(string reason = "Disconnected")
        {
            if (!isConnected)
                return;

            isConnected = false;
            Status = "Disconnected";
            
            try
            {
                heartbeatTimer?.Stop();
                heartbeatTimer?.Dispose();
                
                if (tcpClient.Connected)
                {
                    SendMessage($"DISCONNECT|{reason}");
                }
                
                tcpClient.Close();
                stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error disconnecting client {ClientName}: {ex.Message}");
            }
            finally
            {
                OnDisconnected?.Invoke(this, EventArgs.Empty);
                Console.WriteLine($"👋 Client disconnected: {ClientName} ({EndPoint}) - {reason}");
            }
        }

        public string GetClientInfo()
        {
            return $"ID: {ClientId}, Name: {ClientName}, EndPoint: {EndPoint}, Status: {Status}, " +
                   $"Connected: {ConnectedAt:HH:mm:ss}, Bytes: {BytesSent}/{BytesReceived}, " +
                   $"Messages: {MessagesSent}/{MessagesReceived}";
        }
    }
}
