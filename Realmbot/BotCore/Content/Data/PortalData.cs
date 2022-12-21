using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class PortalData : StaticObjectData {

        public override string Class => "Portal, GuildHallPortal";
        public bool IntergamePortal;
        public string DungeonName;
        public bool NexusPortal;

        public override void Parse(XElement elem) {
            base.Parse(elem);

            IntergamePortal = ContentUtils.HasElement(elem, "IntergamePortal");
            DungeonName = ContentUtils.GetElement(elem, "DungeonName", null);
            NexusPortal = ContentUtils.HasElement(elem, "NexusPortal");
        }
    }
}