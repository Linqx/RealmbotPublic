using System;
using System.Collections.Generic;
using System.Linq;
using BotCore;
using Botcore.Data;
using BotCore.Game.Worlds.Entities;
using BotCore.Structures;
using BotTools;
using Rekishi;

namespace KeyBot.States {
    public class InitializeState : KeyBotState {
        private bool _startedLoadingVaults;
        private bool _finishedLoadingVaults;

        public InitializeState(RealmKeyBot bot) : base(bot, KeyBotStates.Initialize) {
        }

        public override void Start() {
            Log.Write($"[{Bot.Name}] Initializing Key Bot");
        }

        public override void Run() {
            LogKeys();

            if (!_startedLoadingVaults) {
                _startedLoadingVaults = true;
                LoadVaults();
            }

            if (_finishedLoadingVaults) Bot.SetState(new EnterGuildHallState(Bot));
        }

        public override void End() {
            Log.Write($"[{Bot.Name}] Initialization finished");
        }

        private void LogKeys() {
            Vault[] vaults = World.Census.GetGameObjects<Vault>();

            foreach (Vault vault in vaults)
            foreach (ItemData item in vault.Inventory) {
                if (item == null) continue;
                if (!Enum.IsDefined(typeof(KeyType), item.Type)) continue;
                if (Bot.KeyToPostion.ContainsKey(item.Type)) continue;

                Bot.KeyToPostion[item.Type] = vault.Position;
                Log.Write($"[Key Bot] Key Found: {item.Id} at X: {vault.X} Y: {vault.Y}");
            }
        }

        private void LoadVaults() {
            Log.Debug("[Key Bot] Loading vaults");
            Bot.AddMoveAction(Bot.Player.X - 15, Bot.Player.Y);
            Bot.AddMoveAction(Bot.Player.X + 15, Bot.Player.Y);
            Bot.AddMoveAction(Bot.Player.X, Bot.Player.Y - 5);
            Bot.AddMoveAction(Bot.Player.X, Bot.Player.Y, LoadVaultsFinished);
        }

        private void LoadVaultsFinished(RealmBot bot) {
            Log.Debug("[Key Bot] Finished loading vaults");
            _finishedLoadingVaults = true;

            /* No keys were found while loading the vault. */
            if (!Bot.KeyToPostion.Keys.Any()) {
                Log.Debug("[Key Bot] No keys were found during initialization");
                return;
            }

            var keys = Bot.KeyToPostion.Keys;
            List<string> availableKeys = new List<string>();

            foreach (ushort key in keys)
                if (Enum.IsDefined(typeof(KeyType), key))
                    availableKeys.Add(EnumHelper.GetStringValue((KeyType) key));

            if (availableKeys.Count <= 0) return;
            string keyList = string.Join(" ", availableKeys);
            Bot.AvailableKeyListTexts = ContentUtils
                .SplitInParts(keyList, RealmBot.MAX_PLAYER_TEXT_LENGTH, ' ', "/g ").ToArray();
        }
    }
}