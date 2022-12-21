using Botcore.Data;

namespace BotCore.Game.Worlds.Entities {
    public class Pet : GameObject {
        public new PetData Data;

        public Pet(PetData data) : base(data) {
            Data = data;
        }
    }
}