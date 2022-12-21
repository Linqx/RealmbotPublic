using System;
using System.Collections.Generic;
using Botcore.Content;
using Botcore.Data;
using BotCore.Structures;
using RotMGCore.Structures.Game;
using static BotCore.RealmBot;

namespace BotCore.Game.Worlds.Entities {
    public class Player : GameObject {
        private const double MIN_MOVE_SPEED = 0.004d;
        private const double MAX_MOVE_SPEED = 0.0096d;
        private const double DIF_MOVE_SPEED = MAX_MOVE_SPEED - MIN_MOVE_SPEED;
        private const double MOVE_THRESHOLD = 0.4d;

        public Inventory Inventory;
        public Inventory Backpack;
        public bool HasBackpack;

        public double MoveMultiplier = 1d;

        public FullOccupy FullOccupy;
        public UpdatePlayer UpdatePlayer;

        public Player(PlayerData data) : base(data) {
            Data = data;

            Inventory = new Inventory(20);
            Backpack = new Inventory(8);
        }

        public double GetMoveSpeed() {
            if (IsSlowed)
                return MIN_MOVE_SPEED * MoveMultiplier;

            double speed = MIN_MOVE_SPEED + Speed / 75d * DIF_MOVE_SPEED;

            if (IsSpeedy || IsNinjaSpeedy)
                speed *= 1.5d;

            speed *= MoveMultiplier;
            return speed;
        }

        public override bool Update(int time, int dt) {
            if (IsPaused)
                return true;

            if (UpdatePlayer != null)
                return UpdatePlayer(time, dt, this);
            return base.Update(time, dt);
        }

        public override void ApplyStatusData(ObjectStatusData data, int serverDt, int serverTickId) {
            base.ApplyStatusData(data, serverDt, serverTickId);

            int value = 0;
            string strValue = null;
            ItemData item = null;
            int index = 0;

            foreach (KeyValuePair<StatDataType, object> pair in data.Stats) {
                if (StatDataUtils.IsUTF(pair.Key))
                    strValue = (string) pair.Value;
                else
                    value = (int) pair.Value;

                switch (pair.Key) {
                    //TODO; Rest of stats
                    case StatDataType.Attack:
                        Attack = value;
                        break;
                    case StatDataType.Speed:
                        Speed = value;
                        break;
                    case StatDataType.Dexterity:
                        Dexterity = value;
                        break;
                    case StatDataType.Inventory0:
                    case StatDataType.Inventory1:
                    case StatDataType.Inventory2:
                    case StatDataType.Inventory3:
                    case StatDataType.Inventory4:
                    case StatDataType.Inventory5:
                    case StatDataType.Inventory6:
                    case StatDataType.Inventory7:
                    case StatDataType.Inventory8:
                    case StatDataType.Inventory9:
                    case StatDataType.Inventory10:
                    case StatDataType.Inventory11:

                        /*
                        * Grab item from xmls collection.
                        * Create index and set item.
                        */

                        item = GameContent.GetItemData((ushort) value);
                        index = pair.Key - StatDataType.Inventory0;

                        Inventory[index] = GameContent.GetItemData((ushort) value);
                        break;
                    case StatDataType.Has_Backpack:
                        HasBackpack = value > 0;
                        break;
                    case StatDataType.Backpack0:
                    case StatDataType.Backpack1:
                    case StatDataType.Backpack2:
                    case StatDataType.Backpack3:
                    case StatDataType.Backpack4:
                    case StatDataType.Backpack5:
                    case StatDataType.Backpack6:
                    case StatDataType.Backpack7:
                        /*
                         * Grab item from xmls collection.
                         * Create index and add 12 (equipment slots and main inventory slots).
                         * Set item.
                         */

                        item = GameContent.GetItemData((ushort) value);
                        index = pair.Key - StatDataType.Backpack0 + 12;

                        Inventory[index] = item;
                        Backpack[index - 12] = item;
                        break;
                }

                value = 0;
                strValue = null;
                item = null;
                index = 0;
            }
        }

