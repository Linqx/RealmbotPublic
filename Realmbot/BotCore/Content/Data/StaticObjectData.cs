using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class StaticObjectData : GameObjectData {
        public bool OccupySquare;
        public bool EnemyOccupySquare;
        public bool FullOccupy;
        public bool BlocksSight;

        public override void Parse(XElement elem) {
            base.Parse(elem);

            OccupySquare = ContentUtils.HasElement(elem, "OccupySquare");
            EnemyOccupySquare = ContentUtils.HasElement(elem, "EnemyOccupySquare");
            FullOccupy = ContentUtils.HasElement(elem, "FullOccupy");
            BlocksSight = ContentUtils.HasElement(elem, "BlocksSight");
            Static = true;
        }
    }
}