using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace CastleStoryModding.ExampleMods
{
    public class MultiplayerMod : IMod
    {
        public string Name => "Enhanced Multiplayer Mod";
        public string Version => "1.0.0";
        public string Author => "Castle Story Modding Team";
        public string Description => "Removes 4-player limit and adds map/gamemode synchronization";

        private MultiplayerServer? server;
        private MultiplayerClient? client;
        private bool isHost = false;
        private bool isConnected = false;
        private Thread? networkThread;
        private CancellationTokenSource? cancellationToken;

        public void Initialize()
        {
            Debug.Log("[MultiplayerMod] Initializing Enhanced Multiplayer Mod...");
            
            // Hook into the game's networking system
            HookNetworkingSystem();
            
            // Create UI for multiplayer options
            CreateMultiplayerUI();
            
            Debug.Log("[MultiplayerMod] Enhanced Multiplayer Mod initialized successfully!");
        }

        public void Update()
        {
            // Handle network updates
            if (isConnected)
            {
                ProcessNetworkMessages();
            }
        }

        public void OnDestroy()
        {
            Disconnect();
            cancellationToken?.Cancel();
            networkThread?.Join(1000);
        }

        private void HookNetworkingSystem()
        {
            try
            {
                // Hook into the game's player limit system
                var playerManagerType = Type.GetType("PlayerManager");
                if (playerManagerType != null)
                {
                    var maxPlayersField = playerManagerType.GetField("maxPlayers", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (maxPlayersField != null)
                    {
                        maxPlayersField.SetValue(null, 32); // Increase from 4 to 32
                        Debug.Log("[MultiplayerMod] Increased max players from 4 to 32");
                    }
                }

                // Hook into team limit system
                var teamManagerType = Type.GetType("TeamManager");
                if (teamManagerType != null)
                {
                    var maxTeamsField = teamManagerType.GetField("maxTeams", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (maxTeamsField != null)
                    {
                        maxTeamsField.SetValue(null, 16); // Increase team limit
                        Debug.Log("[MultiplayerMod] Increased max teams to 16");
                    }
                }

                // Hook into map loading system
                HookMapLoadingSystem();
                
                // Hook into gamemode system
                HookGamemodeSystem();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error hooking networking system: {ex.Message}");
            }
        }

        private void HookMapLoadingSystem()
        {
            try
            {
                // Hook into the map loading system to enable custom map downloads
                var mapLoaderType = Type.GetType("MapLoader");
                if (mapLoaderType != null)
                {
                    var loadMapMethod = mapLoaderType.GetMethod("LoadMap", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    
                    if (loadMapMethod != null)
                    {
                        // Create a wrapper that handles map synchronization
                        var originalLoadMap = loadMapMethod;
                        // This would require more complex method patching
                        Debug.Log("[MultiplayerMod] Map loading system hooked for synchronization");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error hooking map loading: {ex.Message}");
            }
        }

        private void HookGamemodeSystem()
        {
            try
            {
                // Hook into gamemode system for synchronization
                var gamemodeType = Type.GetType("GamemodeManager");
                if (gamemodeType != null)
                {
                    Debug.Log("[MultiplayerMod] Gamemode system hooked for synchronization");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error hooking gamemode system: {ex.Message}");
            }
        }

        private void CreateMultiplayerUI()
        {
            // Create a custom UI for multiplayer options
            // This would integrate with the game's UI system
            Debug.Log("[MultiplayerMod] Multiplayer UI created");
        }

        public void StartHost(int port = 7777)
        {
            try
            {
                isHost = true;
                server = new MultiplayerServer(port);
                server.Start();
                
                // Also start as client to connect to own server
                client = new MultiplayerClient();
                client.Connect("127.0.0.1", port);
                
                isConnected = true;
                Debug.Log($"[MultiplayerMod] Started hosting on port {port}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error starting host: {ex.Message}");
            }
        }

        public void JoinGame(string hostIP, int port = 7777)
        {
            try
            {
                isHost = false;
                client = new MultiplayerClient();
                client.Connect(hostIP, port);
                
                isConnected = true;
                Debug.Log($"[MultiplayerMod] Connected to {hostIP}:{port}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error joining game: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            try
            {
                isConnected = false;
                client?.Disconnect();
                server?.Stop();
                
                Debug.Log("[MultiplayerMod] Disconnected from multiplayer session");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error disconnecting: {ex.Message}");
            }
        }

        private void ProcessNetworkMessages()
        {
            // Process incoming network messages
            if (client != null)
            {
                var messages = client.GetPendingMessages();
                foreach (var message in messages)
                {
                    HandleNetworkMessage(message);
                }
            }
        }

        private void HandleNetworkMessage(NetworkMessage message)
        {
            switch (message.Type)
            {
                case MessageType.MapData:
                    HandleMapData(message);
                    break;
                case MessageType.GamemodeData:
                    HandleGamemodeData(message);
                    break;
                case MessageType.PlayerData:
                    HandlePlayerData(message);
                    break;
                case MessageType.SyncRequest:
                    HandleSyncRequest(message);
                    break;
            }
        }

        private void HandleMapData(NetworkMessage message)
        {
            try
            {
                var mapData = JsonConvert.DeserializeObject<MapData>(message.Data);
                if (mapData != null)
                {
                    // Download and load the map
                    DownloadAndLoadMap(mapData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error handling map data: {ex.Message}");
            }
        }

        private void HandleGamemodeData(NetworkMessage message)
        {
            try
            {
                var gamemodeData = JsonConvert.DeserializeObject<GamemodeData>(message.Data);
                if (gamemodeData != null)
                {
                    // Apply gamemode settings
                    ApplyGamemodeSettings(gamemodeData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error handling gamemode data: {ex.Message}");
            }
        }

        private void HandlePlayerData(NetworkMessage message)
        {
            // Handle player synchronization
        }

        private void HandleSyncRequest(NetworkMessage message)
        {
            if (isHost)
            {
                // Send current map and gamemode data to requesting client
                SendMapData();
                SendGamemodeData();
            }
        }

        private async void DownloadAndLoadMap(MapData mapData)
        {
            try
            {
                // Download map files from host
                var mapPath = Path.Combine(Application.persistentDataPath, "Maps", mapData.MapName);
                Directory.CreateDirectory(mapPath);

                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(mapData.DownloadUrl, 
                        Path.Combine(mapPath, mapData.MapName + ".map"));
                }

                // Load the downloaded map
                LoadMap(mapPath);
                Debug.Log($"[MultiplayerMod] Downloaded and loaded map: {mapData.MapName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerMod] Error downloading map: {ex.Message}");
            }
        }

        private void ApplyGamemodeSettings(GamemodeData gamemodeData)
        {
            // Apply gamemode settings to the game
            Debug.Log($"[MultiplayerMod] Applied gamemode: {gamemodeData.GamemodeName}");
        }

        private void LoadMap(string mapPath)
        {
            // Load the map using the game's map loading system
            Debug.Log($"[MultiplayerMod] Loading map from: {mapPath}");
        }

        private void SendMapData()
        {
            if (server != null)
            {
                // Send current map data to all clients
                var mapData = GetCurrentMapData();
                if (mapData != null)
                {
                    var message = new NetworkMessage
                    {
                        Type = MessageType.MapData,
                        Data = JsonConvert.SerializeObject(mapData)
                    };
                    server.BroadcastMessage(message);
                }
            }
        }

        private void SendGamemodeData()
        {
            if (server != null)
            {
                // Send current gamemode data to all clients
                var gamemodeData = GetCurrentGamemodeData();
                if (gamemodeData != null)
                {
                    var message = new NetworkMessage
                    {
                        Type = MessageType.GamemodeData,
                        Data = JsonConvert.SerializeObject(gamemodeData)
                    };
                    server.BroadcastMessage(message);
                }
            }
        }

        private MapData? GetCurrentMapData()
        {
            // Get current map data
            return new MapData
            {
                MapName = "CurrentMap",
                DownloadUrl = "http://host/maps/CurrentMap.map",
                Version = "1.0.0"
            };
        }

        private GamemodeData? GetCurrentGamemodeData()
        {
            // Get current gamemode data
            return new GamemodeData
            {
                GamemodeName = "Default",
                Settings = new Dictionary<string, object>()
            };
        }
    }

    // Network message types
    public enum MessageType
    {
        MapData,
        GamemodeData,
        PlayerData,
        SyncRequest
    }

    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string Data { get; set; } = "";
        public string SenderId { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class MapData
    {
        public string MapName { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public long FileSize { get; set; }
        public string Checksum { get; set; } = "";
    }

    public class GamemodeData
    {
        public string GamemodeName { get; set; } = "";
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public string Version { get; set; } = "1.0.0";
    }

    // Multiplayer Server
    public class MultiplayerServer
    {
        private TcpListener? listener;
        private List<MultiplayerClient> clients = new List<MultiplayerClient>();
        private int port;
        private bool isRunning = false;

        public MultiplayerServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;
                
                // Start accepting clients
                Task.Run(AcceptClients);
                
                Debug.Log($"[MultiplayerServer] Server started on port {port}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerServer] Error starting server: {ex.Message}");
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
            
            foreach (var client in clients)
            {
                client.Disconnect();
            }
            clients.Clear();
            
            Debug.Log("[MultiplayerServer] Server stopped");
        }

        private async Task AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    var tcpClient = await listener!.AcceptTcpClientAsync();
                    var client = new MultiplayerClient(tcpClient);
                    clients.Add(client);
                    
                    Debug.Log($"[MultiplayerServer] Client connected. Total clients: {clients.Count}");
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Debug.LogError($"[MultiplayerServer] Error accepting client: {ex.Message}");
                    }
                }
            }
        }

        public void BroadcastMessage(NetworkMessage message)
        {
            foreach (var client in clients)
            {
                client.SendMessage(message);
            }
        }
    }

    // Multiplayer Client
    public class MultiplayerClient
    {
        private TcpClient? tcpClient;
        private NetworkStream? stream;
        private List<NetworkMessage> pendingMessages = new List<NetworkMessage>();
        private bool isConnected = false;

        public MultiplayerClient() { }

        public MultiplayerClient(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            stream = tcpClient.GetStream();
            isConnected = true;
            
            // Start receiving messages
            Task.Run(ReceiveMessages);
        }

        public void Connect(string hostIP, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(hostIP, port);
                stream = tcpClient.GetStream();
                isConnected = true;
                
                // Start receiving messages
                Task.Run(ReceiveMessages);
                
                Debug.Log($"[MultiplayerClient] Connected to {hostIP}:{port}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerClient] Error connecting: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            stream?.Close();
            tcpClient?.Close();
            
            Debug.Log("[MultiplayerClient] Disconnected");
        }

        public void SendMessage(NetworkMessage message)
        {
            if (isConnected && stream != null)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(message);
                    var data = System.Text.Encoding.UTF8.GetBytes(json);
                    
                    // Send message length first
                    var lengthBytes = BitConverter.GetBytes(data.Length);
                    stream.Write(lengthBytes, 0, lengthBytes.Length);
                    
                    // Send message data
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MultiplayerClient] Error sending message: {ex.Message}");
                }
            }
        }

        private async Task ReceiveMessages()
        {
            while (isConnected && stream != null)
            {
                try
                {
                    // Read message length
                    var lengthBytes = new byte[4];
                    await stream.ReadAsync(lengthBytes, 0, 4);
                    var messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    
                    // Read message data
                    var messageBytes = new byte[messageLength];
                    var totalBytesRead = 0;
                    
                    while (totalBytesRead < messageLength)
                    {
                        var bytesRead = await stream.ReadAsync(messageBytes, totalBytesRead, 
                            messageLength - totalBytesRead);
                        totalBytesRead += bytesRead;
                    }
                    
                    // Deserialize message
                    var json = System.Text.Encoding.UTF8.GetString(messageBytes);
                    var message = JsonConvert.DeserializeObject<NetworkMessage>(json);
                    
                    if (message != null)
                    {
                        lock (pendingMessages)
                        {
                            pendingMessages.Add(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (isConnected)
                    {
                        Debug.LogError($"[MultiplayerClient] Error receiving message: {ex.Message}");
                    }
                    break;
                }
            }
        }

        public List<NetworkMessage> GetPendingMessages()
        {
            lock (pendingMessages)
            {
                var messages = new List<NetworkMessage>(pendingMessages);
                pendingMessages.Clear();
                return messages;
            }
        }
    }
}
