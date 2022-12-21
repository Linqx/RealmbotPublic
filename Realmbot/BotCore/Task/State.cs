using System;
using BotCore.Game.Worlds;
using BotCore.Networking;
using Nekusasu;

namespace BotCore.Task {
    public class State<BotType, StateType> where BotType : RealmBot where StateType : Enum {

        public World World => Bot?.World;
        public BotType Bot { get; set; }
        public StateType States { get; set; }
        public State<BotType, StateType> PreviousState { get; set; }

        private PacketHandler<RotMGPacket> _stateHandlers;

        protected State(BotType bot, StateType states) {
            Bot = bot;
            States = states;
        }

        public virtual void Start() {
        }

        public virtual void Run() {
        }

        public virtual void End() {
        }

        public void TryHandlePacket(RotMGPacket packet) {
            _stateHandlers?.Handle(packet);
        }

        public virtual void Dispose() {
            _stateHandlers?.Dispose();
            _stateHandlers = null;
        }

        protected void Subcribe(PacketType id, PacketHandler<RotMGPacket>.HandlePacket handler) {
            if (_stateHandlers == null) {
                _stateHandlers = new PacketHandler<RotMGPacket>();
                _stateHandlers.LogError = false;
            }

            _stateHandlers.Map(RotMGPacket.PacketIds[id], handler);
        }
    }
}