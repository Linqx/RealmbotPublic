using Botcore.Data;

namespace BotCore.Game.Worlds.Entities {
    public class Portal : GameObject {
        public new PortalData Data;

        public Portal(PortalData data) : base(data) {
            Data = data;
        }
    }
}