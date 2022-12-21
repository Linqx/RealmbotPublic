using System;
using System.Threading.Tasks;
using BotCore.Structures;
using BotProgram;
using BotTools;
using KeyBot;
using KeyBotProgram.Discord;

namespace KeyBotProgram {
    public class KeyProgram : RealmBotProgram {
        public static Settings Settings;
        public static bool Running = true;

        private static RealmKeyBot KeyBot;
        private static KeyCommandHandler CommandHandler;
        private static DiscordKeyBot DiscordBot;

        protected override void PreStart() {
            base.PreStart();

            Console.Title = "Key Bot Program";
            Settings = Settings.FromResource("settings.json");
            CommandHandler = new KeyCommandHandler(manager);
        }

        protected override void Start() {
            KeyBot = manager.AddBot<RealmKeyBot>(Settings.Email, Settings.Password);
            KeyBot.ChangeTitle = ChangeTitle;
            KeyBot.WhiteList = Settings.Whitelist;
            KeyBot.CharId = Settings.CharId;
            KeyBot.Connect(Server.USWest, ConnectionMode.VAULT);

            DiscordBot = new DiscordKeyBot(KeyBot);

            base.Start();

            while (Running)
                CommandHandler.Handle(Console.ReadLine());
        }

        protected override void Stop() {
            Logger.Log("Key Program", "Has now stopped!", ConsoleColor.White);
            manager.Stop();

            base.Stop();
        }

        protected override void Update(int time, int dt) {
            base.Update(time, dt);
            KeyBot?.Update(time, dt);
        }

        private static void ChangeTitle(string title) {
            Console.Title = title;
        }
    }
}