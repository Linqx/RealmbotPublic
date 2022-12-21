using System.Xml.Linq;

namespace BotDataExtracter {
    public class PacketData {
        public string PacketId;
        public byte PacketType;

        public PacketData(string packetId, byte packetType) {
            PacketId = packetId;
            PacketType = packetType;
        }

        public XElement ToXml() {
            return new XElement("Packet",
                new XAttribute("type", PacketType),
                PacketId // Value of Element
            );
        }
    }
}