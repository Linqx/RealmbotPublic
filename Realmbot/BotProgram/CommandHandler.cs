namespace BotProgram {
    public class CommandHandler {
        public BotManager Manager;

        public CommandHandler(BotManager manager) {
            Manager = manager;
        }

        public virtual void Handle(string readline) {
        }
    }
}