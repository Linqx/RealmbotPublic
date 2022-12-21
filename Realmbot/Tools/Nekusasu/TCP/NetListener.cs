using System;
using System.Net;
using System.Net.Sockets;
using Bookkeeping;

namespace Networking.TCP {
    public abstract class NetListener<TConnection, TPacket>
        where TPacket : Packet
        where TConnection : NetConnection<TPacket> {
        public IPEndPoint LocalEndPoint;

        private readonly Socket socket;
        private bool running;

        protected NetListener(int port) {
            LocalEndPoint = new IPEndPoint(IPAddress.Any, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(LocalEndPoint);
            socket.Listen(5);
        }

        public virtual void Start() {
            Log.Write(LogEntry.Init(this).Append(" is listening on port: " + LocalEndPoint.Port));
            running = true;
            socket.BeginAccept(OnAcceptCallback, null);
        }

        public virtual void Stop() {
            running = false;
            socket.Close();
        }

        private void OnAcceptCallback(IAsyncResult ar) {
            Socket remoteSocket = socket.EndAccept(ar);
            if (!running) {
                remoteSocket.Dispose();
                return;
            }

            TConnection connection = (TConnection) Activator.CreateInstance(typeof(TConnection), remoteSocket);
            if (connection == null) {
                remoteSocket.Close();
                return;
            }

            HandleConnection(connection);
            socket.BeginAccept(OnAcceptCallback, null);
        }

        protected abstract void HandleConnection(TConnection connection);
    }
}