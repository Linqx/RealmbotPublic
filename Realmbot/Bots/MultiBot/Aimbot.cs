using System;

namespace MultiBot {
    public class Aimbot {
        public static AimMode AimMode = AimMode.CLOSEST;
        public static bool ShootAtWalls = true;
        public static bool BossPriority = true;
        public static bool DamageIgnored = false;
        public static bool OnlyAtExcepted = false;
        public static bool AimAtInvulnerable = false;
        public static int SpellBombHPThreshold = 0;
        public static int SkullThreshold = 0;

        public static double AutoAimDistance = 10;
        public static int BoundingDistance = 2;
        public static bool TargetLead = true;

        private readonly Census census;

        public Aimbot(Census census) {
            this.census = census;
        }

        public WorldPosData CalcMouseAimAngle(double projSpeed, double maxDist, WorldPosData playerPosition,
            WorldPosData mousePosition, bool hpThreshold = false) {
            maxDist *= maxDist;

            bool running = true;
            GameObject[] enemies = census.GetVulnerableObjects();
            WorldPosData enemyPosition = WorldPosData.Zero;
            bool bossPriority = BossPriority;
            double bounding = BoundingDistance * BoundingDistance;
            double minDist = 1000;

            while (running) {
                foreach (GameObject enemy in enemies) {
                    WorldPosData currentPosition;
                    if (!ShootAtWalls && !(enemy is Character)) continue;
                    if (bossPriority && !enemy.Boss) continue;
                    if (enemy.Dead) continue;
                    if (enemy.Ignored && DamageIgnored) continue;
                    if (!enemy.Excepted && OnlyAtExcepted) continue;
                    if (enemy.IsInvulnerble && !AimAtInvulnerable) continue;

                    if (double.IsNaN(projSpeed)) {
                        if (Math.Abs(maxDist - 144.0) < 0.001 && enemy.MaxHealth < SpellBombHPThreshold) continue;
                        if (Math.Abs(maxDist - 49.0) < 0.001 && enemy.MaxHealth < SkullThreshold) continue;

                        currentPosition = enemy.TickPosition;
                    }
                    else if (hpThreshold && enemy.MaxHealth < SpellBombHPThreshold) {
                        continue;
                    }
                    else if (enemy.Jittery || !TargetLead) {
                        currentPosition = enemy.Position;
                    }
                    else {
                        currentPosition = CalcLeadPosition(playerPosition, projSpeed, enemy.Position, enemy.MoveVector);
                    }

                    if (currentPosition == WorldPosData.Zero) continue;

                    double dist = enemy.Position.Distance(playerPosition);
                    if (dist > maxDist) continue;

                    dist = enemy.Position.Distance(mousePosition);
                    if (dist > bounding) continue;

                    if (bossPriority && enemy.Boss) {
                        running = false;
                        enemyPosition = currentPosition;
                        break;
                    }

                    if (!(dist <= minDist)) continue;
                    minDist = dist;
                    enemyPosition = currentPosition;
                }

                if (bossPriority) {
                    if (running)
                        bossPriority = false;
                }
                else {
                    running = false;
                }
            }

            return enemyPosition;
        }

        private static WorldPosData CalcLeadPosition(WorldPosData playerPosition, double projSpeed, WorldPosData target,
            WorldPosData moveVector) {
            WorldPosData diff = target - playerPosition;
            double a = 2 * (moveVector.DotProduct(moveVector) - projSpeed * projSpeed);
            double b = 2 * diff.DotProduct(moveVector);
            double c = diff.DotProduct(diff);
            double squareRoot = Math.Sqrt(b * b - 2 * a * c);
            double quadP = (-b + squareRoot) / a;
            double quadN = (-b - squareRoot) / a;

            if (quadP < quadN && quadP >= 0)
                moveVector *= quadP;
            else if (quadN >= 0)
                moveVector *= quadN;
            else
                return WorldPosData.Zero;

            return target + moveVector;
        }
    }

    public enum AimMode {
        MOUSE,
        HEALTH,
        CLOSEST,
        RANDOM
    }
}