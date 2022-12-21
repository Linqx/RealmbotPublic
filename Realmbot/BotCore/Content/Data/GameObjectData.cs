using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class GameObjectData : BasicObjectData {

        public override string Class => "GameObject";
        public string DisplayId;
        public string Group;
        public int MinSize;
        public int MaxSize;
        public int SizeStep;

        public int MaxHealth;
        public int Defense;

        public bool Enemy;
        public bool Player;
        public bool Static;
        public bool Quest;
        public bool God;
        public bool Hero;
        public bool Cube;
        public bool Oryx;

        public bool StunImmune;
        public bool ArmorBrokenImmune;
        public bool StasisImmune;
        public bool SlowedImmune;
        public bool DazedImmune;
        public bool ParalyzeImmune;
        public bool PetrifiedImmune;
        public bool CurseImmune;

        public override void Parse(XElement elem) {
            base.Parse(elem);

            DisplayId = ContentUtils.GetElement(elem, "DisplayId", Id);
            Group = ContentUtils.GetElement(elem, "Group", null);

            if (ContentUtils.HasElement(elem, "Size")) {
                MinSize = MaxSize = int.Parse(ContentUtils.GetElement(elem, "Size", "100"));
                SizeStep = 0;
            }
            else {
                MinSize = int.Parse(ContentUtils.GetElement(elem, "MinSize", "100"));
                MaxSize = int.Parse(ContentUtils.GetElement(elem, "MaxSize", "100"));
                SizeStep = int.Parse(ContentUtils.GetElement(elem, "SizeStep", "0"));
            }

            MaxHealth = int.Parse(ContentUtils.GetElement(elem, "MaxHitPoints", "200"));
            Defense = int.Parse(ContentUtils.GetElement(elem, "Defense", "0"));

            Enemy = ContentUtils.HasElement(elem, "Enemy");
            Player = ContentUtils.HasElement(elem, "Player");
            Static = ContentUtils.HasElement(elem, "Static");
            Quest = ContentUtils.HasElement(elem, "Quest");
            Cube = ContentUtils.HasElement(elem, "Cube");
            Oryx = ContentUtils.HasElement(elem, "Oryx");

            StunImmune = ContentUtils.HasElement(elem, "StunImmune");
            ArmorBrokenImmune = ContentUtils.HasElement(elem, "ArmorBrokenImmune");
            StasisImmune = ContentUtils.HasElement(elem, "StasisImmune");
            SlowedImmune = ContentUtils.HasElement(elem, "SlowedImmune");
            DazedImmune = ContentUtils.HasElement(elem, "DazedImmune");
            ParalyzeImmune = ContentUtils.HasElement(elem, "ParalyzeImmune");
            PetrifiedImmune = ContentUtils.HasElement(elem, "PetrifiedImmune");
            CurseImmune = ContentUtils.HasElement(elem, "CurseImmune");
        }
    }
}