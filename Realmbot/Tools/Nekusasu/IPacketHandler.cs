using Networking.TCP;

namespace Networking {
    public interface IPacketHandler<TConnection, TPacket>
        where TPacket : Packet
        where TConnection : NetConnection<TPacket> {
        byte Id { get; }

        void Handle(TPacket packet, TConnection connection);
    }
}