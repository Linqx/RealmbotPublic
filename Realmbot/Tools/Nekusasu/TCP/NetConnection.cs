using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Hidden;
using Bookkeeping;
using Stream;
using Buffer = Stream.Buffer;

namespace Networking.TCP {
    public abstract class NetConnection<TPacket> where TPacket : Packet {
        public delegate void OnConnectCallback(bool success, NetConnection<TPacket> connection);

        public delegate void OnDisconnectCallback(NetConnection<TPacket> connection);

        public delegate void OnTokenPacketCallback(TPacket packet, NetConnection<TPacket> connection);

        public IPAddress RemoteAddress => ((IPEndPoint) socket.RemoteEndPoint).Address;
        public IPEndPoint RemoteEndPoint => (IPEndPoint) socket.RemoteEndPoint;

        /// <summary>
        /// True if this connection has been disconnected
        /// </summary>
        public bool Disconnected => disconnected == 1;

        private string _rc4EncryptKey;
        public string Rc4EncryptKey {
            get => _rc4EncryptKey;
            set {
                _rc4EncryptKey = value;
                _outgoingRc4 = new RC4(Fast.FromHexString(value));
            }
        }

        private string _rc4DecryptKey;
        public string Rc4DecryptKey {
            get => _rc4DecryptKey;
            set {
                _rc4DecryptKey = value;
                _incomingRc4 = new RC4(Fast.FromHexString(value));
            }
        }
        public bool LittleEndian = false;

        /// <summary>
        /// System socket used to send and receive data
        /// </summary>
        private readonly Socket socket;

        /// <summary>
        /// Dictionary containing callbacks for token packets
        /// </summary>
        private readonly ConcurrentDictionary<int, OnTokenPacketCallback> tokenCallbacks =
            new ConcurrentDictionary<int, OnTokenPacketCallback>();

        /// <summary>
        /// Queue of packets to send async
        /// </summary>
        private readonly Queue<SendPayload> sendQueue = new Queue<SendPayload>();

        /// <summary>
        /// Buffer used to hold received data
        /// </summary>
        private Buffer currentBuffer;

        /// <summary>
        /// Factory used to create packets from received data
        /// </summary>
        private PacketFactory<TPacket> packetFactory;

        /// <summary>
        /// Value used to syncronize disconnection calls
        /// </summary>
        private int disconnected;

        /// <summary>
        /// Delegate to be called upon disconnect
        /// </summary>
        private OnDisconnectCallback disconnectCallback;

        /// <summary>
        /// The next token id to assign
        /// </summary>
        private int nextToken;

        /// <summary>
        /// True if currently sending a packet
        /// </summary>
        private bool sending;

        private RC4 _incomingRc4;
        private RC4 _outgoingRc4;

        protected NetConnection(Socket socket) {
            this.socket = socket;
            Init();
        }

        protected NetConnection() {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Init();
        }

        private void Init() {
            packetFactory = new PacketFactory<TPacket>();
            socket.NoDelay = true;
            currentBuffer = new Buffer(4);
        }

        #region Error Handling

        /// <summary>
        /// Checks if the given error qualifies for socket close
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool CheckError(SocketError error) {
            switch (error) {
                case SocketError.Success:
                case SocketError.IOPending:
                case SocketError.WouldBlock:
                    return false;
                default:
                    return true;
            }
        }

        #endregion

        private enum ReceiveState {
            Size,
            Payload
        }

        #region Connection

        #region Connect

        public bool Connect(string host, int port) {
            return Connect(new IPEndPoint(IPAddress.Parse(host), port));
        }

        public bool Connect(EndPoint endPoint) {
            try {
                socket.Connect(endPoint);
                return true;
            }
            catch {
                return false;
            }
        }

