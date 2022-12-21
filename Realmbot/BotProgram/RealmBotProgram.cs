using System;
using System.Linq;
using BotCore;
using BotCore.Game.Worlds;
using BotCore.Networking;

namespace BotProgram {
    public class RealmBotProgram : ModularProgram {
        protected static BotManager manager;

        protected override void Command(string command, string[] args) {
        }

        protected override void PreStart() {
            manager = new BotManager();
            manager.Start();
        }

        protected override void Start() {
        }

        protected override void Stop() {
        }

        protected override void Update(int time, int dt) {
            //Logger.Log("Update", $"Update: Elapsed: {time} Delta: {dt}");

            RealmBot[] bots = manager.GetBots<RealmBot>();
            foreach (RealmBot bot in bots) {
                World world = bot.World;

                if (world != null) {
                    world.CurrentUpdate =
                        time; // There's not really another way to simulate getTimer using a physics/fps simulation.
                    world.Update(time, dt);
                    world.LastUpdate = time;
                }

                if (bot.TextsToSend.Any()) {
                    const int TEXT_DELAY = 2000;
                    if (bot.NextText < time) {
                        bot.NextText = time + TEXT_DELAY;
                        PlayerTextPacket playerText = bot.TextsToSend.Dequeue();
                        bot.Send(playerText);
                    }
                }

                RotMGPacket[] packets = bot.FlushPackets();
                if (packets == null) continue;
                foreach (RotMGPacket packet in packets)
                    bot.Client.Handle(packet);
            }
        }
    }
}