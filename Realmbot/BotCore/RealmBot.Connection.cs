using System;
using BotCore.Networking;
using BotCore.Structures;
using BotTools;
using Kakusu;
using Kakusu.Utils.NET.Crypto;
using Nekusasu;
using Nekusasu.TCP;
using Rekishi;

namespace BotCore {
    public partial class RealmBot {
        public Action<RealmBot> OnDisconnect;
        public Action<RealmBot> OnTryReconnect;
        public RotMGClient Client;
        public string BuildVersion;
        public bool Reconnecting;
        public bool Connected;
        public string ConnectedServerName;
        public bool Disposed;
        public string ConnectionGuid = "";
        public Action<RotMGPacket> HandleFromState;
        public string CurrentServer => _currentServer;

        private string _currentServer;
        private int _currentGameId;
        private int _currentKeyTime;
        private byte[] _currentKey;
        private int _currentPort = 2050;

        public void Connect(string server, int port, int gameId, int keyTime, byte[] key) {
            _currentServer = server;
            _currentPort = port;
            _currentGameId = gameId;
            _currentKeyTime = keyTime;
            _currentKey = key;

            Client = CreateConnection();
            Client.ConnectAsync(_currentServer, _currentPort, ConnectCallback);
        }

        public void Connect(Server server, ConnectionMode connectionMode) {
            ConnectedServerName = server.ToString();

            _currentServer = EnumHelper.GetStringValue(server);
            _currentGameId = (int) connectionMode;
            _currentKeyTime = -1;
            _currentKey = null;

            Client = CreateConnection();
            Client.ConnectAsync(_currentServer, _currentPort, ConnectCallback);
        }

        private void ConnectCallback(bool connected, NetConnection<RotMGPacket> client) {
            if (connected) {
                Log.Write($"[{Name}] Connected to {Client.RemoteEndPoint} successfully!", ConsoleColor.Cyan);

                Connected = true;
                Disposed = false;

                if (Reconnecting) {
                    Reconnecting = false;
                    OnTryReconnect?.Invoke(this);
                }

                HelloPacket hello = CreateHello(_currentGameId, _currentKeyTime, _currentKey);
                Send(hello);
            }
            else {
                Log.Error($"[{Name}] Failed to connect. Attempting again in 5 seconds");
                Delay.RunSeconds(5.0, () => { Connect(_currentServer, _currentPort, _currentGameId, _currentKeyTime, _currentKey); });
            }
        }

        private void ResetConnection() {
            Client.Disconnect();
            UnmapHandlers();
            Client = CreateConnection();
        }

        public void Reconnect(string server, int port, int gameId, int keyTime, byte[] key) {
            ResetConnection();
            Reconnecting = true;

            if (string.IsNullOrEmpty(server))
                server = _currentServer;

            if (port == -1)
                port = _currentPort;

            Connect(server, port, gameId, keyTime, key);
        }

        public void Reconnect(Server server, ConnectionMode connectionMode) {
            ResetConnection();
            Reconnecting = true;
            Connect(server, connectionMode);
        }

        public void Reconnect(ConnectionMode connectionMode) {
            ResetConnection();
            Reconnecting = true;
            Connect(_currentServer, _currentPort, (int) connectionMode, _currentKeyTime, _currentKey);
        }

        public void Reconnect() {
            ResetConnection();
            Reconnecting = true;
            Connect(_currentServer, _currentPort, _currentGameId, _currentKeyTime, _currentKey);
        }

        private void Disconnected(NetConnection<RotMGPacket> client) {
            Disconnect();
        }

        public void Disconnect() {
            if (!Connected) return;
            ClearMoveActions();
            TextsToSend.Clear();
            OnDisconnect?.Invoke(this);
            Reconnecting = false;
            Connected = false;
        }

        public void Dispose() {
            if (Disposed) return;
            Disposed = true;
            Client.Disconnect();
        }

        private RotMGClient CreateConnection() {
            RotMGClient client = new RotMGClient(this);
            MapHandlers(client);
            MapPackets(client);
            SubscribePackets(client);
            client.SetDisconnectCallback(Disconnected);
            return client;
        }

        private HelloPacket CreateHello(int gameId, int keyTime, byte[] key) {
            /* Construct Hello Packet to connect to Realm of the Mad God. */

            return new HelloPacket {
                BuildVersion = BuildVersion,
                GameId = gameId,
                GUID = SCry.Encrypt(Email),
                Password = SCry.Encrypt(Password),
                Secret = "",
                KeyTime = keyTime,
                Key = key ?? new byte[0],
                MapJson = "",
                EntryTag = "",
                GameNet = "",
                GameUserNetId = "",
                PlayPlatform = "rotmg",
                PlatformToken = "",
                UserToken = "",
                PreviousConnectionGuid = ConnectionGuid
            };
        }

        public void Send(RotMGPacket packet) {
            if (!Connected) return;
            Client.SendAsync(packet);
        }

        public void QueuePacket(RotMGPacket packet) {
            /* Synchronously add packets to queue of packets to be handled. */
            lock (_packetSync) {
                _packetsQueue?.Add(packet);
            }
        }

        public RotMGPacket[] FlushPackets() {
            /* Synchronously receive all packets in queue and clear queue. */

            lock (_packetSync) {
                if (_packetsQueue == null || _packetsQueue.Count == 0)
                    return null;

                RotMGPacket[] packets = _packetsQueue.ToArray();
                _packetsQueue.Clear();
                return packets;
            }
        }

        protected virtual void MapPackets(RotMGClient client) {
        }

        protected virtual void SubscribePackets(RotMGClient client) {
        }
    }
}