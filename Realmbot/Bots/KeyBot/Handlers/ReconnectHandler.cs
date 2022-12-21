//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BotTools;

//namespace KeyBot.Handlers {
//    public class ReconnectHandler : KeyBotPacketHandler {
//        public override PacketType Handles => PacketType.RECONNECT;

//        public override void Handle(Packet packet, RealmKeyBot bot) {
//            ReconnectPacket reconnect = (ReconnectPacket) packet;

//            Logger.Log("Key Reconnect", reconnect.ToString(), ConsoleColor.Cyan);

//            #region Key Texts Sorting

//            if (bot.Starting && !bot.SkipLoadingVaults) {
//                var keys = bot.KeyToPostion.Keys;
//                List<string> availableKeys = new List<string>();

//                foreach (ushort key in keys)
//                    if (Enum.IsDefined(typeof(KeyType), key))
//                        availableKeys.Add(EnumHelper.GetStringValue((KeyType) key));

//                if (availableKeys.Count > 0) {
//                    string keyList = string.Join(" ", availableKeys);
//                    bot.AvailableKeyListTexts = ContentUtils
//                        .SplitInParts(keyList, RealmBot.MAX_PLAYER_TEXT_LENGTH, ' ', "/g ").ToArray();
//                }
//                else {
//                    availableKeys.Add(
//                        "/g Sorry, I don't have any available keys! Don't forget to reset me once I have a key!");
//                    bot.AvailableKeyListTexts = availableKeys.ToArray();
//                }
//            }

//            #endregion

//            if (bot.Starting)
//                bot.Starting = false;

//            if (bot.ShouldDupe) {
//                bot.ShouldDupe = false;
//                bot.DupingFinished = false;
//            }

//            if (bot.ShouldChainDupe) {
//                bot.ShouldChainDupe = false;
//                bot.DupingFinished = false;
//            }

//            bot.Reconnect(reconnect.Host, reconnect.Port, reconnect.GameId, reconnect.KeyTime, reconnect.Key);
//        }
//    }
//}