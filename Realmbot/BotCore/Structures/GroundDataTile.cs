using Shio;

namespace BotCore.Structures {
    public class GroundTileData {
        public int X { get; set; }
        public int Y { get; set; }
        public ushort Type { get; set; }

        public void Read(PacketReader rdr) {
            X = rdr.ReadInt16();
            Y = rdr.ReadInt16();
            Type = rdr.ReadUInt16();
        }
    }
}