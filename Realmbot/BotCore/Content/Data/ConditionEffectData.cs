using System;
using System.Xml.Linq;
using BotTools;

namespace Botcore.Data {
    public class ConditionEffectData {
        public ConditionEffects Effect;
        public double Duration;
        public byte Target;

        public ConditionEffectData Parse(XElement elem) {
            string eff = elem.Value;

            if (!ParseEnum.TryParse(eff, out ConditionEffects cond)) {
                Logger.Log("Content", $"Unkown Condition Effect: {eff}", ConsoleColor.Red);
                return null;
            }

            Effect = cond;
            Duration = double.Parse(ContentUtils.GetAttribute(elem, "duration", "1"));
            Target = byte.Parse(ContentUtils.GetAttribute(elem, "target", "0"));
            return this;
        }
    }

    public class ConditionEffectsConsts {
        public const int NEW_CON_THRESHOLD = 32;
    }

    public enum ConditionEffects {
        [ParseableName("Nothing")] NOTHING = 0,
        [ParseableName("Dead")] DEAD = 1,
        [ParseableName("Quiet")] QUIET = 2,
        [ParseableName("Weak")] WEAK = 3,
        [ParseableName("Slowed")] SLOWED = 4,
        [ParseableName("Sick")] SICK = 5,
        [ParseableName("Dazed")] DAZED = 6,
        [ParseableName("Stunned")] STUNNED = 7,
        [ParseableName("Blind")] BLIND = 8,
        [ParseableName("Hallucinating")] HALLUCINATING = 9,
        [ParseableName("Drunk")] DRUNK = 10,
        [ParseableName("Confused")] CONFUSED = 11,
        [ParseableName("Stun Immune")] STUN_IMMUNE = 12,
        [ParseableName("Invisible")] INVISIBLE = 13,
        [ParseableName("Paralyzed")] PARALYZED = 14,
        [ParseableName("Speedy")] SPEEDY = 15,
        [ParseableName("Bleeding")] BLEEDING = 16,
        [ParseableName("Armor Broken Immune")] ARMORBROKENIMMUNE = 17,
        [ParseableName("Healing")] HEALING = 18,
        [ParseableName("Damaging")] DAMAGING = 19,
        [ParseableName("Berserk")] BERSERK = 20,
        [ParseableName("Paused")] PAUSED = 21,
        [ParseableName("Stasis")] STASIS = 22,
        [ParseableName("Stasis Immune")] STASIS_IMMUNE = 23,
        [ParseableName("Invincible")] INVINCIBLE = 24,
        [ParseableName("Invulnerable")] INVULNERABLE = 25,
        [ParseableName("Armored")] ARMORED = 26,
        [ParseableName("Armor Broken")] ARMORBROKEN = 27,
        [ParseableName("Hexed")] HEXED = 28,
        [ParseableName("Ninja Speedy")] NINJA_SPEEDY = 29,
        [ParseableName("Unstable")] UNSTABLE = 30,
        [ParseableName("Darkness")] DARKNESS = 31,
        [ParseableName("Slowed Immune")] SLOWED_IMMUNE = 32,
        [ParseableName("Dazed Immune")] DAZED_IMMUNE = 33,
        [ParseableName("Paralyzed Immune")] PARALYZED_IMMUNE = 34,
        [ParseableName("Petrified")] PETRIFIED = 35,
        [ParseableName("Petrified Immune")] PETRIFIED_IMMUNE = 36,
        [ParseableName("Pet Effect Icon")] PET_EFFECT_ICON = 37,
        [ParseableName("Curse")] CURSE = 38,
        [ParseableName("Curse Immune")] CURSE_IMMUNE = 39,
        [ParseableName("HP Boost")] HP_BOOST = 40,
        [ParseableName("MP Boost")] MP_BOOST = 41,
        [ParseableName("ATT Boost")] ATT_BOOST = 42,
        [ParseableName("DEF Boost")] DEF_BOOST = 43,
        [ParseableName("SPD Boost")] SPD_BOOST = 44,
        [ParseableName("VIT Boost")] VIT_BOOST = 45,
        [ParseableName("WIS Boost")] WIS_BOOST = 46,
        [ParseableName("DEX Boost")] DEX_BOOST = 47,
        [ParseableName("Silenced")] SILENCED = 48,
        [ParseableName("Exposed")] EXPOSED = 49,
        [ParseableName("Ground Damage")] GROUNDDAMAGE = 99
    }

