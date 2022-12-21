using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class CharacterData : GameObjectData {

        public override string Class => "Character";
        public Dictionary<byte, ProjectileProperties> Projectiles;
        public double? Exp;

        public override void Parse(XElement elem) {
            base.Parse(elem);

            Projectiles = ContentUtils.HasElement(elem, "Projectile")
                ? elem.Elements("Projectiles").Select(_ => new ProjectileProperties().Parse(_)).ToDictionary(_ => _.Id)
                : null;
            Exp = ContentUtils.HasElement(elem, "Exp")
                ? double.Parse(ContentUtils.GetElement(elem, "Exp", "0"))
                : (double?) null;
        }
    }
}