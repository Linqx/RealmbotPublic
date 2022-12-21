using System;
using BotTools;

namespace LocalBot.Handlers {
    public class NewTickHandler : LocalBotHandler {
        public override PacketType Handles => PacketType.NEWTICK;

        public override void Handle(Packet packet, RealmLocalBot bot) {
            NewTickPacket newTick = (NewTickPacket) packet;

            bot.Map.HandleNewTick(newTick);

            #region Moving

            bool moveFinished = false;
            if (bot.CurrentMoveAction != null) {
                WorldPosData newPos = bot.GetNextLocation(out moveFinished);

                if (newPos.X != bot.Player.X)
                    bot.Player.X = newPos.X;

                if (newPos.Y != bot.Player.Y)
                    bot.Player.Y = newPos.Y;
            }

            //Logger.Log("Key New Tick", $"X: {bot.Player.X} Y: {bot.Player.Y}");

            MovePacket move = new MovePacket {
                TickId = newTick.TickId,
                Time = bot.Map.LastMoveTime = bot.Map.LastUpdate,
                NewPosition = new WorldPosData {X = bot.Player.X, Y = bot.Player.Y},
                Records = new MoveRecord[0]
            };

            //Logger.Log("New Tick Handler", move.ToString());

            bot.Send(move);

            // Handle after MovePacket is set because server anti-cheat
            if (moveFinished)
                bot.CurrentMoveAction?.OnActionFinished(bot);

            #region MoveMultiplier

            ushort? tileType = bot.Map.GetTile((int) bot.Player.X, (int) bot.Player.Y);
            if (tileType.HasValue) {
                TileData tile = GameContent.GetTileData(tileType.Value);
                if (tile == null) {
                    bot.Player.MoveMultiplier = 1d;
                    Logger.Log("New Tick Handler", $"Unkown tile! Type: {tileType.Value}", ConsoleColor.Yellow);
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

            #endregion

            #endregion
        }
    }
}