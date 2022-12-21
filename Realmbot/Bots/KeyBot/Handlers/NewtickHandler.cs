//using System;
//using BotTools;

//namespace KeyBot.Handlers {
//    public class NewTickHandler : KeyBotPacketHandler {

//        public override PacketType Handles => PacketType.NEWTICK;
//        private readonly PortalData _guildHallPortalData = GameContent.GetPortalData("Guild Hall Portal");

//        public override void Handle(Packet packet, RealmKeyBot bot) {
//            NewTickPacket newTick = (NewTickPacket) packet;

//            bot.Map.HandleNewTick(newTick);

//            Move(newTick, bot);
//            SetMoveMultiplier(bot);

//            if (bot.Map?.Name == "Vault") {
//                if (bot.Starting)
//                    LogKeys(bot);

//                if (!bot.GuildHallPortalFound)
//                    FindGuildHall(bot);

//                if (bot.Starting && !bot.StartingFinished)
//                    if (bot.GuildHallPortalFound) {
//                        GameObject guildHallPortal = bot.Map.GetGameObject(bot.GuildHallPortalObjId);

//                        LoadVaults(bot, guildHallPortal);
//                        EnterGuildHall(bot, guildHallPortal);
//                    }

//                if (!bot.Starting) {
//                    if (!bot.GrabbingKey && (bot.ShouldDupe || bot.ShouldChainDupe))
//                        GrabKey(bot);

//                    if (bot.HasKey && !bot.DupingFinished && !bot.Duping)
//                        Dupe(bot);

//                    if (bot.DupingFinished && !bot.ShouldDupe && !bot.ShouldChainDupe && bot.GuildHallPortalFound)
//                        AfterDupe(bot);
//                }
//            }
//        }

//        private void Move(NewTickPacket newTick, RealmKeyBot bot) {
//            if (bot.Player == null) return;

//            bool moveFinished = false;
//            if (bot.CurrentMoveAction != null) {
//                WorldPosData newPos = bot.GetNextLocation(out moveFinished);

//                if (newPos.X != bot.Player.X)
//                    bot.Player.X = newPos.X;

//                if (newPos.Y != bot.Player.Y)
//                    bot.Player.Y = newPos.Y;
//            }

//            MovePacket move = new MovePacket {
//                TickId = newTick.TickId,
//                Time = bot.Map.LastMoveTime = bot.Map.LastUpdate,
//                NewPosition = new WorldPosData {X = bot.Player.X, Y = bot.Player.Y},
//                Records = new MoveRecord[0]
//            };

//            bot.Send(move);

//            if (moveFinished)
//                bot.CurrentMoveAction?.OnActionFinished(bot);
//        }

//        private void SetMoveMultiplier(RealmKeyBot bot) {
//            if (bot.Player == null) return;

//            ushort? tileType = bot.Map.GetTile((int) bot.Player.X, (int) bot.Player.Y);
//            if (tileType.HasValue) {
//                TileData tile = GameContent.GetTileData(tileType.Value);
//                if (tile == null) {
//                    bot.Player.MoveMultiplier = 1d;
//                    Logger.Log("KeyBot NewTick", $"Unkown tile! Type: {tileType.Value}", ConsoleColor.Yellow);
//                }
//                else {
//                    if (tile.NoWalk)
//                        bot.Player.MoveMultiplier = 0d;
//                    else
//                        bot.Player.MoveMultiplier = tile.Speed;
//                }
//            }
//            else {
//                bot.Player.MoveMultiplier = 1d;
//            }
//        }

//        private void LogKeys(RealmKeyBot bot) {
//            Vault[] vaults = bot.Map.GetGameObjects<Vault>();

//            foreach (Vault vault in vaults)
//            foreach (ItemData item in vault.Inventory) {
//                if (item == null)
//                    continue;

//                if (Enum.IsDefined(typeof(KeyType), item.Type))
//                    if (!bot.KeyToPostion.ContainsKey(item.Type)) {
//                        bot.KeyToPostion.Add(item.Type, vault.Position);

