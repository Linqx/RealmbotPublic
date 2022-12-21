using System;
using System.Collections.Generic;
using System.Linq;
using BotCore;
using Botcore.Content;
using BotCore.Networking;
using BotDataExtracter;
using BotTools;

namespace BotProgram {
    public class BotManager {
        public static string BuildVersion;

        private readonly Dictionary<int, RealmBot> _bots;
        private readonly object _sync = new object();
        private int _nextId;
        private bool stopped;

        public BotManager() {
            _bots = new Dictionary<int, RealmBot>();
        }

        public void Start() {
            if (DataExtracter.NewVersion()) {
                DataExtracter dataExtracter =
                    new DataExtracter(ExtractionType.OBJECTS | ExtractionType.TILES | ExtractionType.PACKETS);
                dataExtracter.Extract();
                dataExtracter.Dispose();

                Logger.Log("Bot Manager", "Successfully updated!", ConsoleColor.Cyan);
            }

            DataExtracter.SetPacketIds();
            DataExtracter.SetBuildVersion();

            Dictionary<PacketType, byte> dict = new Dictionary<PacketType, byte>();
            foreach (KeyValuePair<string, byte> type in DataExtracter.PacketTypes) {
                string packet = type.Key;
                byte id = type.Value;

                if (Enum.TryParse(packet, out PacketType packetType))
                    dict.Add(packetType, id);
            }

            RotMGPacket.PacketIds = dict;

            BuildVersion = DataExtracter.BuildVersion;
            GameContent.Initialize();
        }

        public void Stop() {
            if (stopped) return;

            stopped = true;
            RealmBot[] bots = _bots.Values.ToArray();

            foreach (RealmBot bot in bots)
                RemoveBot(bot.Id);
        }

        public Bot GetBot<Bot>(string email) where Bot : RealmBot {
            if (stopped) return null;

            lock (_sync) {
                RealmBot[] bots = _bots.Values.Where(_ => _.Email == email).ToArray();

                if (bots.Length > 0)
                    return (Bot) bots[0];
                return null;
            }
        }

        public Bot[] GetBots<Bot>() where Bot : RealmBot {
            if (stopped) return null;

            lock (_sync) {
                return _bots.Values.Select(_ => (Bot) _).ToArray();
            }
        }

        public Bot AddBot<Bot>(string email, string password, params object[] arguments) where Bot : RealmBot {
            if (stopped) return null;

            Bot bot;

            lock (_sync) {
                object[] args = new object[] {email, password}.Concat(arguments).ToArray();
                bot = (Bot) Activator.CreateInstance(typeof(Bot), args);
                bot.OnDisconnect = BotDisconnected;
                bot.OnTryReconnect = BotReconnected;
                bot.BuildVersion = BuildVersion;

                bot.Id = ++_nextId;
                _bots.Add(_nextId, bot);

#if DEBUG
                Logger.Log("Bot Manager", $"Bot: {email} added");
#endif
            }

            return bot;
        }

        public Bot AddBot<Bot>(Bot bot) where Bot : RealmBot {
            if (stopped) return null;

            lock (_sync) {
                bot.OnDisconnect = BotDisconnected;
                bot.OnTryReconnect = BotReconnected;

                bot.Id = ++_nextId;
                _bots.Add(_nextId, bot);

#if DEBUG
                Logger.Log("Bot Manager", $"Bot: {bot.Email} added");
#endif
            }

            return bot;
        }

        public void RemoveBot(int id) {
            lock (_sync) {
                if (_bots.TryGetValue(id, out RealmBot bot)) {
                    RotMGPacket[] flush = bot.FlushPackets();
                    if (flush != null)
                        foreach (RotMGPacket packet in flush)
                            if (packet is FailurePacket)
                                bot.Client?.Handle(packet);

                    bot.Dispose();
                    _bots.Remove(id);
#if DEBUG
                    Logger.Log("Bot Manager",
                        $"Bot: {bot.Email} removed from {bot.ConnectedServerName} @ {bot.CurrentServer}" +
                        (bot.Player != null ? $" at Position: {bot.Player.Position}" : string.Empty),
                        ConsoleColor.Red);
#endif
                }
                else {
                    Logger.Log("Bot Manager", $"Failed to remove bot: {id}", ConsoleColor.Red);
                }
            }
        }

        public void RemoveBot(string email) {
            lock (_sync) {
                RealmBot[] bots = _bots.Values.Where(_ => _.Email == email).ToArray();

                if (bots.Length == 0) {
                    Logger.Log("Bot Manager", $"No bot found with the email: {email}", ConsoleColor.Red);
                    return;
                }

                foreach (RealmBot bot in bots)
                    if (_bots.TryGetValue(bot.Id, out RealmBot dummy))
                        dummy.OnDisconnect?.Invoke(dummy);
                    else
                        Logger.Log("Bot Manager", $"Failed to remove bot: {bot.Id}", ConsoleColor.Red);
            }
        }

        public void BotDisconnected(RealmBot bot) {
            if (!bot.Disposed)
                RemoveBot(bot.Id);
        }

        public void BotReconnected(RealmBot bot) {
            AddBot(bot);
        }
    }
}