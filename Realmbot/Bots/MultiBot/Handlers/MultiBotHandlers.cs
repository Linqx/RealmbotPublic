using System;
using System.Collections.Generic;

namespace MultiBot.Handlers {
    public abstract class MultiBotHandler {
        public abstract PacketType Handles { get; }
        public abstract void Handle(Packet packet, RealmMultiBot bot);
    }

    public class MultiBotHandlers {
        private DefaultPacketHandlers _defaultHandlers = new DefaultPacketHandlers();
        private readonly Dictionary<PacketType, MultiBotHandler> _handlers;

        public MultiBotHandlers() {
            _handlers = new Dictionary<PacketType, MultiBotHandler>();
            foreach (Type type in typeof(MultiBotHandler).Assembly.GetTypes())
                if (typeof(MultiBotHandler).IsAssignableFrom(type) && !type.IsAbstract) {
                    MultiBotHandler handler = (MultiBotHandler) Activator.CreateInstance(type);
                    _handlers.Add(handler.Handles, handler);
                }
        }

        public void HandlePacketPostFrame(Packet packet, RealmMultiBot bot) {
            if (_handlers.ContainsKey(packet.Type))
                bot.QueuePacket(packet);
            else
                _defaultHandlers.HandlePacketPostFrame(packet, bot);
        }

        public void HandlePacket(Packet packet, RealmMultiBot bot) {
            if (_handlers.ContainsKey(packet.Type))
                _handlers[packet.Type].Handle(packet, bot);
            else
                _defaultHandlers.HandlePacket(packet, bot);
        }
    }
}