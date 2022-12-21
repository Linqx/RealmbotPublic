using System;
using System.Collections.Generic;
using Botcore.Content;
using BotCore.Content.Data;
using Botcore.Data;
using BotCore.Structures;
using BotCore.Structures.Static;
using RotMGCore.Structures.Game;

namespace BotCore.Game.Worlds.Entities {
    public class GameObject : BasicObject {
        /// <summary>
        /// Bit-field for ConditionEffects with an id less than 32.
        /// </summary>
        public int ConditionEffectBits1;

        /// <summary>
        /// Bit-field for ConditionEffects with an id greater than 31.
        /// </summary>
        public int ConditionEffectBits2;

        /// <summary>
        /// The name of a GameObject;
        /// </summary>
        public string Name;

        /// <summary>
        /// The max amount of health a GameObject has.
        /// </summary>
        public int MaxHealth;

        /// <summary>
        /// The amount of health a GameObject has.
        /// </summary>
        public int Health;

        /// <summary>
        /// The amount of defense a GameObject has.
        /// </summary>
        public int Defense;

        /// <summary>
        /// All Xml related data is contained here.
        /// </summary>
        public GameObjectData Data;

        public bool Static;

        public WorldPosData TickPosition; // Server's exact position
        public WorldPosData PosAtTick; // Client's interprolated position
        public WorldPosData MoveVector;

        public bool IsPaused;
        public bool IsStasis;
        public bool IsInvincible;
        public bool IsInvulnerble;
        public bool IsArmored;
        public bool IsArmorBroken;
        public bool IsArmorBrokenImmune;
        public bool IsStunned;
        public bool IsStunImmune;
        public bool IsSlowedImmune;
        public bool IsShowPetEffectIcon;
        public bool IsParalyzed;
        public bool IsParalyzeImmune;
        public bool IsDazedImmune;
        public bool IsPetrified;
        public bool IsPetrifiedImmune;
        public bool IsCursed;
        public bool IsCursedImmune;
        public bool IsExposed;
        public bool IsDazed;

        public bool Ignored;
        public bool Excepted;
        public bool Vulnerable = true;
        public bool Dead;
        public bool Boss;
        public bool Jittery;

        public bool ShouldUpdateVulnerable;

        private int _lastTickId;
        private int _lastTickUpdateTime;

        public GameObject(GameObjectData data) {
            Data = data;
            ShouldUpdateVulnerable = Data.Enemy || Data.Player;
        }

        public static GameObject Resolve(GameObjectData data) {
            switch (data) {
                case CharacterData charData:
                    switch (charData) {
                        case PlayerData playerData:
                            return new Player(playerData);
                        default:
                            return new Character(charData);
                    }

                case StaticObjectData staticObjectData:
                    switch (staticObjectData) {
                        case PortalData portalData:
                            return new Portal(portalData);
                        default:
                            GameObject go = new GameObject(staticObjectData) {
                                Static = true
                            };
                            return go;
                    }

                case ContainerData containerData:
                    if (containerData == GameContent.GetGameObjectData("Vault Chest"))
                        return new Vault(containerData);
                    else
                        return new Container(containerData);

                case PetData petData:
                    return new Pet(petData);

                default:
                    return new GameObject(data);
            }
        }

        public override bool AddTo(World world, double x, double y) {
            World = world;
            WorldPosData pos = new WorldPosData(x, y);
            PosAtTick = TickPosition = pos;

            if (!MoveToFirst(x, y)) {
                World = null;
                return false;
            }

            return true;
        }

        public bool MoveTo(double x, double y) {
            /*
             * Check if tile exists on map, if not fail to move.
             * Set position of object.
             */

            if (!map.TileExists((int) x, (int) y))
                return false;

            X = x;
            Y = y;

            if (LastPosition != MapPosition)
                census.UpdateObjectMap(this, MapPosition.X, MapPosition.Y, LastPosition.X, LastPosition.Y, false);

            //var player = Map.SearchForPlayer("Linqx");

            //if (player != null && ObjectId == player?.ObjectId + 1)
            //    Logger.Log($"GameObject", $"x: {X}, y: {Y}");

            return true;
        }

        public bool MoveToFirst(double x, double y) {
            /*
             * Check if tile exists on map, if not fail to move.
             * Set position of object.
             */

            if (!World.Map.TileExists((int) x, (int) y))
                return false;

            X = x;
            Y = y;

            census.UpdateObjectMap(this, MapPosition.X, MapPosition.Y, 0, 0, true);

            //var player = Map.SearchForPlayer("Linqx");

            //if (player != null && ObjectId == player?.ObjectId + 1)
            //    Logger.Log($"GameObject", $"x: {X}, y: {Y}");

            return true;
        }

