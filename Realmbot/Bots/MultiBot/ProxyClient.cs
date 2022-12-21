using System;
using System.Net.Sockets;

namespace MultiBot {
    public class ProxyClient : IDisposable {
        private readonly NetworkConnection connection;
        private readonly Disconnect disconnect;
        private bool disconnected;

        public ProxyClient(Socket socket, Disconnect disconnect) {
            this.disconnect = disconnect;
            connection = new NetworkConnection(socket, null, null) {
                HandlePacket = HandlePacket,
                OnDisconnect = Disconnected,
                ParsePacket = Packet.Parse,
                ConnectionName = "Proxy Client"
            };
            connection.StartReceive();
        }

        public void Dispose() {
            /* If proxy hasn't disconnected yet, dispose proxy. */

            if (disconnected) return;
            disconnected = true;

            connection.Dispose();
        }

        public void HandlePacket(Packet packet) {
            /* Sends the received packet to the leader MultiBot. */
        }

        public void Disconnected() {
            /* If proxy hasn't disconnected yet, call disconnect action. */

            if (disconnected) return;
            disconnect();
        }
    }
}