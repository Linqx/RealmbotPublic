using System;
using System.Collections.Generic;
using Botcore.Data;
using BotCore.Structures;
using static BotCore.RealmBot;

namespace BotCore.Game.Worlds.Entities {
    public class Projectile : BasicObject {

        public const double TO_DEGREES = 180 / Math.PI;
        public const double TO_RADIANS = Math.PI / 180;
        public GameObject Owner;
        public WorldPosData StartPosition;
        public ProjectileProperties Properties;
        public byte BulletId;
        public int StartTime;
        public double Angle;
        public int Damage;
        public List<int> ObjectsHit;
        public bool DamagesPlayer;
        public bool DamagesEnemy;

        public UpdateProjectile OnUpdateProjectile;

        /// <summary>
        /// An ingame projectile shot by Players, Enemies and certain objects.
        /// </summary>
        /// <param name="owner">Owner of the projectile.</param>
        /// <param name="startPos">Starting position of the projectile.</param>
        /// <param name="props">Properties of the projectile.</param>
        /// <param name="bulletId">BulletId of the projectile.</param>
        /// <param name="startTime">Starting time of the projectile.</param>
        /// <param name="angle">Angle the projectile was shot at.</param>
        /// <param name="damage">The amount of damage the projectile will deal before defense has been calculated.</param>
        public Projectile(int objectId, GameObject owner, ProjectileProperties props, byte bulletId, int startTime,
            double angle, bool fromEnemy) {
            ObjectId = objectId;
            Owner = owner;
            Properties = props;
            BulletId = bulletId;
            StartTime = startTime;
            Angle = angle;
            DamagesPlayer = fromEnemy;
            DamagesEnemy = !DamagesPlayer;
        }

        public override bool Update(int time, int dt) {
            if (OnUpdateProjectile != null)
                return OnUpdateProjectile.Invoke(time, dt, this);

            return true;
        }

        public bool MoveTo(WorldPosData pos) {
            if (!Owner.World.Map.TileExists((int) pos.X, (int) pos.Y))
                return false;

            Position = pos;
            return true;
        }

        public override bool AddTo(World world, double x, double y) {
            StartPosition = new WorldPosData(x, y);

            return base.AddTo(world, x, y);
        }

        public WorldPosData GetPositionAt(int elapsed) {
            WorldPosData p = StartPosition;
            double dist = elapsed * (Properties.Speed / 10000d);
            double phase = BulletId % 2 == 0 ? 0d : Math.PI;

            if (Properties.Wavy) {
                double periodFactor = 6 * Math.PI;
                double amplitudeFactor = Math.PI / 64;
                double theta = Angle + amplitudeFactor * Math.Sin(phase + periodFactor * elapsed / 1000d);
                p.X += dist * Math.Cos(theta);
                p.Y += dist * Math.Sin(theta);
            }
            else if (Properties.Parametric) {
                double t = elapsed / Properties.LifetimeMS * 2 * Math.PI;
                double x = Math.Sin(t) * (BulletId % 2 != 0 ? 1 : -1);
                double y = Math.Sin(2 * t) * (BulletId % 4 < 2 ? 1 : -1);
                double sin = Math.Sin(Angle);
                double cos = Math.Cos(Angle);
                p.X += (x * cos - y * sin) * Properties.Magnitude;
                p.Y += (x * sin + y * cos) * Properties.Magnitude;
            }
            else {
                if (Properties.Boomerang) {
                    double halfway = Properties.LifetimeMS * (Properties.Speed / 10000d) / 2;
                    if (dist > halfway)
                        dist = halfway - (dist - halfway);
                }

                p.X += dist * Math.Cos(Angle);
                p.Y += dist * Math.Sin(Angle);

                if (Properties.Amplitude != 0) {
                    double deflection = Properties.Amplitude *
                                        Math.Sin(phase + elapsed / Properties.LifetimeMS * Properties.Frequency * 2 *
                                                 Math.PI);
                    p.X += deflection * Math.Cos(Angle + Math.PI / 2);
                    p.Y += deflection * Math.Sin(Angle + Math.PI / 2);
                }
            }

            return p;
        }
    }
}