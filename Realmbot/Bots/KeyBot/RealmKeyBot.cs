using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BotCore;
using BotCore.Networking;
using BotCore.Structures;
using BotTools;
using KeyBot.States;
using Rekishi;

namespace KeyBot {
    public class RealmKeyBot : RealmBot {

        public string[] WhiteList {
            get => _whiteList;
            set {
                _whiteList = value;
                WhiteListTexts = ContentUtils.SplitInParts(string.Join(", ", WhiteList), MAX_PLAYER_TEXT_LENGTH, ' ', "/g ");
            }
        }

        public bool Starting = true;
        public bool StartingFinished = false;
        public bool ShouldDupe = false;
        public ushort DupeKey;
        public bool ShouldChainDupe = false;
        public int ChainKeyType = -1;
        public bool DupingFinished;
        public Dictionary<ushort, WorldPosData> KeyToPostion;
        public string[] KeyListTexts;
        public string[] AllKeys;
        public string[] AvailableKeyListTexts;
        public string[] WhiteListTexts;
        public bool SkipLoadingVaults = false;
        public bool GrabbingKey = false;
        public bool HasKey = false;
        public bool Duping;
        public Action<string> ChangeTitle;
        public KeyBotState CurrentState;

        private string _mapName;
        private string[] _whiteList;
        private readonly Queue<KeyBotState> _stateQueue;

        public RealmKeyBot(string email, string password) : base(email, password) {
            Name = "Key Bot";
            _stateQueue = new Queue<KeyBotState>();

            HandleFromState = HandlePacketFromState;

            Setup();
        }

        private void Setup() {
            KeyToPostion = new Dictionary<ushort, WorldPosData>();
            AllKeys = ((KeyType[]) Enum.GetValues(typeof(KeyType))).Select(_ => EnumHelper.GetStringValue(_))
                .ToArray(); // Gets name of all keys in the KeyType enum
            string keyList = string.Join(" ", AllKeys);
            KeyListTexts =
                ContentUtils.SplitInParts(keyList, MAX_PLAYER_TEXT_LENGTH, ' ',
                    "/g "); // Seperates the key list into a split array
        } // so the servers don't cut anything out

        private void HandlePacketFromState(RotMGPacket packet) {
            CurrentState?.TryHandlePacket(packet);
        }

        protected override void SubscribePackets(RotMGClient client) {
            client.Subscribe(PacketType.MAPINFO, OnMapInfo);
            client.Subscribe(PacketType.NEWTICK, OnNewTick);
        }


        private void OnMapInfo(RotMGPacket packet) {
            MapInfoPacket mapInfo = (MapInfoPacket) packet;
            _mapName = mapInfo.Name;
            SetTitle();
        }

        private void SetTitle() {
            StringBuilder title = new StringBuilder();
            title.Append($"Key Bot Program - ");

            if (CurrentState != null)
                title.Append($"{CurrentState.States.ToString()}, ");

            title.Append($"{ConnectedServerName}, {_mapName}");

            ChangeTitle?.Invoke(title.ToString());
        }

        private void OnNewTick(RotMGPacket packet) {
            if (CurrentState == null)
                SetState(new InitializeState(this));
        }

        public void Update(int time, int dt) {
            if (_stateQueue.Any()) {
                KeyBotState previousState = CurrentState;
                previousState?.End();
                previousState?.Dispose();

                CurrentState = _stateQueue.Dequeue();
                CurrentState.PreviousState = previousState;
                CurrentState.Start();
                SetTitle();
            }

            CurrentState?.Run();
        }

        public void SetState(KeyBotState state) {
            Log.Debug($"Queued state: {state.States}");
            _stateQueue.Enqueue(state);
        }
    }
}