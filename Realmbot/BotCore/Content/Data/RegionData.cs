using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class RegionData {
        public ushort Type;
        public string Id;

        public void Parse(XElement element) {
            Type = ContentUtils.ParseType(ContentUtils.GetAttribute(element, "type", null));
            Id = ContentUtils.GetAttribute(element, "id", null);
        }
    }
}