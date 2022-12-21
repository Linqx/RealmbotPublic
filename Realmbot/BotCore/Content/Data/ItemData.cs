using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BotTools;
using RotMGCore.Structures.Game;

namespace Botcore.Data {
    public class ItemData : BasicObjectData {

        public override string Class => "Equipment";
        public int SlotType;
        public int FeedPower;
        public int Tier;
        public double RateOfFire;
        public bool Usable;
        public int BagType;
        public int MpCost;
        public int FameBonus;
        public int NumProjectiles;
        public double ArcGap;
        public bool Consumable;
        public bool Potion;
        public string DisplayId;
        public string SuccessorId;
        public bool Soulbound;
        public double Cooldown;
        public bool Resurrects;
        public int Texture1;
        public int Texture2;
        public bool IsBackpack;
        public int Doses;
        public bool XpBooster;
        public bool LootDropBooster;
        public bool LootTierBooster;
        public int SetType;
        public int MpEndCost;
        public double? Timer;

        public PetRarity? Rarity;
        public PetFamily? Family;

        public Dictionary<PlayerStatId, int> StatBoosts;
        public ActivateEffectData[] ActivateEffects;
        public ProjectileProperties Projectile;

        public override void Parse(XElement elem) {
            base.Parse(elem);

            SlotType = int.Parse(ContentUtils.GetElement(elem, "SlotType", "10"));
            FeedPower = int.Parse(ContentUtils.GetElement(elem, "feedPower", "0"));
            Tier = int.Parse(ContentUtils.GetElement(elem, "Tier", "-1"));
            RateOfFire = double.Parse(ContentUtils.GetElement(elem, "RateOfFire", "1"));
            Usable = ContentUtils.HasElement(elem, "Usable");
            BagType = int.Parse(ContentUtils.GetElement(elem, "BagType", "0"));
            ArcGap = double.Parse(ContentUtils.GetElement(elem, "ArcGap", "11.25"));
            MpCost = int.Parse(ContentUtils.GetElement(elem, "MpCost", "0"));
            FameBonus = int.Parse(ContentUtils.GetElement(elem, "FameBonus", "0"));
            NumProjectiles = int.Parse(ContentUtils.GetElement(elem, "NumProjectiles", "1"));
            Consumable = ContentUtils.HasElement(elem, "Consumable");
            Potion = ContentUtils.HasElement(elem, "Potion");
            DisplayId = ContentUtils.GetElement(elem, "DisplayId", Id);
            SuccessorId = ContentUtils.GetElement(elem, "SuccessorId", null);
            Soulbound = ContentUtils.HasElement(elem, "Soulbound");
            Cooldown = double.Parse(ContentUtils.GetElement(elem, "Cooldown", "0"));
            Resurrects = ContentUtils.HasElement(elem, "Resurrects");
            Texture1 = ContentUtils.ParseIntHex(ContentUtils.GetElement(elem, "Tex1", "0"));
            Texture2 = ContentUtils.ParseIntHex(ContentUtils.GetElement(elem, "Tex2", "0"));
            IsBackpack = ContentUtils.HasElement(elem, "IsBackpack");
            Doses = int.Parse(ContentUtils.GetElement(elem, "Doses", "0"));
            XpBooster = ContentUtils.HasElement(elem, "XpBoost");
            LootDropBooster = ContentUtils.HasElement(elem, "LDBoosted");
            LootTierBooster = ContentUtils.HasElement(elem, "LTBoosted");
            SetType = ContentUtils.ParseIntHex(ContentUtils.GetAttribute(elem, "setType", "-1"));
            MpEndCost = int.Parse(ContentUtils.GetElement(elem, "MpEndCost", "-1"));
            Timer = ContentUtils.HasElement(elem, "Timer")
                ? double.Parse(ContentUtils.GetElement(elem, "Timer", "-1"))
                : (double?) null;
            StatBoosts = elem.Elements("ActivateOnEquip")
                .ToDictionary(_ => (PlayerStatId) int.Parse(_.Attribute("stat").Value),
                    _ => int.Parse(_.Attribute("amount").Value));
            ActivateEffects = elem.Elements("Activate").Select(_ => new ActivateEffectData().Parse(_)).ToArray();
            Projectile = ContentUtils.HasElement(elem, "Projectile")
                ? new ProjectileProperties().Parse(elem.Element("Projectile"))
                : null;
        }
    }