        public override void UpdateConditionEffects(bool first) {
            if (first) {
                if (World.Player == this) {
                    IsWeak = HasConditionEffect(ConditionEffectsBits.WEAK_BIT);
                    IsSlowed = HasConditionEffect(ConditionEffectsBits.SLOWED_BIT);
                    IsSick = HasConditionEffect(ConditionEffectsBits.SICK_BIT);
                    IsBlind = HasConditionEffect(ConditionEffectsBits.BLIND_BIT);
                    IsDrunk = HasConditionEffect(ConditionEffectsBits.DRUNK_BIT);
                    IsBleeding = HasConditionEffect(ConditionEffectsBits.BLEEDING_BIT);
                    IsConufsed = HasConditionEffect(ConditionEffectsBits.CONFUSED_BIT);
                    IsSpeedy = HasConditionEffect(ConditionEffectsBits.SPEEDY_BIT);
                    IsNinjaSpeedy = HasConditionEffect(ConditionEffectsBits.NINJA_SPEEDY_BIT);
                    IsHallucinating = HasConditionEffect(ConditionEffectsBits.HALLUCINATING_BIT);
                    IsDamaging = HasConditionEffect(ConditionEffectsBits.DAMAGING_BIT);
                    IsBerserk = HasConditionEffect(ConditionEffectsBits.BERSERK_BIT);
                    IsUnstable = HasConditionEffect(ConditionEffectsBits.UNSTABLE_BIT);
                    IsDarkness = HasConditionEffect(ConditionEffectsBits.DARKNESS_BIT);
                    IsQuiet = HasConditionEffect(ConditionEffectsBits.QUIET_BIT);
                }

                IsInvisible = HasConditionEffect(ConditionEffectsBits.INVISIBLE_BIT);
                IsHealing = HasConditionEffect(ConditionEffectsBits.HEALING_BIT);
            }
            else {
                if (World.Player == this) IsSilenced = HasConditionEffect(ConditionEffectsBits2.SILENCED_BIT);
            }

            base.UpdateConditionEffects(first);
        }

        public override void UpdateVulnerable() {
            bool vulnerable = !IsPaused && !IsInvincible && !IsStasis && !Dead;

            if (vulnerable != Vulnerable) {
                Vulnerable = vulnerable;
                census.UpdateVulnerableObjectMap(this, MapPosition.X, MapPosition.Y, LastPosition.X, LastPosition.Y,
                    Vulnerable);
            }
        }

        public bool ValidPosition(double x, double y) {
            IntPoint intP = new IntPoint((int) x, (int) y);
            if (MapPosition != intP && !map.Walkable(intP.X, intP.Y))
                return false;

            double xFrac = x - intP.X;
            double yFrac = y - intP.Y;

            if (xFrac < 0.5) {
                if (FullOccupy(x - 1, y))
                    return false;

                if (yFrac < 0.5) {
                    if (FullOccupy(x, y - 1) || FullOccupy(x - 1, y - 1))
                        return false;
                }
                else if (yFrac > 0.5) {
                    if (FullOccupy(x, y + 1) || FullOccupy(x - 1, y + 1))
                        return false;
                }
            }
            else if (xFrac > 0.5) {
                if (FullOccupy(x + 1, y))
                    return false;

                if (yFrac < 0.5) {
                    if (FullOccupy(x, y - 1) || FullOccupy(x + 1, y - 1))
                        return false;
                }
                else if (yFrac > 0.5) {
                    if (FullOccupy(x, y + 1) || FullOccupy(x + 1, y + 1))
                        return false;
                }
            }
            else if (yFrac < 0.5) {
                if (FullOccupy(x, y - 1))
                    return false;
            }
            else if (yFrac > 0.5) {
                if (FullOccupy(x, y + 1))
                    return false;
            }

            return true;
        }