        public override bool Update(int time, int dt) {
            /*
             * Check if GameObject should be moving.
             * If moving and tick id is behind than physics simulation, reset move vector and force position.
             * Else, interprolate position correctly.
             */

            if (Math.Abs(MoveVector.X) > 0.001 || Math.Abs(MoveVector.Y) > 0.001) {
                if (_lastTickId < World.LastTickId) {
                    MoveVector.X = 0;
                    MoveVector.Y = 0;
                    MoveTo(TickPosition.X, TickPosition.Y);
                }
                else {
                    int tickDT = time - _lastTickUpdateTime;
                    WorldPosData p = PosAtTick + tickDT * MoveVector;

                    MoveTo(p.X, p.Y);

                    //var linqx = Map.SearchForPlayer("Linqx");
                    //if (linqx != null)
                    //    if (ObjectId == linqx.ObjectId + 1)
                    //        Logger.Log("Pet", $"x: {p.X} y: {p.Y} dt: {tickDT} mX: {MoveVector.X} mY: {MoveVector.Y}");
                }
            }

            return true;
        }

        public void OnTickPosition(WorldPosData pos, int serverDt, int serverTickId) {
            if (_lastTickId < World.LastTickId)
                MoveTo(TickPosition.X, TickPosition.Y);

            _lastTickUpdateTime = World.LastUpdate;
            TickPosition = pos;
            PosAtTick = Position;
            MoveVector = (TickPosition - PosAtTick) / serverDt;
            _lastTickId = serverTickId;
        }

        public void OnGoTo(WorldPosData position, int time) {
            Position = position;
            _lastTickUpdateTime = time;
            TickPosition = position;
            PosAtTick = position;
            MoveVector.X = 0;
            MoveVector.Y = 0;
        }

        public bool HasConditionEffect(ConditionEffectsBits effect) {
            return (ConditionEffectBits1 & (int) effect) != 0;
        }

        public bool HasConditionEffect(ConditionEffectsBits2 effect) {
            return (ConditionEffectBits2 & (int) effect) != 0;
        }

        public virtual void ApplyObjectData(ObjectData data, int serverDt, int serverTickId) {
            ObjectId = data.Status.ObjectId;

            if (ObjectId == World.Bot.ObjectId && World.Player == null) {
                World.Bot.Player = this as Player;
                World.Player = this as Player;
            }

            ApplyStatusData(data.Status, serverDt, serverTickId);
        }

        public virtual void ApplyStatusData(ObjectStatusData data, int serverDt, int serverTickId) {
            if (serverDt != 0 && ObjectId != World.Bot.ObjectId)
                OnTickPosition(data.Position, serverDt, serverTickId);

            int value = 0;
            string strValue = null;

            foreach (KeyValuePair<StatDataType, object> pair in data.Stats) {
                if (StatDataUtils.IsUTF(pair.Key))
                    strValue = (string) pair.Value;
                else
                    value = (int) pair.Value;

                switch (pair.Key) {
                    case StatDataType.MaximumHP:
                        MaxHealth = value;
                        break;
                    case StatDataType.HP:
                        Health = value;
                        break;
                    case StatDataType.Defense:
                        Defense = value;
                        break;
                    case StatDataType.Name:
                        Name = strValue;
                        break;
                    case StatDataType.Effects:
                        if (ConditionEffectBits1 != value) {
                            ConditionEffectBits1 = value;
                            UpdateConditionEffects(true);
                        }

                        break;
                    case StatDataType.Effects2:
                        if (ConditionEffectBits2 != value) {
                            ConditionEffectBits2 = value;
                            UpdateConditionEffects(false);
                        }

                        break;
                }

                value = 0;
                strValue = null;
            }
        }

