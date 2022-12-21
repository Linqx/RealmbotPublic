using System.Linq;
using BotProgram;
using LocalBot;

namespace LocalBotProgram {
    public class LocalCommandHandler : CommandHandler {

        public LocalCommandHandler(BotManager manager) : base(manager) {
        }

        public override void Handle(string readline) {
            string[] data = readline.Split(' ');
            string command = data[0];
            string[] args = data.Where((_, i) => i != 0).ToArray();

            switch (command) {
                case "nexus":
                    if (args.Length > 0) {
                        int amount = int.Parse(args[0]);
                        if (amount > 500) amount = 500;

                        for (int i = 0; i < amount; i++) {
                            RealmLocalBot bot = Manager.AddBot<RealmLocalBot>($"", "");
                            bot.CharId = 1;
                            bot.Connect("127.0.0.1", 2050, (int) ConnectionMode.NEXUS, 0, null);
                            LocalProgram.Bots.Add(bot);
                        }
                    }

                    break;
                case "realm":
                    if (args.Length > 0) {
                        int amount = int.Parse(args[0]);
                        if (amount > 500) amount = 500;

                        for (int i = 0; i < amount; i++) {
                            RealmLocalBot bot = Manager.AddBot<RealmLocalBot>($"", "");
                            bot.CharId = 1;
                            bot.Connect("127.0.0.1", 2050, (int) ConnectionMode.REALM, 0, null);
                            LocalProgram.Bots.Add(bot);
                        }
                    }

                    break;
                case "wander":
                    foreach (RealmLocalBot i in LocalProgram.Bots)
                        i.Wander(i);
                    break;
            }
        }
    }
}