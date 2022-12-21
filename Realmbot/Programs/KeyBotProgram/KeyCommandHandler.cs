using System;
using System.Linq;
using BotCore.Structures;
using BotProgram;
using BotTools;
using KeyBot;
using Rekishi;

namespace KeyBotProgram {
    public class KeyCommandHandler : CommandHandler {
        public KeyCommandHandler(BotManager manager) : base(manager) {
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
                case "moverelative":
                    if (args.Length > 0) {
                        string email = args[0];
                        int x = int.Parse(args[1]);
                        int y = int.Parse(args[2]);

                        RealmKeyBot bot = Manager.GetBot<RealmKeyBot>(email);
                        if (bot == null || bot.Player == null)
                            Logger.Log("Key Command Handler", $"Error moving bot: {email}!", ConsoleColor.Red);
                        else
                            bot.AddMoveAction(new WorldPosData {X = bot.Player.X + x, Y = bot.Player.Y + y});
                    }

                    break;
                case "move":
                    if (args.Length > 0) {
                        string email = args[0];
                        int x = int.Parse(args[1]);
                        int y = int.Parse(args[2]);

                        RealmKeyBot bot = Manager.GetBot<RealmKeyBot>(email);
                        if (bot == null || bot.Player == null)
                            Logger.Log("Key Command Handler", $"Error moving bot: {email}!", ConsoleColor.Red);
                        else
                            bot.AddMoveAction(new WorldPosData {X = x, Y = y});
                    }
                    break;
                case "g":
                    if (args.Length > 0)
                    {
                        string email = args[0];
                        string[] a = new string[args.Length - 1];

                        for (int i = 0; i < args.Length; i++) {
                            if (i == 0) continue;
                            a[i - 1] = args[i];
                        }

                        string chat = string.Join(" ", a);

                        RealmKeyBot bot = Manager.GetBot<RealmKeyBot>(email);
                        if (bot == null || bot.Player == null) {
                            Log.Error($"Bot {email} not found");
                            return;
                        }

                        bot.SendText($"/g {chat}");
                    }
                    break;
                case "stop":
                    KeyProgram.Running = false;
                    break;
            }
        }
    }
}