        public WorldPosData ModifyMove(WorldPosData pos) {
            if (IsParalyzed || IsPetrified)
                return new WorldPosData(X, Y);

            WorldPosData diffPos = pos - Position;

            if (diffPos.X < MOVE_THRESHOLD && diffPos.X > -MOVE_THRESHOLD && diffPos.Y < MOVE_THRESHOLD &&
                diffPos.Y > -MOVE_THRESHOLD)
                return ModifyStep(pos);

            double stepSize = MOVE_THRESHOLD / Math.Max(Math.Abs(diffPos.X), Math.Abs(diffPos.Y));
            double d = 0;
            bool done = false;
            WorldPosData newPoint = Position;

            while (!done) {
                if (d + stepSize >= 1) {
                    stepSize = 1 - d;
                    done = true;
                }

                newPoint = ModifyStep(newPoint + diffPos * stepSize);
                d += stepSize;
            }

            return newPoint;
        }

        public WorldPosData ModifyStep(WorldPosData pos) {
            WorldPosData newPoint = pos;
            double nextXBorder = double.NaN;
            double nextYBorder = double.NaN;
            bool xCross = X % 0.5 == 0 && pos.X != X || (int) (X * 2) != (int) (pos.X * 2);
            bool yCross = Y % 0.5 == 0 && pos.Y != Y || (int) (Y * 2) != (int) (pos.Y * 2);

            if (!xCross && !yCross || ValidPosition(pos.X, pos.Y))
                return newPoint;


            if (xCross) {
                nextXBorder = pos.X > X ? (int) (pos.X * 2) * 0.5d : (int) (X * 2) * 0.5d;

                if ((int) nextXBorder > (int) X)
                    nextXBorder -= 0.01d;
            }

            if (yCross) {
                nextYBorder = pos.Y > Y ? (int) (pos.Y * 2) * 0.5d : (int) (Y * 2) * 0.5d;

                if ((int) nextYBorder > (int) Y)
                    nextYBorder -= 0.01d;
            }

            if (!xCross) {
                newPoint.Y = nextYBorder;
                return newPoint;
            }

            if (!yCross) {
                newPoint.X = nextXBorder;
                return newPoint;
            }

            double xBorderDist = pos.X > X ? pos.X - nextXBorder : nextXBorder - pos.X;
            double yBorderDist = pos.Y > Y ? pos.Y - nextYBorder : nextYBorder - pos.Y;

            if (xBorderDist > yBorderDist) {
                if (ValidPosition(pos.X, nextYBorder)) {
                    newPoint.Y = nextYBorder;
                    return newPoint;
                }

                if (ValidPosition(nextXBorder, pos.Y)) {
                    newPoint.X = nextXBorder;
                    return newPoint;
                }
            }
            else {
                if (ValidPosition(nextXBorder, pos.Y)) {
                    newPoint.X = nextXBorder;
                    return newPoint;
                }

                if (ValidPosition(pos.X, nextYBorder)) {
                    newPoint.Y = nextYBorder;
                    return newPoint;
                }
            }

            newPoint.X = nextXBorder;
            newPoint.Y = nextYBorder;
            return newPoint;
        }

        #region Stats

        public int MaxMagic;
        public int Mana;
        public int Attack;
        public int Speed;
        public int Dexterity;
        public int Vitality;
        public int Wisdom;

        #endregion

        #region Boosts

        public int HpBoost;
        public int ManaBoost;
        public int AttackBoost;
        public int DefenseBoost;
        public int SpeedBoost;
        public int DexterityBoost;
        public int VitalityBoost;
        public int WisdomBoost;

        #endregion

        #region Condition Effects Properties

        public bool IsWeak;
        public bool IsSlowed;
        public bool IsSick;
        public bool IsBlind;
        public bool IsDrunk;
        public bool IsBleeding;
        public bool IsConufsed;
        public bool IsSpeedy;
        public bool IsNinjaSpeedy;
        public bool IsHallucinating;
        public bool IsDamaging;
        public bool IsBerserk;
        public bool IsUnstable;
        public bool IsDarkness;
        public bool IsSilenced;
        public bool IsQuiet;
        public bool IsInvisible;
        public bool IsHealing;

        #endregion

    }
}