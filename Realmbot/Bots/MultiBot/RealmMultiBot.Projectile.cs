using System;

namespace MultiBot {
    public partial class RealmMultiBot {
        public bool OnUpdateProjectile(int time, int dt, Projectile p) {
            int elapsed = time - p.StartTime;

            if (elapsed > p.Properties.LifetimeMS)
                return false;

            p.Position = p.GetPositionAt(elapsed);
            int x = p.MapPosition.X;
            int y = p.MapPosition.Y;

            ushort? tile = Map.GetTile(x, y);
            if (!p.MoveTo(p.Position) || tile.HasValue && tile.Value == 0xFFFF) {
                if (p.DamagesPlayer)
                    SquareHit(time, p.BulletId, p.Owner.ObjectId);

                return false;
            }

            GameObject so = Census.GetStaticObject(x, y);
            StaticObjectData soData = (StaticObjectData) so?.Data;
            if (so != null && (!so.Data.Enemy || !p.DamagesEnemy) &&
                (soData.EnemyOccupySquare || !p.Properties.PassesCover && soData.OccupySquare)) {
                if (p.DamagesPlayer)
                    OtherHit(time, p.BulletId, p.Owner.ObjectId, so.ObjectId);

                return false;
            }

            GameObject target = GetHit(p.X, p.Y, p);

            if (target != null) {
                Player player = World.Player;
                bool isPlayer = player != null;
                bool isTargetAnEnemy = target.Data.Enemy;
                bool sendMessage = isPlayer && !player.IsPaused &&
                                   (p.DamagesPlayer || isTargetAnEnemy && p.Owner == player);

                if (sendMessage) {
                    int damage = target.DamageWithDefense(p.Damage, p.Properties.ArmorPiercing);
                    bool dead = target.Health <= damage;

                    if (target == player) {
                        if (SubtractDamage(damage, time))
                            return false;

                        player.Damage(false, p);
                        PlayerHit(p.BulletId, p.Owner.ObjectId);
                    }
                    else {
                        if (target.Data.Enemy) {
                            if (target.Data.Cube && VulnerableSettings.BlockCubes) return true;
                            if (!target.Data.God && VulnerableSettings.GodsOnly) return true;

                            //Logger.Log("Info", $"Projectile Pos: {p.Position} Target Position: {target.Position} Distance: {p.Position.Distance(target.Position)}");
                            target.Damage(dead, p);
                            EnemyHit(time, p.BulletId, target.ObjectId, dead);
                        }
                        else {
                            if (!p.Properties.MultiHit)
                                OtherHit(time, p.BulletId, p.Owner.ObjectId, target.ObjectId);
                        }
                    }
                }

                if (p.Properties.MultiHit)
                    p.ObjectsHit.Add(target.ObjectId);
                else
                    return false;
            }

            return true;
        }

        //getHit_optimized in Evan's client (private aka chad version)
        public GameObject GetHit(double x, double y, Projectile p) {
            WorldPosData diff = WorldPosData.Zero;
            double minDist = 3;
            GameObject closestObject = null;
            bool passThroughInvuln = VulnerableSettings.PassThroughInvulnerable;
            bool damageIngored = VulnerableSettings.DamageIgnored;
            bool blockCubes = VulnerableSettings.BlockCubes;
            bool godsOnly = VulnerableSettings.GodsOnly;
            bool playerCanBeHit = false; // God mode poggers
            GameObject[] objectsCanHit = Census.GetSurroundingVulnerable((int) x, (int) y);

            if (objectsCanHit.Length == 0) return null;

            if (p.DamagesEnemy)
                foreach (GameObject go in objectsCanHit) {
                    if (go is Player) continue;
                    if (go.IsInvulnerble && passThroughInvuln && !p.Properties.HasConditions) continue;
                    if (go.Ignored && !damageIngored) continue;
                    if (go.Data.Cube && blockCubes) continue;
                    if (!go.Data.God && godsOnly) continue;

                    diff.X = Math.Abs(x - go.X);
                    diff.Y = Math.Abs(y - go.Y);
                    if (diff.X > 0.5) continue;
                    if (diff.Y > 0.5) continue;

                    if (!(p.Properties.MultiHit && p.ObjectsHit.Contains(go.ObjectId)))
                        return go;
                }
            else if (p.DamagesPlayer)
                foreach (GameObject go in objectsCanHit) {
                    if (!(go is Player)) continue;

                    diff.X = Math.Abs(x - go.X);
                    diff.Y = Math.Abs(y - go.Y);
                    if (diff.X > 0.5) continue;
                    if (diff.Y > 0.5) continue;

                    if (!p.Properties.MultiHit || !p.ObjectsHit.Contains(go.ObjectId)) {
                        if (go == World.Player) {
                            if (p.Properties.MultiHit)
                                return go;
                            playerCanBeHit = true;
                            continue;
                        }

                        double dist = diff.Distance(diff);
                        if (dist < minDist) {
                            minDist = dist;
                            closestObject = go;
                        }
                    }
                }

            if (playerCanBeHit && closestObject == null)
                return World.Player;

            return closestObject;
        }

        public WorldPosData CalcAimbotAngle(ushort weaponType, WorldPosData mouse_pos) {
            return WorldPosData.Zero;

            //ItemData weapon = GameContent.GetItemData(weaponType);
            //if (weapon == null) return WorldPosData.Zero;

            //switch (Aimbot.AimMode)
            //{
            //    case AimMode.MOUSE:
            //        return Aimbot.CalcMouseAimAngle(weapon.Projectile.Speed, weapon.Projectile.MaxProjectileTravel + Aimbot.AutoAimDistance, Player.Position, mouse_pos);
            //    case AimMode.HEALTH:
            //        break;
            //    case AimMode.CLOSEST:
            //        break;
            //    case AimMode.RANDOM:
            //        break;
            //}

            //return CalcLeadPosition(weapon.Projectile.Speed, linqxPet.Position, linqxPet.MoveVector);
        }
    }
}