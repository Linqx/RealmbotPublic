using BotCore;

namespace AFKBot {
    public class RealmAFKBot : RealmBot {
        public RealmAFKBot(string email, string password) : base(email, password) {
            Name = "Realm AFK Bot";
        }
    }
}