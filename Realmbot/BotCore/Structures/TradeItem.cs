using Shio;

namespace BotCore.Structures {
    public class TradeItem {
        public int Item { get; set; }
        public int SlotType { get; set; }
        public bool Tradeable { get; set; }
        public bool Include { get; set; }

        public void Read(PacketReader rdr) {
            Item = rdr.ReadInt32();
            SlotType = rdr.ReadInt32();
            Tradeable = rdr.ReadBoolean();
            Include = rdr.ReadBoolean();
        }
    }
}