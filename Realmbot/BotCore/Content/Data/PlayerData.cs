using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class PlayerData : CharacterData {
        public override string Class => "Player";
        public int[] SlotTypes;
        public PlayerStats Stats;
        public Dictionary<PlayerStatId, LevelIncrease> LevelIncreases;

        public int[] Equipment;

        public override void Parse(XElement elem) {
            base.Parse(elem);

            SlotTypes = ContentUtils.ParseIntArray(ContentUtils.GetElement(elem, "SlotTypes",
                "0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0"));
            Stats = new PlayerStats().Parse(elem);
            LevelIncreases = elem.Elements("LevelIncrease")
                .ToDictionary(_ => StatsNameToId(_.Value), _ => new LevelIncrease(_));

            //There were only 19 different equipment slots in the xml (when there should be 20).
            //Filled array with 20 -1s then placed parsed element on top of array.
            Equipment = new int[20].Select(_ => _ = -1).ToArray();
            int[] parsedEquips = ContentUtils.ParseIntArray(ContentUtils.GetElement(elem, "Equipment",
                "-1, -1, -1, -1, -1, -1, -1, -1 -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1"));
            for (int i = 0; i < parsedEquips.Length; i++)
                Equipment[i] = parsedEquips[i];

            //All characters unlockable from start, didn't bother parsing unlock costs and unlock levels
        }

        public static PlayerStatId StatsNameToId(string name) {
            switch (name) {
                case "MaxHitPoints":
                    return PlayerStatId.MAX_HIT_POINTS;
                case "MaxMagicPoints":
                    return PlayerStatId.MAX_MANA_POINTS;
                case "Attack":
                    return PlayerStatId.ATTACK;
                case "Defense":
                    return PlayerStatId.DEFENSE;
                case "Speed":
                    return PlayerStatId.SPEED;
                case "Dexterity":
                    return PlayerStatId.DEXTERITY;
                case "HpRegen":
                    return PlayerStatId.VITALITY;
                case "MpRegen":
                    return PlayerStatId.WISDOM;
            }

            return PlayerStatId.NONE;
        }
    }

    public class PlayerStats {
        public PlayerStat Life;
        public PlayerStat Mana;
        public PlayerStat Attack;
        public PlayerStat Defense;
        public PlayerStat Speed;
        public PlayerStat Dexterity;
        public PlayerStat Vitality;
        public PlayerStat Wisdom;

        public Dictionary<PlayerStatId, PlayerStat> IdToPlayerStatPair;

        public PlayerStats Parse(XElement elem) {
            Life = new PlayerStat(elem.Element("MaxHitPoints"));
            Mana = new PlayerStat(elem.Element("MaxMagicPoints"));
            Attack = new PlayerStat(elem.Element("Attack"));
            Defense = new PlayerStat(elem.Element("Defense"));
            Speed = new PlayerStat(elem.Element("Speed"));
            Dexterity = new PlayerStat(elem.Element("Dexterity"));
            Vitality = new PlayerStat(elem.Element("HpRegen"));
            Wisdom = new PlayerStat(elem.Element("MpRegen"));

            IdToPlayerStatPair = new Dictionary<PlayerStatId, PlayerStat>();
            IdToPlayerStatPair[PlayerStatId.MAX_HIT_POINTS] = Life;
            IdToPlayerStatPair[PlayerStatId.MAX_MANA_POINTS] = Mana;
            IdToPlayerStatPair[PlayerStatId.ATTACK] = Attack;
            IdToPlayerStatPair[PlayerStatId.DEFENSE] = Defense;
            IdToPlayerStatPair[PlayerStatId.SPEED] = Speed;
            IdToPlayerStatPair[PlayerStatId.DEXTERITY] = Dexterity;
            IdToPlayerStatPair[PlayerStatId.VITALITY] = Vitality;
            IdToPlayerStatPair[PlayerStatId.WISDOM] = Wisdom;
            return this;
        }
    }

    public class PlayerStat {
        public int Default;
        public int Max;

        public PlayerStat(XElement elem) {
            Default = int.Parse(elem.Value);
            Max = int.Parse(ContentUtils.GetAttribute(elem, "max", null));
        }
    }

    public class LevelIncrease {
        public int Min;
        public int Max;

        public LevelIncrease(XElement elem) {
            Min = int.Parse(ContentUtils.GetAttribute(elem, "min", "0"));
            Max = int.Parse(ContentUtils.GetAttribute(elem, "max", "0"));
        }
    }

    public enum PlayerStatId {
        NONE = -1,
        MAX_HIT_POINTS = 0,
        MAX_MANA_POINTS = 3,
        ATTACK = 20,
        DEFENSE = 21,
        SPEED = 22,
        VITALITY = 26,
        WISDOM = 27,
        DEXTERITY = 28
    }
}