        public virtual void UpdateConditionEffects(bool first) {
            if (first) {
                IsPaused = HasConditionEffect(ConditionEffectsBits.PAUSE_BIT);
                IsStasis = HasConditionEffect(ConditionEffectsBits.STASIS_BIT);
                IsInvincible = HasConditionEffect(ConditionEffectsBits.INVINCIBLE_BIT);
                IsInvulnerble = HasConditionEffect(ConditionEffectsBits.INVULNERABLE_BIT);
                IsArmored = HasConditionEffect(ConditionEffectsBits.ARMORED_BIT);
                IsArmorBroken = HasConditionEffect(ConditionEffectsBits.ARMOR_BROKEN_BIT);
                IsArmorBrokenImmune = HasConditionEffect(ConditionEffectsBits.ARMOR_BROKEN_IMMUNE_BIT);
                IsDazed = HasConditionEffect(ConditionEffectsBits.DAZED_BIT);
                IsStunned = HasConditionEffect(ConditionEffectsBits.STUNNED_BIT);
                IsStunImmune = HasConditionEffect(ConditionEffectsBits.STUN_IMMUNE_BIT);
                IsParalyzed = HasConditionEffect(ConditionEffectsBits.PARALYZED_BIT);
            }
            else {
                IsSlowedImmune = HasConditionEffect(ConditionEffectsBits2.SLOWED_IMMUNE_BIT);
                IsDazedImmune = HasConditionEffect(ConditionEffectsBits2.DAZED_IMMUNE_BIT);
                IsParalyzeImmune = HasConditionEffect(ConditionEffectsBits2.PARALYZED_IMMUNE_BIT);
                IsPetrified = HasConditionEffect(ConditionEffectsBits2.PETRIFIED_BIT);
                IsPetrifiedImmune = HasConditionEffect(ConditionEffectsBits2.PETRIFIED_IMMUNE_BIT);
                IsShowPetEffectIcon = HasConditionEffect(ConditionEffectsBits2.PET_EFFECT_ICON_BIT);
                IsCursed = HasConditionEffect(ConditionEffectsBits2.CURSE_BIT);
                IsCursedImmune = HasConditionEffect(ConditionEffectsBits2.CURSE_IMMUNE_BIT);
                IsExposed = HasConditionEffect(ConditionEffectsBits2.EXPOSED_BIT);
            }

            if (ShouldUpdateVulnerable)
                UpdateVulnerable(); // Might need to update again when GameObject has died client sided.
        }

        public virtual void UpdateVulnerable() {
            bool vulnerable = false;

            if (Data.Enemy)
                if (!Dead && !IsInvincible) {
                    if (Data.Cube && VulnerableSettings.BlockCubes)
                        return;

                    if (!Data.God && VulnerableSettings.GodsOnly)
                        return;

                    if (IsStasis)
                        return;

                    vulnerable = true;
                }

            if (vulnerable != Vulnerable) {
                Vulnerable = vulnerable;
                census.UpdateVulnerableObjectMap(this, MapPosition.X, MapPosition.Y, LastPosition.X, LastPosition.Y,
                    Vulnerable);
            }
        }

        public int DamageWithDefense(int origDamage, bool armorPiercing) {
            int def = Defense;

            if (armorPiercing || IsArmorBroken)
                def = 0;
            else if (IsArmored)
                def *= 2;

            if (IsExposed)
                def -= 20;

            int min = origDamage * 3 / 20;
            int damage = Math.Max(min, origDamage - def);

            if (IsInvulnerble)
                damage = 0;

            if (IsPetrified)
                damage = (int) (damage * 0.9);

            if (IsCursed)
                damage = (int) (damage * 1.2);

            return damage;
        }

        /// <summary>
        /// Applies condition effects of a projectile onto the <see cref="GameObject"/>.
        /// </summary>
        /// <param name="kill">Depicts whether or not the <see cref="GameObject"/> has been killed.</param>
        /// <param name="projectile">The projectile that hit the <see cref="GameObject"/>.</param>
        public void Damage(bool kill, Projectile projectile) {
            if (kill)
                Dead = true;
            else
                foreach (ConditionEffectData ce in projectile.Properties.Effects)
                    switch (ce.Effect) {
                        case ConditionEffects.QUIET:
                            if (World.Player == this) {
                                World.Player.Mana = 0;
                                World.Player.IsQuiet = true;
                            }

                            break;
                        case ConditionEffects.PARALYZED:
                            if (!IsParalyzeImmune)
                                IsParalyzed = true;
                            break;
                        case ConditionEffects.STUNNED:
                            if (!IsStunImmune)
                                IsStunned = true;
                            break;
                        case ConditionEffects.DAZED:
                            if (!IsDazedImmune)
                                IsDazed = true;
                            break;
                        case ConditionEffects.PETRIFIED:
                            if (!IsPetrifiedImmune)
                                IsPetrified = true;
                            break;
                    }
        }
    }
}