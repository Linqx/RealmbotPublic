//using System;
//using System.Linq;
//using BotTools;

//namespace KeyBot.Handlers {
//    public class TextHandler : KeyBotPacketHandler {
//        public override PacketType Handles => PacketType.TEXT;

//        private readonly string[] _helpMessages = {
//            "/g [!help]: Shows this command",
//            "/g [!open <Key Type>]: Opens given key",
//            "/g [!chain <Key Type>]: Create 8 of the given key and chains them",
//            "/g [!list]: Shows list of all keys",
//            "/g [!keys]: Shows list of all keys",
//            "/g [!keylist]: Shows list of all keys",
//            "/g [!available]: Shows list of all avalaible keys",
//            "/g [!connection <Server Name>]: Connects to the given server",
//            "/g [!con <Server Name>]: Connects to the given server",
//            "/g [!conn <Server Name>]: Connects to the given server",
//            "/g [!whitelist: Shows list of all whitelisted players"
//        };

//        public override void Handle(Packet packet, RealmKeyBot bot) {
//            TextPacket text = (TextPacket) packet;

//            if (bot.Player == null)
//                return;

//            if (!bot.Starting) {
//                if (!string.IsNullOrWhiteSpace(text.Recipient)) {
//                    if (text.Recipient == "*Guild*")
//                        Logger.Log($"{text.Name} >> {text.Recipient}", text.Text, ConsoleColor.Green);
//                    else
//                        Logger.Log($"{text.Name} >> {text.Recipient}", text.Text, ConsoleColor.Cyan);
//                }
//                else {
//                    Logger.Log(text.Name, text.Text);
//                }

//                if (text.Text.StartsWith("!")) {
//                    string[] split = text.Text.Split(' ');

//                    HandleCommand(bot, split[0].Substring(1), text.Name, split.Where((_, i) => i > 0).ToArray());
//                }
//            }
//        }

//        public void HandleCommand(RealmKeyBot bot, string command, string player, string[] args) {
//            if (player == bot.Player.Name)
//                return;

//            if (!bot.WhiteList.Contains(player)) // Epic Sit
//            {
//                bot.SendText($"/g {player}, sit.");
//                return;
//            }

//            if (bot.ShouldDupe || bot.ShouldChainDupe) {
//                bot.SendText("/g Sorry, I'm currently busy! Please try again in a few minutes.");
//                return;
//            }

//            switch (command) {
//                case "help":
//                    bot.SendTexts(_helpMessages);
//                    break;
//                case "open":
//                    if (args.Length == 0) {
//                        bot.SendText("/g Make sure to use !help if you need guidance using my commands!");
//                        return;
//                    }

//                    if (ParseEnum.TryParse(args[0], out KeyType keyType)) {
//                        if (bot.AllKeys.Contains(args[0])) {
//                            ushort type = (ushort) keyType;
//                            if (bot.KeyToPostion.ContainsKey(type)) {
//                                bot.SendText("/g Sure can do!");

//                                if (bot.ChainKeyType == type) // Check if chaining
//                                {
//                                    bot.UseNextKey(); // Uses next key in chain
//                                    return;
//                                }

//                                bot.DropAllItems(); // Drop all current keys because chain is being interrupted 
//                                bot.ChainKeyType = -1; // with a different key type.

//                                bot.DupeKey = type;
//                                bot.ShouldDupe = true;
//                                bot.Reconnect(ConnectionMode.VAULT);
//                            }
//                            else {
//                                bot.SendText(
//                                    "/g Sorry, that key is not available. If you'd like to buy me one, please dm Danny or Evan on discord!");
//                            }
//                        }
//                        else {
//                            bot.SendText("/g Invalid key type! Use !keys, !list or !keylist for a list of all keys.");
//                        }
//                    }

//                    break;
//                case "chain":
//                    if (args.Length == 0) {
//                        bot.SendText("/g Use !help if you need guidance using my commands!");
//                        return;
//                    }

//                    if (ParseEnum.TryParse(args[0], out KeyType chainKeyType)) {
//                        if (bot.AllKeys.Contains(args[0])) {
//                            ushort type = (ushort) chainKeyType;
//                            if (bot.KeyToPostion.ContainsKey(type)) {
//                                bot.SendText("/g Sure can do!");

//                                bot.DropAllItems();

//                                bot.DupeKey = type;
//                                bot.ShouldChainDupe = true;
//                                bot.ChainKeyType = type;
//                                bot.Reconnect(ConnectionMode.VAULT);
//                            }
//                            else {
//                                bot.SendText(
//                                    "/g Sorry, that key is not available. If you'd like to buy me one, please dm Danny or Evan on discord.");
//                            }
//                        }
//                        else {
//                            bot.SendText("/g Invalid key type! Use !keys, !list or !keylist for a list of all keys.");
//                        }
//                    }

//                    break;
//                case "list":
//                case "keys":
//                case "keylist":
//                    bot.SendTexts(bot.KeyListTexts);
//                    break;
//                case "available":
//                    bot.SendTexts(bot.AvailableKeyListTexts);
//                    break;
//                case "connect":
//                case "con":
//                case "conn":
//                    if (args.Length == 0) {
//                        bot.SendText("/g Invalid server name!");
//                    }
//                    else {
//                        if (ParseEnum.TryParse(args[0], out Server server)) {
//                            bot.SendText(
//                                $"/g Roger! Connecting to: {server.ToString()} ({EnumHelper.GetStringValue(server)}).");
//                            bot.Reconnect(server, ConnectionMode.VAULT);
//                            bot.SkipLoadingVaults = true;
//                            bot.Starting = true;
//                            bot.StartingFinished = false;
//                        }
//                        else {
//                            bot.SendText("/g Invalid server name!");
//                        }
//                    }

//                    break;
//                case "whitelist":
//                    bot.SendTexts(bot.WhiteListTexts);
//                    break;
//            }
//        }
//    }
//}