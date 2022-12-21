using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class TileData {
        public ushort Type;
        public string Id;
        public bool NoWalk;
        public bool Damaging;
        public int MinDamage;
        public int MaxDamage;
        public float Speed;
        public bool Push;

        public void Parse(XElement elem) {
            Type = ContentUtils.ParseType(ContentUtils.GetAttribute(elem, "type", null));
            Id = ContentUtils.GetAttribute(elem, "id", null);
            NoWalk = ContentUtils.HasElement(elem, "NoWalk");

            string speedValue = ContentUtils.GetElement(elem, "Speed", "1.0");
            Speed = float.Parse(speedValue.StartsWith(".") ? "0" + speedValue : speedValue);

            Push = ContentUtils.HasElement(elem, "Push");

            if (ContentUtils.HasElement(elem, "MinDamage")) {
                MinDamage = int.Parse(ContentUtils.GetElement(elem, "MinDamage", null));
                Damaging = true;
            }

            if (ContentUtils.HasElement(elem, "MaxDamage")) {
                MaxDamage = int.Parse(ContentUtils.GetElement(elem, "MaxDamage", null));
                Damaging = true;
            }
        }
    }
}