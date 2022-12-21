using System.Linq;
using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class ProjectileData : BasicObjectData {
        public override string Class => "Projectile";
    }

    public class ProjectileProperties {
        public byte Id;
        public string ObjectId;
        public int MinDamage;
        public int MaxDamage;
        public double Speed;
        public double LifetimeMS;
        public bool MultiHit;
        public bool PassesCover;
        public bool ArmorPiercing;
        public bool Wavy;
        public bool Parametric;
        public bool Boomerang;
        public double Amplitude;
        public double Frequency;
        public double Magnitude;
        public ConditionEffectData[] Effects;
        public bool HasConditions;
        public double MaxProjectileTravel;

        public ProjectileProperties Parse(XElement elem) {
            Id = byte.Parse(ContentUtils.GetAttribute(elem, "id", "0"));
            ObjectId = ContentUtils.GetElement(elem, "ObjectId", null);
            if (ContentUtils.HasElement(elem, "Damage")) {
                MinDamage = MaxDamage = int.Parse(ContentUtils.GetElement(elem, "Damage", "0"));
            }
            else {
                if (ContentUtils.HasElement(elem, "MinDamage"))
                    MinDamage = int.Parse(ContentUtils.GetElement(elem, "MinDamage", "0"));
                if (ContentUtils.HasElement(elem, "MaxDamage"))
                    MaxDamage = int.Parse(ContentUtils.GetElement(elem, "MaxDamage", "0"));
            }

            Speed = double.Parse(ContentUtils.GetElement(elem, "Speed", "100"));
            LifetimeMS = double.Parse(ContentUtils.GetElement(elem, "LifetimeMS", "1000"));
            MultiHit = ContentUtils.HasElement(elem, "MultiHit");
            PassesCover = ContentUtils.HasElement(elem, "PassesCover");
            ArmorPiercing = ContentUtils.HasElement(elem, "ArmorPiercing");
            Wavy = ContentUtils.HasElement(elem, "Wavy");
            Parametric = ContentUtils.HasElement(elem, "Parametric");
            Boomerang = ContentUtils.HasElement(elem, "Boomerang");
            Amplitude = double.Parse(ContentUtils.GetElement(elem, "Amplitude", "0"));
            Frequency = double.Parse(ContentUtils.GetElement(elem, "Frequency", "1"));
            Magnitude = double.Parse(ContentUtils.GetElement(elem, "Magnitude", "3"));
            Effects = elem.Elements("ConditionEffect").Select(_ => new ConditionEffectData().Parse(_)).ToArray();
            HasConditions = Effects.Length > 0;
            MaxProjectileTravel = Speed * LifetimeMS;
            return this;
        }
    }
}