using System;
using Botcore.Content;
using Botcore.Data;
using BotCore.Game.Worlds.Entities;
using BotCore.Networking;
using BotCore.Structures;

namespace BotCore {
    public partial class RealmBot {
        private const double MIN_ATTACK_FREQ = 0.0015d;
        private const double MAX_ATTACK_FREQ = 0.008d;
        private const double MAX_MIN_ATTACK_FREQ_DIFF = MAX_ATTACK_FREQ - MIN_ATTACK_FREQ;

        private const double MIN_ATTACK_MULT = 0.5d;
        private const double MAX_ATTACK_MULT = 2d;
        private const double MAX_MIN_ATTACK_MULT_DIFF = MAX_ATTACK_MULT - MIN_ATTACK_MULT;

        public int AttackStartTime;

        public void AttemptShoot(double angle) {
            if (World == null || Player == null) return;
            if (Player.IsStunned || Player.IsPaused || Player.IsPetrified) return;
            if (Player.Inventory[0] == null) return;

            ItemData weapon = Player.Inventory[0];

            double rateOfFire = weapon.RateOfFire;
            double attackPeriod = 1 / AttackFrequency() * (1 / rateOfFire);
            if (World.CurrentUpdate < AttackStartTime + attackPeriod) return;

            AttackStartTime = World.CurrentUpdate;
            Shoot(angle, weapon.Type, World.CurrentUpdate, true);
        }

        private void
            Shoot(double attackAngle, ushort weaponType, int time, bool useAttackMultiplier) // DoShoot in client
        {
            /*
             * Get the bullet id and set the next bullet id.
             * Tell server that projectile has been shot.
             * Add projectile to Map.
             */

            ItemData weapon = GameContent.GetItemData(weaponType);
            double arcGap = weapon.ArcGap * Projectile.TO_RADIANS;
            double totalArc = arcGap * (weapon.NumProjectiles - 1);
            double angle = attackAngle - totalArc * 0.5d;

            WorldPosData pos = new WorldPosData {
                X = Player.X + Math.Cos(angle) * 0.3d,
                Y = Player.Y + Math.Sin(angle) * 0.3d
            };

            for (int i = 0; i < weapon.NumProjectiles; i++) {
                byte id = GetBulletId();
                int damage =
                    (int) (RandomSync.NextIntRange((uint) weapon.Projectile.MinDamage,
                               (uint) weapon.Projectile.MaxDamage) * (useAttackMultiplier ? AttackMultiplier() : 1d));

                //TODO: Projectile angle manipulation hacks


                //Logger.Log("RealmBot", $"Shooting >> Id: {id} Angle: {angle} Time: {Map.CurrentUpdate} Position: {Player.Position}");

                PlayerShootPacket playerShootPacket = new PlayerShootPacket {
                    Time = time,
                    BulletId = id,
                    ContainerType = weaponType,
                    Position = pos,
                    Angle = angle
                };

                Projectile proj = GenerateProjectile(playerShootPacket, weapon.Projectile);
                proj.Damage = damage;

                World.Census.AddObject(proj, pos.X, pos.Y);
                Send(playerShootPacket);
                angle += arcGap;
            }
        }

        public Projectile GenerateProjectile(PlayerShootPacket playerShoot, ProjectileProperties props) {
            return new Projectile(
                World.Census.GetNewObjectId(ObjectId, playerShoot.BulletId),
                Player,
                props,
                playerShoot.BulletId,
                playerShoot.Time,
                playerShoot.Angle,
                false
            );
        }

        public Projectile GenerateProjectile(ServerPlayerShootPacket serverPlayerShoot, ProjectileProperties props) {
            return new Projectile(
                World.Census.GetNewObjectId(ObjectId, serverPlayerShoot.BulletId),
                Player,
                props,
                serverPlayerShoot.BulletId,
                World.LastUpdate,
                serverPlayerShoot.Angle,
                false
            );
        }

        public double AttackFrequency() {
            if (Player.IsDazed)
                return MIN_ATTACK_FREQ;

            double attackFreq = MIN_ATTACK_FREQ + Player.Dexterity / 75d * MAX_MIN_ATTACK_FREQ_DIFF;
            if (Player.IsBerserk)
                attackFreq *= 1.5;

            return attackFreq;
        }

        public double AttackMultiplier() {
            if (Player.IsWeak)
                return MIN_ATTACK_MULT;

            double attMult = MIN_ATTACK_MULT + Player.Attack / 75d * MAX_MIN_ATTACK_MULT_DIFF;

            if (Player.IsDamaging)
                attMult *= 1.5;

            return attMult;
        }

        public void ShootAck(int time) {
            ShootAckPacket shootAck = new ShootAckPacket();
            shootAck.Time = time;
            Send(shootAck);
        }

        public void SquareHit(int time, byte bulletId, int objectId) {
            SquareHit squareHit = new SquareHit {
                Time = time,
                BulletId = bulletId,
                ObjectId = objectId
            };

            Send(squareHit);
        }

        public void OtherHit(int time, byte bulletId, int objectId, int targetId) {
            OtherHitPacket otherHit = new OtherHitPacket {
                Time = time,
                BulletId = bulletId,
                ObjectId = objectId,
                TargetId = targetId
            };

            Send(otherHit);
        }

        public void EnemyHit(int time, byte bulletId, int objectId, bool dead) {
            EnemyHitPacket enemyHit = new EnemyHitPacket {
                Time = time,
                BulletId = bulletId,
                TargetId = objectId,
                Kill = dead
            };

            Send(enemyHit);
        }

        public void PlayerHit(byte bulletId, int objectId) {
            PlayerHitPacket playerHit = new PlayerHitPacket {
                BulletId = bulletId,
                ObjectId = objectId
            };

            Send(playerHit);
        }
    }
}