    public class ActivateEffectData {
        public ActivateEffects Effects;
        public PlayerStatId Stat;
        public int Amount;
        public double Range;
        public double DurationSec; //Used for wis modifier
        public int DurationMS;
        public ConditionEffects? ConditionEffect;
        public int EffectDuration;
        public double MaximumDistance;
        public double Radius;
        public int TotalDamage;
        public string ObjectId;
        public double AngleOffset;
        public int MaxTargets;
        public string Id; //CreatePortal
        public int SkinType;
        public string DungeonName;
        public string LockedName;
        public string Target;
        public string Center;
        public bool UseWisMod;
        public ShowEffectType? VisualEffect;
        public int Color;

        public ActivateEffectData Parse(XElement elem) {
            if (!Enum.TryParse(elem.Value, out Effects))
                Logger.Log("Item Data", $"Failed to parse ActivateEffect: {elem.Value}", ConsoleColor.Red);
            Stat = (PlayerStatId) int.Parse(ContentUtils.GetAttribute(elem, "stat", "-1"));
            Amount = int.Parse(ContentUtils.GetAttribute(elem, "amount", "0"));
            Range = double.Parse(ContentUtils.GetAttribute(elem, "range", "0"));
            DurationSec = double.Parse(ContentUtils.GetAttribute(elem, "duration", "0"));
            DurationMS = (int) (DurationSec * 1000);

            if (ContentUtils.HasAttribute(elem, "effect")) {
                string effect = ContentUtils.GetAttribute(elem, "effect", "Nothing");
                if (ParseEnum.TryParse(effect, out ConditionEffects cond))
                    ConditionEffect = cond;
            }
            else if (ContentUtils.HasAttribute(elem, "condEffect")) {
                string effect = ContentUtils.GetAttribute(elem, "condEffect", "Nothing");
                if (ParseEnum.TryParse(effect, out ConditionEffects cond))
                    ConditionEffect = cond;
            }
            else {
                ConditionEffect = null;
            }

            EffectDuration = (int) (double.Parse(ContentUtils.GetAttribute(elem, "condDuration", "0")) * 1000);
            MaximumDistance = double.Parse(ContentUtils.GetAttribute(elem, "maxDistance", "13"));
            Radius = float.Parse(ContentUtils.GetAttribute(elem, "radius", "0"));
            TotalDamage = int.Parse(ContentUtils.GetAttribute(elem, "totalDamage", "0"));
            ObjectId = ContentUtils.GetAttribute(elem, "objectId", null);
            AngleOffset = double.Parse(ContentUtils.GetAttribute(elem, "angleOffset", "0"));
            MaxTargets = int.Parse(ContentUtils.GetAttribute(elem, "maxTargets", "0"));
            ;
            Id = ContentUtils.GetAttribute(elem, "id", null);
            SkinType = int.Parse(ContentUtils.GetAttribute(elem, "skinType", "0"));
            DungeonName = ContentUtils.GetAttribute(elem, "dungeonName", null);
            LockedName = ContentUtils.GetAttribute(elem, "lockedName", null);
            Target = ContentUtils.GetAttribute(elem, "target", null);
            Center = ContentUtils.GetAttribute(elem, "center", null);
            UseWisMod = bool.Parse(ContentUtils.GetAttribute(elem, "UseWisMod", "false"));
            VisualEffect = ContentUtils.HasAttribute(elem, "visualEffect")
                ? (ShowEffectType) int.Parse(ContentUtils.GetAttribute(elem, "visualEffect", "0"))
                : 0;
            Color = ContentUtils.ParseIntHex(ContentUtils.GetAttribute(elem, "color", "0"));
            return this;
        }
    }

    public enum ActivateEffects {
        None,
        Shoot,
        StatBoostSelf,
        StatBoostAura,
        BulletNova,
        ConditionEffectAura,
        ConditionEffectSelf,
        Heal,
        HealNova,
        Magic,
        MagicNova,
        Teleport,
        VampireBlast,
        Trap,
        StasisBlast,
        Decoy,
        Lightning,
        PoisonGrenade,
        RemoveNegativeConditions,
        RemoveNegativeConditionsSelf,
        IncrementStat,
        Pet,
        PermaPet,
        Create,
        UnlockPortal,
        DazeBlast,
        ClearConditionEffectAura,
        ClearConditionEffectSelf,
        Dye,
        CreatePet,
        ShurikenAbility,
        UnlockSkin,
        MysteryPortal,
        GenericActivate,
        CreatePortal,
        PetSkin,
        Unlock,
        MysteryDyes,
        TeleportToObject,
        KillRealmHeroes,
        Exchange,
        BulletCreate,
        ObjectToss,
        UnlockPetSkin,
        LevelTwenty,
        GroupTransform,
        GrantSupporterPoints,
        LineAoE,
        TeleportLimit,
        ChangeObject,
        MarkAndTeleport
    }
}