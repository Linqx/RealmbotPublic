using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotCore;
using Botcore.Content;
using Botcore.Data;
using BotCore.Game.Worlds.Entities;
using BotCore.Networking;
using Rekishi;

namespace KeyBot.States
{
    public class EnterGuildHallState : KeyBotState {
        public Portal GuildHallPortal;

        private readonly PortalData _guildHallData = GameContent.GetPortalData("Guild Hall Portal");
        private bool _enteringGuildHall;
        private bool _runningToCenter;

        public EnterGuildHallState(RealmKeyBot bot) : base(bot, KeyBotStates.EnteringGuildHall) {
        }

        public override void Start() {
            Log.Write($"[{Bot.Name}] Entering guild hall.");
            Subcribe(PacketType.NEWTICK, OnNewTick);
        }

        public override void Run() {
            if (Bot.Player == null) return;
            if (GuildHallPortal == null) FindGuildHall();
            if (!_enteringGuildHall && GuildHallPortal != null) EnterGuildHall();
        }

        public override void End() {
            Log.Write($"[{Bot.Name}] Entered guild hall.");
        }

        private void FindGuildHall() {
            Portal[] portals = Bot.World.Census.GetGameObjects<Portal>();
            foreach (Portal portal in portals)
            {
                if (portal.Data.Type != _guildHallData.Type) continue;
                GuildHallPortal = portal;
                Log.Debug("[Key Bot] Guild Hall Portal found");
                break;
            }
        }

        private void EnterGuildHall() {
            _enteringGuildHall = true;

            Bot.AddMoveAction(GuildHallPortal.Position, bot => {
                Log.Debug("[Key Bot] Attempting to enter Guild Hall Portal");
                
                UsePortalPacket usePortal = new UsePortalPacket();
                usePortal.ObjectId = GuildHallPortal.ObjectId;
                bot.Send(usePortal);
            });
        }

        private void OnNewTick(RotMGPacket packet) {
            if (_runningToCenter) return;
            if (!Bot.World.Map.Name.Contains("Guild Hall")) return;

            _runningToCenter = true;
            Bot.AddMoveAction(38, 38, OnCenter);
        }

        private void OnCenter(RealmBot bot) {
            Bot.SetState(new AcceptingCommandsState(Bot));
        }
    }
}
