using Nekusasu;
using Nekusasu.TCP;
using Rekishi;

namespace BotCore.Networking {
    public class RotMGClient : NetConnection<RotMGPacket> {
        private readonly RealmBot _bot;
        private readonly PacketHandler<RotMGPacket> _packetHandlers;
        private readonly PacketHandler<RotMGPacket> _subscribedHandlers;

        public RotMGClient(RealmBot bot) {
            _bot = bot;
            _packetHandlers = new PacketHandler<RotMGPacket>();
            _subscribedHandlers = new PacketHandler<RotMGPacket>();
            _subscribedHandlers.LogError = false;

            Rc4EncryptKey = "6a39570cc9de4ec71d64821894";
            Rc4DecryptKey = "c79332b197f92ba85ed281a023";
        }

        protected override void OnDisconnect() {
        }

        protected override void HandlePacket(RotMGPacket packet) {
            _bot.QueuePacket(packet);
        }

        public void Handle(RotMGPacket packet) {
            _packetHandlers.Handle(packet);
            _subscribedHandlers.Handle(packet);
            _bot.HandleFromState?.Invoke(packet);
        }

        public void Map(PacketType id, PacketHandler<RotMGPacket>.HandlePacket handler) {
            _packetHandlers.Map(RotMGPacket.PacketIds[id], handler);
        }

        public void Unmap(PacketType id) {
            _packetHandlers.Unmap(RotMGPacket.PacketIds[id]);
            _subscribedHandlers.Unmap(RotMGPacket.PacketIds[id]);
        }

        public void Subscribe(PacketType id, PacketHandler<RotMGPacket>.HandlePacket handler) {
            _subscribedHandlers.Map(RotMGPacket.PacketIds[id], handler);
        }
    }
}