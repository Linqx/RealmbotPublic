using System;

namespace BotCore.Structures {
    public class MoveAction {
        public WorldPosData Position { get; set; }
        public Action<RealmBot> Callback { get; set; }

        public MoveAction(WorldPosData position, Action<RealmBot> callback) {
            Position = position;
            Callback = callback;
        }

        public void OnActionFinished(RealmBot bot) {
            Callback?.Invoke(bot);
        }
    }
}