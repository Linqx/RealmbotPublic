using BotCore.Game.Worlds.Entities;

namespace BotCore {
    public partial class RealmBot {
        public delegate bool FullOccupy(double x, double y);

        public delegate bool UpdatePlayer(int time, int dt, Player player);

        public delegate bool UpdateProjectile(int time, int dt, Projectile p);
    }
}