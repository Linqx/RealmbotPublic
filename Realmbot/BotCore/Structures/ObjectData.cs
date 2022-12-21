using Shio;

namespace BotCore.Structures {
    public class ObjectData {
        public ushort Type { get; set; }
        public ObjectStatusData Status { get; set; }

        public void Read(PacketReader rdr) {
            Type = rdr.ReadUInt16();
            Status = new ObjectStatusData();
            Status.Read(rdr);
        }
    }
}