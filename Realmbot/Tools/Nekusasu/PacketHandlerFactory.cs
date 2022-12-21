using System;
using System.Collections.Generic;
using System.Linq;
using Networking.TCP;

namespace Networking {
    public class PacketHandlerFactory<TConnection, THandler, TPacket>
        where TPacket : Packet
        where TConnection : NetConnection<TPacket>
        where THandler : IPacketHandler<TConnection, TPacket> {
        private readonly IPacketHandler<TConnection, TPacket>[] handlerTypes;

        public PacketHandlerFactory() {
            Type handlerType = typeof(THandler).GetGenericTypeDefinition();
            IEnumerable<IPacketHandler<TConnection, TPacket>> handlers = handlerType.Assembly.GetTypes()
                .Where(_ => IsPacketHandler(_, handlerType))
                .Select(_ => (IPacketHandler<TConnection, TPacket>) Activator.CreateInstance(_));
            handlerTypes = new IPacketHandler<TConnection, TPacket>[256];

            foreach (IPacketHandler<TConnection, TPacket> handler in handlers)
                handlerTypes[handler.Id] = handler;
        }

        private static bool IsPacketHandler(Type sub, Type baseClass) {
            Type baseType = sub.BaseType;
            if (baseType == null) return false;
            if (!baseType.IsAbstract) return false;
            return baseType.IsGenericType && baseType.GetGenericTypeDefinition() == baseClass;
        }

        public void Handle(TPacket packet, TConnection connection) {
            IPacketHandler<TConnection, TPacket> handler = handlerTypes[packet.Id];
            handler?.Handle(packet, connection);
        }
    }
}