//                        Logger.Log("Key NewTick", $"New Key Found: {item.Id} at X: {vault.X} Y: {vault.Y}",
//                            ConsoleColor.White);
//                    }
//            }
//        }

//        private void FindGuildHall(RealmKeyBot bot) {
//            Portal[] portals = bot.Map.GetGameObjects<Portal>();

//            foreach (Portal i in portals)
//                if (i.Data.Type == _guildHallPortalData.Type) {
//                    bot.GuildHallPortalObjId = i.ObjectId;
//                    bot.GuildHallPortalFound = true;
//#if DEBUG
//                    Logger.Log("KeyBot NewTick", $"Guild Hall Portal Found at X: {i.X} Y: {i.Y}");
//#endif
//                    break;
//                }
//        }

//        private void LoadVaults(RealmKeyBot bot, GameObject guildHallPortal) {
//            if (bot.GuildHallPortalFound) {
//                bot.StartingFinished = true;

//                if (guildHallPortal != null)
//                    if (!bot.SkipLoadingVaults) {
//                        bot.AddMoveAction(new WorldPosData {X = bot.Player.X - 15f, Y = bot.Player.Y});
//                        bot.AddMoveAction(new WorldPosData {X = bot.Player.X + 15f, Y = bot.Player.Y});
//                    }
//            }
//        }

//        private void EnterGuildHall(RealmKeyBot bot, GameObject guildHallPortal) {
//            bot.AddMoveAction(new WorldPosData {X = guildHallPortal.X, Y = guildHallPortal.Y}, botCallback => {
//                Logger.Log("KeyBot NewTick", "Attempting to use Guild Hall Portal");
//                botCallback.Send(new UsePortalPacket {ObjectId = guildHallPortal.ObjectId});
//            });
//        }

//        private void GrabKey(RealmKeyBot bot) {
//            bot.GrabbingKey = true;

//            if (bot.KeyToPostion.TryGetValue(bot.DupeKey, out WorldPosData vaultPos)) {
//                Vault vault = (Vault) bot.Map.GetGameObject(vaultPos.X, vaultPos.Y);
//                int slotWithKey = -1;

//                for (int i = 0; i < vault.Inventory.Count(); i++) {
//                    ItemData item = vault.Inventory[i];

//                    if (item == null)
//                        continue;

//                    if (item.Type == bot.DupeKey) {
//                        slotWithKey = i;
//                        break;
//                    }
//                }

//                bot.AddMoveAction(vaultPos, botCallback => {
//                    Delay.RunSeconds(0.5, () => {
//                        //Logger.Log("Key New Tick", "Key Grabbed");
//                        bot.InvSwap(4, slotWithKey, vault.ObjectId,
//                            vault.Inventory); // Player's inventory SHOULD be empty at this point.
//                        bot.HasKey = true;
//                    });
//                });
//            }
//            else // *Should* never happen
//            {
//                Logger.Log("Key New Tick Handler", "ERROR: Tried to grab invalid key! Stopping bot!");
//                bot.Disconnect();
//            }
//        }

//        private void Dupe(RealmKeyBot bot) {
//            bot.Duping = true;

//            if (bot.ShouldDupe) {
//            }

//            if (bot.ShouldChainDupe) {
//            }
//        }

//        private void AfterDupe(RealmKeyBot bot) {
//            bot.DupingFinished = false;

//            GameObject guildHallPortal = bot.Map.GetGameObject(bot.GuildHallPortalObjId);

//            bot.AddMoveAction(new WorldPosData {X = guildHallPortal.X, Y = guildHallPortal.Y}, botCallback => {
//                botCallback.Send(new UsePortalPacket {ObjectId = guildHallPortal.ObjectId});
//                //Logger.Log("Key Bot Update Handler", "Using Guild Hall Poral");
//            });
//        }
//    }
//}