        public void ConnectAsync(string host, int port, OnConnectCallback callback) {
            ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port), callback);
        }

        public void ConnectAsync(EndPoint endPoint, OnConnectCallback callback) {
            try {
                socket.BeginConnect(endPoint, OnConnect, callback);
            }
            catch {
                callback(false, this);
            }
        }

        public void OnConnect(IAsyncResult ar) {
            OnConnectCallback callback = ar.AsyncState as OnConnectCallback;
            try {
                socket.EndConnect(ar);
                ReadAsync();
            }
            catch {
            }

            callback?.Invoke(socket.Connected, this);
        }

        #endregion

        #region Disconnect 

        public bool Disconnect() {
            if (Interlocked.CompareExchange(ref disconnected, 1, 0) == 1)
                return false; // return if this method was already called
            DoDisconnect();
            return true;
        }

        private void DoDisconnect() {
            disconnectCallback?.Invoke(this);
            socket.Close();
            OnDisconnect();
        }

        protected abstract void OnDisconnect();

        /// <summary>
        /// Sets the callback to be called upon disconnection
        /// </summary>
        /// <param name="callback"></param>
        public void SetDisconnectCallback(OnDisconnectCallback callback) {
            disconnectCallback = callback;
        }

        #endregion

        #endregion

        #region Sending

        private Buffer PackagePacket(TPacket packet) {
            Buffer payload;
            if (LittleEndian) {
                BitWriter w = new BitWriter();
                w.Write(0); // reserve size int space
                w.Write(packet.Id);
                packet.WritePacket(w);

                payload = w.GetData();
                System.Buffer.BlockCopy(BitConverter.GetBytes(payload.Size - 4), 0, payload.Data, 0,
                    4); // insert size int to the start
            }
            else {
                using (MemoryStream stream = new MemoryStream()) {
                    PacketWriter w = new PacketWriter(stream);
                    w.Write(0); // reserve size int space;
                    w.Write(packet.Id);
                    packet.WritePacketNetworkOrder(w);

                    byte[] data = stream.ToArray();
                    payload = new Buffer(data.Length);

                    /* Set size of packet to first 4 bytes of buffer. */
                    int networkSize = IPAddress.HostToNetworkOrder(data.Length);
                    byte[] sizeData = BitConverter.GetBytes(networkSize);
                    payload.AddData(sizeData, 0, 4);

                    /*
                     * Write data to buffer
                     * Skip first 4 bytes because they determine the size of the buffer.
                     */
                    payload.AddData(data, 4, data.Length - 4);
                }
            }

            if (!string.IsNullOrEmpty(Rc4EncryptKey)) {
                /* Set payload data with RC4 encrypted buffer. */
                _outgoingRc4.Crypt(payload.Data, 5, payload.Data.Length - 5);
            }

            return payload;
        }

        private static Buffer PackageTokenPacket(TPacket packet) {
            BitWriter w = new BitWriter();
            w.Write(0); // reserve size int space
            packet.WriteTokenPacket(w);

            Buffer payload = w.GetData();
            System.Buffer.BlockCopy(BitConverter.GetBytes(payload.Size - 4), 0, payload.Data, 0,
                4); // insert size int to the start

            return payload;
        }

        /// <summary>
        /// Creates a token and stores the callback
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private bool TryAssignToken(TPacket packet, OnTokenPacketCallback callback) {
            if (!(packet is ITokenPacket tokenPacket)) {
                Log.Error($"{packet.GetType().Name} is not an ITokenPacket!");
                return false;
            }

            int token = Interlocked.Increment(ref nextToken);
            tokenCallbacks[token] = callback;
            tokenPacket.Token = token;
            return true;
        }

        #region Sync

        public void Send(TPacket packet) {
            Buffer payload = PackagePacket(packet);
            SendBuffer(payload, packet);
        }

        public void SendTokenResponse(TPacket packet) {
            if (packet is ITokenPacket token)
                token.TokenResponse = true;
            Buffer payload = PackageTokenPacket(packet);
            SendBuffer(payload, packet);
        }

        public void SendToken(TPacket packet, OnTokenPacketCallback callback) {
            if (!TryAssignToken(packet, callback)) return;
            Buffer payload = PackageTokenPacket(packet);
            SendBuffer(payload, packet);
        }

        private void SendBuffer(Buffer buffer, TPacket packet) {
            SocketError error;
            try {
                socket.Send(buffer.Data, 0, buffer.Size, SocketFlags.None, out error);
            }
            catch {
                error = SocketError.Disconnecting;
            }

            if (CheckError(error)) {
                Log.Error("SocketError received on Send: " + error);
                if (packet is ITokenPacket token && !token.TokenResponse)
                    tokenCallbacks.TryRemove(token.Token, out OnTokenPacketCallback dummy);
                Disconnect();
            }
        }

        #endregion

        #region Async

        private class SendPayload {
            public Buffer buffer;

            public TPacket packet;
        }

        public void SendAsync(TPacket packet) {
            if (disconnected == 1) return;
            Buffer packagedBuffer = PackagePacket(packet);
            SendAsync(new SendPayload {
                buffer = packagedBuffer,
                packet = packet
            });
        }

        private void SendAsync(SendPayload payload) {
            lock (sendQueue) {
                if (disconnected == 1) return;
                if (sending) {
                    sendQueue.Enqueue(payload);
                    return;
                }

                sending = true;
            }

            SendBufferAsync(payload);
        }

        private void DequeuePayload() {
            SendPayload payload;
            lock (sendQueue) {
                if (sendQueue.Count == 0) {
                    sending = false;
                    return;
                }

                payload = sendQueue.Dequeue();
            }

            SendBufferAsync(payload);
        }

        private void SendBufferAsync(SendPayload payload) {
            if (disconnected == 1) {
                Log.Debug("Already disconnected");
                return;
            }
            socket.BeginSend(payload.buffer.Data, 0, payload.buffer.Size, SocketFlags.None, out SocketError error,
                OnSend, payload);
            if (CheckError(error)) {
                Log.Error("SocketError received on SendAsync: " + error);
                if (payload.packet is ITokenPacket token && !token.TokenResponse)
                    tokenCallbacks.TryRemove(token.Token, out OnTokenPacketCallback dummy);
                Disconnect();
            }
        }

        private void OnSend(IAsyncResult ar) {
            if (disconnected == 1) return;
            int sentLength = socket.EndSend(ar, out SocketError error);
            SendPayload payload = (SendPayload) ar.AsyncState;
            if (CheckError(error)) {
                Log.Error("SocketError received on SendAsync: " + error);
                if (payload.packet is ITokenPacket token && !token.TokenResponse)
                    tokenCallbacks.TryRemove(token.Token, out OnTokenPacketCallback dummy);
                Disconnect();
            }
            else {
                DequeuePayload();
            }
        }

        #endregion

        #endregion
        
        #region Reading

        protected abstract void HandlePacket(TPacket packet);

        private void ReceivedSize() {
            int size = BitConverter.ToInt32(currentBuffer.Data, 0);

            if (!LittleEndian)
                size = IPAddress.NetworkToHostOrder(size);

            currentBuffer.Reset(size - 4);
        }

        private void ReceivedPayload() {
            /* Does not contain the size in the buffer. */
            byte[] data = currentBuffer.Data;

            try {
                if (!string.IsNullOrEmpty(Rc4DecryptKey)) {
                    _incomingRc4.Crypt(data, 1, data.Length - 1);
                }

                if (LittleEndian) HandlePayloadHostOrder(data);
                else HandlePayloadNetworkOrder(data);
            }
            finally {
                currentBuffer.Reset(4);
            }
        }

        private void HandleTokenPacket(TPacket packet, int token) {
            if (!tokenCallbacks.TryGetValue(token, out OnTokenPacketCallback callback)) return;
            callback(packet, this);
        }

        private void HandlePayloadHostOrder(byte[] data) {
            BitReader r = new BitReader(data, data.Length);
            byte id = r.ReadUInt8();
            TPacket packet = packetFactory.CreatePacket(id);
            switch (packet) {
                case null:
                    Log.Error($"No {typeof(TPacket).Name} for id: {id}");
                    return;
                case ITokenPacket token: {
                    packet.ReadTokenPacket(r);
                    if (token.TokenResponse)
                        HandleTokenPacket(packet, token.Token);
                    else
                        HandlePacket(packet);
                    break;
                }
                default:
                    packet.ReadPacket(r);
                    HandlePacket(packet);
                    break;
            }
        }

        private void HandlePayloadNetworkOrder(byte[] data) {
            using (MemoryStream stream = new MemoryStream(data)) {
                PacketReader r = new PacketReader(stream);
                byte id = r.ReadByte();
                TPacket packet = packetFactory.CreatePacket(id);
                switch (packet) {
                    case null:
                        Log.Error($"No {typeof(TPacket).Name} for id: {id}");
                        return;
                    default:
                        packet.ReadPacketNetworkOrder(r);
                        HandlePacket(packet);
                        break;
                }
            }
        }

        #region Sync

        public void Read() {
            while (socket.Connected) {
                ReadSize();
                ReadPayload();
            }
        }

        private void ReadSize() {
            if (disconnected == 1) return;
            while (currentBuffer.RemainingLength > 0)
                try {
                    socket.Receive(currentBuffer.Data, currentBuffer.Size, currentBuffer.RemainingLength,
                        SocketFlags.None,
                        out SocketError error);
                    if (CheckError(error)) {
                        if (Disconnect())
                            Log.Error("SocketError received on ReadSize: " + error);
                        return;
                    }
                }
                catch (ObjectDisposedException) // socket was already disposed
                {
                    return;
                }

            ReceivedSize();
        }

        private void ReadPayload() {
            if (disconnected == 1) return;
            while (currentBuffer.RemainingLength > 0)
                try {
                    socket.Receive(currentBuffer.Data, currentBuffer.Size, currentBuffer.RemainingLength,
                        SocketFlags.None,
                        out SocketError error);
                    if (CheckError(error)) {
                        if (Disconnect())
                            Log.Error("SocketError received on ReadPayload: " + error);
                        return;
                    }
                }
                catch (ObjectDisposedException) // socket was already disposed
                {
                    return;
                }

            ReceivedPayload();
        }

        #endregion

        #region Async

        public void ReadAsync() {
            BeginReadSize();
        }

        private bool TryEndRead(IAsyncResult ar) {
            if (disconnected == 1) return false;
            int length;
            try {
                length = socket.EndReceive(ar, out SocketError error);
                if (CheckError(error)) {
                    if (Disconnect())
                        Log.Error("SocketError received on TryEndRead: " + error);
                    return false;
                }
            }
            catch (ObjectDisposedException) // socket was already disposed
            {
                return false;
            }

            if (length <= 0) // closed socket, disconnect
            {
                Disconnect();
                return false;
            }

            currentBuffer.Size += length; // data was read into the buffer, increment the size accordingly
            return true;
        }

        private void BeginReadSize() {
            if (disconnected == 1) return;
            try {
                socket.BeginReceive(currentBuffer.Data, currentBuffer.Size, currentBuffer.RemainingLength,
                    SocketFlags.None,
                    out SocketError error, OnReadSizeCallback, null);

                if (CheckError(error))
                    if (Disconnect())
                        Log.Error("SocketError received on BeginReadSize: " + error);
            }
            catch (ObjectDisposedException) // socket was already disposed
            {
            }
        }

        private void OnReadSizeCallback(IAsyncResult ar) {
            if (!TryEndRead(ar))
                return;

            if (currentBuffer.RemainingLength > 0) {
                BeginReadSize(); // still need more data
            }
            else {
                ReceivedSize();
                BeginReadPayload();
            }
        }

        private void BeginReadPayload() {
            if (disconnected == 1) return;
            try {
                socket.BeginReceive(currentBuffer.Data, currentBuffer.Size, currentBuffer.RemainingLength,
                    SocketFlags.None,
                    out SocketError error, OnReadPayloadCallback, null);

                if (CheckError(error))
                    if (Disconnect())
                        Log.Error("SocketError received on BeginReadPayload: " + error);
            }
            catch (ObjectDisposedException) // socket was already disposed
            {
            }
        }

        private void OnReadPayloadCallback(IAsyncResult ar) {
            if (!TryEndRead(ar))
                return;

            if (currentBuffer.RemainingLength > 0) {
                BeginReadPayload(); // need more data
            }
            else {
                ReceivedPayload();
                BeginReadSize();
            }
        }

        #endregion

        #endregion

    }
}