using System;
using BotTools;

namespace MultiBot.Handlers {
    public class NewTickHandler : MultiBotHandler {

        public override PacketType Handles => PacketType.NEWTICK;
        private readonly PortalData _guildHallPortalData = GameContent.GetPortalData("Guild Hall Portal");

        public override void Handle(Packet packet, RealmMultiBot bot) {
            NewTickPacket newTick = (NewTickPacket) packet;

            bot.World.OnNewTick(newTick);
            Move(newTick, bot);
            SetMoveMultiplier(bot);

            //TEMP
            if (bot.Map?.Name == "Vault") {
                if (!bot.GuildHallPortalFound)
                    FindGuildHall(bot);

                if (bot.GuildHallPortalFound) {
                    GameObject guildHallPortal = bot.Census.GetGameObject(bot.GuildHallPortalObjId);

                    if (guildHallPortal != null)
                        bot.AddMoveAction(new WorldPosData {X = guildHallPortal.X, Y = guildHallPortal.Y},
                            botCallback => {
                                Logger.Log("MultiBot NewTick", "Attempting to use Guild Hall Portal");
                                botCallback.Send(new UsePortalPacket {ObjectId = guildHallPortal.ObjectId});
                            });
                }
            }
        }

        private void Move(NewTickPacket newTick, RealmMultiBot bot) {
            if (bot.Player == null) return;

            bool moveFinished = false;
            if (bot.CurrentMoveAction != null) {
                WorldPosData newPos = bot.GetNextLocation(out moveFinished);

                if (newPos.X != bot.Player.X)
                    bot.Player.X = newPos.X;

                if (newPos.Y != bot.Player.Y)
                    bot.Player.Y = newPos.Y;
            }

            MovePacket move = new MovePacket {
                TickId = newTick.TickId,
                Time = bot.World.LastMoveTime = bot.World.LastUpdate,
                NewPosition = bot.Player.Position,
                Records = new MoveRecord[0]
            };

            bot.Send(move);

            if (moveFinished)
                bot.CurrentMoveAction?.OnActionFinished(bot);
        }

        private void SetMoveMultiplier(RealmMultiBot bot) {
            if (bot.Player == null) return;

            ushort? tileType = bot.Map.GetTile((int) bot.Player.X, (int) bot.Player.Y);
            if (tileType.HasValue) {
                TileData tile = GameContent.GetTileData(tileType.Value);
                if (tile == null) {
                    bot.Player.MoveMultiplier = 1d;
                    Logger.Log("MultiBot NewTick", $"Unkown tile! Type: {tileType.Value}", ConsoleColor.Yellow);
                }
                else {
                    if (tile.NoWalk)
                        bot.Player.MoveMultiplier = 0d;
                    else
                        bot.Player.MoveMultiplier = tile.Speed;
                }
            }
            else {
                bot.Player.MoveMultiplier = 1d;
            }
        }

        private void FindGuildHall(RealmMultiBot bot) {
            Portal[] portals = bot.Census.GetGameObjects<Portal>();

            foreach (Portal i in portals)
                if (i.Data.Type == _guildHallPortalData.Type) {
                    bot.GuildHallPortalObjId = i.ObjectId;
                    bot.GuildHallPortalFound = true;
#if DEBUG
                    Logger.Log("KeyBot NewTick", $"Guild Hall Portal Found at X: {i.X} Y: {i.Y}");
#endif
                    break;
                }
        }
    }
}