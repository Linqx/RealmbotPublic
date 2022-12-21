using System.Collections.Generic;
using Bookkeeping;

namespace Networking {
    public class PacketHandler<TPacket>
        where TPacket : Packet {
        public delegate void HandlePacket(TPacket packet);

        public bool LogError = true;
        private readonly Dictionary<int, HandlePacket> handlers;

        public PacketHandler() {
            handlers = new Dictionary<int, HandlePacket>();
        }

        public void Handle(TPacket packet) {
            byte id = packet.Id;
            if (!handlers.ContainsKey(id)) {
                if (LogError) Log.Error($"[PacketFactory<{packet.GetType().Name}>] Handler not found for packet id: {packet.Id}");
                return;
            }

            HandlePacket handler = handlers[id];
            handler.Invoke(packet);
        }

        public void Map(int id, HandlePacket handler) {
            handlers[id] = handler;
        }

        public void Unmap(int id) {
            if (handlers.ContainsKey(id))
                handlers.Remove(id);
        }

        public void Dispose() {
            handlers.Clear();
        }
    }
}