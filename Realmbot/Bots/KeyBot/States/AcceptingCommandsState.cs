using System.Linq;
using BotCore.Networking;
using BotCore.Structures;
using BotTools;
using Rekishi;

namespace KeyBot.States {
    public class AcceptingCommandsState : KeyBotState {
        private readonly string[] _helpMessages = {
            "/g [!help]: Shows this command",
            "/g [!open <Key Type>]: Opens given key",
            "/g [!keys]: Shows list of all available keys",
            "/g [!connect <Server Name>]: Connects to the given server",
            "/g [!server]: Displays the current server",
            "/g [!whitelist: Shows list of all whitelisted players"
        };

        private readonly string[] _confirmationMessages = {
            "/g Sure can do!",
            "/g Of course!",
            "/g No problemo!",
            "/g Affirmative!",
            "/g Kk",
            "/g Yes master!",
            "/g Ok senpai... UwU"
        };

        private bool _sendingHelp;

        public AcceptingCommandsState(RealmKeyBot bot) : base(bot, KeyBotStates.AcceptingCommands) {
        }

        public override void Start() {
            Subcribe(PacketType.TEXT, OnText);
        }

        public override void Run() {
        }

        public override void End() {
        }

        private void OnText(RotMGPacket packet) {
            TextPacket text = (TextPacket) packet;

            if (text.Name == Bot.Player?.Name)
                if (text.Text.Contains("[!whitelist: Shows list of all whitelisted players")) {
                    _sendingHelp = false; // finish sending help
                    return;
                }

            if (!text.Text.StartsWith("!")) return;
            string[] split = text.Text.Split(' ');
            HandleCommand(split[0].Substring(1), text.Name, split.Where((_, i) => i > 0).ToArray());
        }

        public void HandleCommand(string command, string player, string[] args) {
            if (player == Bot.Player.Name) return;
            if (_sendingHelp) return;

            if (!Bot.WhiteList.Contains(player)) // Epic Sit
            {
                Bot.SendText($"/g {player}, sit.");
                return;
            }

            switch (command) {
                case "help":
                    _sendingHelp = true;
                    Bot.SendTexts(_helpMessages);
                    break;
                case "open":
                    if (args.Length == 0) {
                        Bot.SendText("/g Make sure to use !help if you need guidance using my commands!");
                        return;
                    }

                    if (!ParseEnum.TryParse(args[0], out KeyType keyType)) {
                        Bot.SendText("/g Invalid key type! Use !keys for a list of available keys.");
                        return;
                    }

                    if (!Bot.KeyToPostion.ContainsKey((ushort) keyType)) {
                        Bot.SendText(
                            "/g Sorry, that key is not available. If you'd like to buy me one, please dm Danny or Evan on discord!");
                        return;
                    }

                    Bot.SendText(_confirmationMessages[Randum.Next(_confirmationMessages.Length)]);
                    Delay.RunSeconds(1f, () => {
                        Bot.Reconnect(ConnectionMode.VAULT);
                        Bot.SetState(new GetKeyState(Bot));
                    });

                    break;
                case "keys":
                    if (Bot.AvailableKeyListTexts == null) {
                        Bot.SendText("/g No keys were found on this account!");
                        return;
                    }

                    Bot.SendTexts(Bot.AvailableKeyListTexts);
                    break;
                case "connect":
                case "con":
                case "conn":
                    if (args.Length == 0) {
                        Bot.SendText("/g No server was specified!");
                        return;
                    }

                    if (!ParseEnum.TryParse(args[0], out Server server)) {
                        Bot.SendText("/g Invalid server name!");
                        return;
                    }

                    Bot.SendText($"/g Roger! Connecting to: {server.ToString()} ({EnumHelper.GetStringValue(server)})");
                    Delay.RunSeconds(1f, () => {
                        Bot.Reconnect(server, ConnectionMode.VAULT);
                        Bot.SetState(new EnterGuildHallState(Bot));
                    });
                    break;
                case "whitelist":
                    Bot.SendTexts(Bot.WhiteListTexts);
                    break;
            }
        }
    }
}