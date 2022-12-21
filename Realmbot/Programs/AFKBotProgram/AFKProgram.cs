using System;
using AFKBot;
using BotCore.Structures;
using BotProgram;
using BotTools;

namespace AFKBotProgram {
    public class AFKProgram : RealmBotProgram {
        private const ConnectionMode CONNECTION_MODE = ConnectionMode.NEXUS;

        public static Settings Settings;

        private static bool Running = true;
        private static AFKCommandHandler Handler;
        private static RealmAFKBot AFKBot;

        protected override void PreStart() {
            base.PreStart();

            Settings = Settings.FromResource("settings.json");
            Handler = new AFKCommandHandler(manager);
        }

        protected override void Start() {
            base.Start();

            AFKBot = manager.AddBot<RealmAFKBot>(Settings.Email, Settings.Password);
            AFKBot.CharId = Settings.CharId;
            AFKBot.Connect(Server.USWest, CONNECTION_MODE);

            while (Running)
                Handler.Handle(Console.ReadLine());
        }

        protected override void Stop() {
            base.Stop();

            Logger.Log("AFK Bot", "Has now stopped!", ConsoleColor.White);

            Running = false;
            Console.ReadLine();
        }

        protected override void Update(int time, int dt) {
            base.Update(time, dt);
        }
    }
}