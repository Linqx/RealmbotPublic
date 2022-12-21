using System;
using System.Collections.Generic;

namespace LocalBot {
    public abstract class LocalBotHandler {
        public abstract PacketType Handles { get; }
        public abstract void Handle(Packet packet, RealmLocalBot bot);
    }

    public class LocalBotHandlers {
        private DefaultPacketHandlers _defaultHandlers = new DefaultPacketHandlers();
        private readonly Dictionary<PacketType, LocalBotHandler> _handlers;

        public LocalBotHandlers() {
            _handlers = new Dictionary<PacketType, LocalBotHandler>();
            foreach (Type type in typeof(LocalBotHandler).Assembly.GetTypes())
                if (typeof(LocalBotHandler).IsAssignableFrom(type) && !type.IsAbstract) {
                    LocalBotHandler handler = (LocalBotHandler) Activator.CreateInstance(type);
                    _handlers.Add(handler.Handles, handler);
                }
        }

        public void HandlePacketPostFrame(Packet packet, RealmLocalBot bot) {
            if (_handlers.ContainsKey(packet.Type))
                bot.QueuePacket(packet);
            else
                _defaultHandlers.HandlePacketPostFrame(packet, bot);
        }

        public void HandlePacket(Packet packet, RealmLocalBot bot) {
            if (_handlers.ContainsKey(packet.Type))
                _handlers[packet.Type].Handle(packet, bot);
            else
                _defaultHandlers.HandlePacket(packet, bot);
        }
    }
}