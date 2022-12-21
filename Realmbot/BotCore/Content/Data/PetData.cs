using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class PetData : GameObjectData {

        public override string Class => "Pet";
        public PetFamily Family;
        public PetRarity Rarity;
        public PetAbility FirstAbility;

        public override void Parse(XElement elem) {
            base.Parse(elem);

            Family = ParseEnum.Parse<PetFamily>(ContentUtils.GetElement(elem, "Family", "? ? ? ?"));
            Rarity = ParseEnum.Parse<PetRarity>(ContentUtils.GetElement(elem, "Rarity", "Common"));
        }
    }

    public enum PetRarity {
        Common,
        Uncommon,
        Rare,
        Legendary,
        Divine
    }

    public enum PetFamily {
        Aquatic,
        Automaton,
        Avian,
        Canine,
        Exotic,
        Farm,
        Feline,
        Humanoid,
        Insect,
        Penguin,
        Reptile,
        Spooky,
        [ParseableName("? ? ? ?")] Question,
        Woodland
    }

    public enum PetAbility {
        [ParseableName("Attack Close")] AttackClose = 402,
        [ParseableName("Attack Mid")] AttackMid = 404,
        [ParseableName("Attack Far")] AttackFar = 405,
        Electric = 406,
        Heal = 407,
        MagicHeal = 408,
        Savage = 409,
        Decoy = 410,
        [ParseableName("Rising Fury")] RisingFury = 411
    }
}