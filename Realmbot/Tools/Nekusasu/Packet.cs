using Stream;

namespace Networking {
    public abstract class Packet {
        public abstract byte Id { get; }

        public void WritePacket(BitWriter w) {
            Write(w);
        }

        public void WritePacketNetworkOrder(PacketWriter w) {
            WriteNetworkOrder(w);
        }

        public void ReadPacket(BitReader r) {
            Read(r);
        }

        public void ReadPacketNetworkOrder(PacketReader r) {
            ReadNetworkOrder(r);
        }

        public void WriteTokenPacket(BitWriter w) {
            /* Token packets are only supported via host order (little endian). */
            w.Write(Id);
            if (this is ITokenPacket token) {
                w.Write(token.Token);
                w.Write(token.TokenResponse);
            }

            Write(w);
        }

        public void ReadTokenPacket(BitReader r) {
            /* Token packets are only supported via host order (little endian). */
            if (this is ITokenPacket token) {
                token.Token = r.ReadInt32();
                token.TokenResponse = r.ReadBool();
            }

            Read(r);
        }

        protected abstract void Write(BitWriter w);
        protected abstract void WriteNetworkOrder(PacketWriter w);

        protected abstract void Read(BitReader r);
        protected abstract void ReadNetworkOrder(PacketReader r);
    }
}