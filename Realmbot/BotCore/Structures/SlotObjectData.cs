using Shio;

namespace BotCore.Structures {
    public class SlotObjectData {
        public int ObjectId { get; set; }
        public byte SlotId { get; set; }
        public int ObjectType { get; set; }

        public void Write(PacketWriter wtr) {
            wtr.Write(ObjectId);
            wtr.Write(SlotId);
            wtr.Write(ObjectType);
        }
    }
}