using System;
using System.Collections.Generic;
using BotProgram;
using BotTools;
using LocalBot;

namespace LocalBotProgram {
    public class LocalProgram : RealmBotProgram {
        private const ConnectionMode CONNECTION_MODE = ConnectionMode.NEXUS;

        public static List<RealmLocalBot> Bots = new List<RealmLocalBot>();

        private static bool _running = true;
        private static LocalCommandHandler _handler;

        protected override void PreStart() {
            base.PreStart();

            _handler = new LocalCommandHandler(manager);
        }

        protected override void Start() {
            base.Start();

            while (_running)
                _handler.Handle(Console.ReadLine());
        }

        protected override void Stop() {
            Logger.Log("Local Program", "Has now stopped!", ConsoleColor.Red);

            manager.Stop();
            _running = false;
            Console.ReadLine();
        }

        protected override void Update(int time, int dt) {
            base.Update(time, dt);
        }
    }
}