using System.Linq;
using BotProgram;

namespace AFKBotProgram {
    public class AFKCommandHandler : CommandHandler {
        public AFKCommandHandler(BotManager manager) : base(manager) {
        }

        public override void Handle(string readline) {
            string[] data = readline.Split(' ');
            string command = data[0];
            string[] args = data.Where((_, i) => i != 0).ToArray();

            switch (command) {
                case "disconnect":
                    if (args.Length > 0)
                        Manager.RemoveBot(args[0]);
                    break;
            }
        }
    }
}