    [Flags]
    public enum ConditionEffectsBits {
        DEAD_BIT = 1 << (ConditionEffects.DEAD - 1),
        QUIET_BIT = 1 << (ConditionEffects.QUIET - 1),
        WEAK_BIT = 1 << (ConditionEffects.WEAK - 1),
        SLOWED_BIT = 1 << (ConditionEffects.SLOWED - 1),
        SICK_BIT = 1 << (ConditionEffects.SICK - 1),
        DAZED_BIT = 1 << (ConditionEffects.DAZED - 1),
        STUNNED_BIT = 1 << (ConditionEffects.STUNNED - 1),
        BLIND_BIT = 1 << (ConditionEffects.BLIND - 1),
        HALLUCINATING_BIT = 1 << (ConditionEffects.HALLUCINATING - 1),
        DRUNK_BIT = 1 << (ConditionEffects.DRUNK - 1),
        CONFUSED_BIT = 1 << (ConditionEffects.CONFUSED - 1),
        STUN_IMMUNE_BIT = 1 << (ConditionEffects.STUN_IMMUNE - 1),
        INVISIBLE_BIT = 1 << (ConditionEffects.INVINCIBLE - 1),
        PARALYZED_BIT = 1 << (ConditionEffects.PARALYZED - 1),
        SPEEDY_BIT = 1 << (ConditionEffects.SPEEDY - 1),
        BLEEDING_BIT = 1 << (ConditionEffects.BLEEDING - 1),
        ARMOR_BROKEN_IMMUNE_BIT = 1 << (ConditionEffects.ARMORBROKENIMMUNE - 1),
        HEALING_BIT = 1 << (ConditionEffects.HEALING - 1),
        DAMAGING_BIT = 1 << (ConditionEffects.DAMAGING - 1),
        BERSERK_BIT = 1 << (ConditionEffects.BERSERK - 1),
        PAUSE_BIT = 1 << (ConditionEffects.PAUSED - 1),
        STASIS_BIT = 1 << (ConditionEffects.STASIS - 1),
        INVINCIBLE_BIT = 1 << (ConditionEffects.INVINCIBLE - 1),
        INVULNERABLE_BIT = 1 << (ConditionEffects.INVULNERABLE - 1),
        ARMORED_BIT = 1 << (ConditionEffects.ARMORED - 1),
        ARMOR_BROKEN_BIT = 1 << (ConditionEffects.ARMORBROKEN - 1),
        HEXED_BIT = 1 << (ConditionEffects.HEXED - 1),
        NINJA_SPEEDY_BIT = 1 << (ConditionEffects.NINJA_SPEEDY - 1),
        UNSTABLE_BIT = 1 << (ConditionEffects.UNSTABLE - 1),
        DARKNESS_BIT = 1 << (ConditionEffects.DARKNESS - 1)
    }

    [Flags]
    public enum ConditionEffectsBits2 {
        SLOWED_IMMUNE_BIT = 1 << (ConditionEffects.SLOWED_IMMUNE - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        DAZED_IMMUNE_BIT = 1 << (ConditionEffects.DAZED_IMMUNE - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        PARALYZED_IMMUNE_BIT = 1 << (ConditionEffects.PARALYZED_IMMUNE - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        PETRIFIED_BIT = 1 << (ConditionEffects.PETRIFIED - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        PETRIFIED_IMMUNE_BIT = 1 << (ConditionEffects.PETRIFIED_IMMUNE - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        PET_EFFECT_ICON_BIT = 1 << (ConditionEffects.PET_EFFECT_ICON - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        CURSE_BIT = 1 << (ConditionEffects.CURSE - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        CURSE_IMMUNE_BIT = 1 << (ConditionEffects.CURSE_IMMUNE - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        HP_BOOST_BIT = 1 << (ConditionEffects.HP_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        MP_BOOST_BIT = 1 << (ConditionEffects.MP_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        ATT_BOOST_BIT = 1 << (ConditionEffects.ATT_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        DEF_BOOST_BIT = 1 << (ConditionEffects.DEF_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        SPD_BOOST_BIT = 1 << (ConditionEffects.SPD_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        VIT_BOOST_BIT = 1 << (ConditionEffects.VIT_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        WIS_BOOST_BIT = 1 << (ConditionEffects.WIS_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        DEX_BOOST_BIT = 1 << (ConditionEffects.DEX_BOOST - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        SILENCED_BIT = 1 << (ConditionEffects.SILENCED - ConditionEffectsConsts.NEW_CON_THRESHOLD),
        EXPOSED_BIT = 1 << (ConditionEffects.EXPOSED - ConditionEffectsConsts.NEW_CON_THRESHOLD)
    }
}