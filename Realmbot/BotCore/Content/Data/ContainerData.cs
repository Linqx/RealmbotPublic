using System.Xml.Linq;
using Botcore.Data;
using BotTools;

namespace BotCore.Content.Data {
    public class ContainerData : GameObjectData {

        public override string Class => "Container";
        public int[] SlotTypes { get; set; }

        public override void Parse(XElement elem) {
            base.Parse(elem);

            if (ContentUtils.HasElement(elem, "SlotTypes"))
                SlotTypes = ContentUtils.ParseIntArray(ContentUtils.GetElement(elem, "SlotTypes",
                    "0, 0, 0, 0, 0, 0, 0, 0"));
        }
    }
}