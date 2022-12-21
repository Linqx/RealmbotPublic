using System;
using System.Collections.Generic;
using System.Linq;

namespace Networking {
    public class PacketFactory<TPacket> where TPacket : Packet {

        public int TypeCount => PacketTypes.Count;
        protected Dictionary<byte, Type> PacketTypes;

        public PacketFactory() {
            PacketTypes = GetPacketTypes();
        }

        protected virtual Dictionary<byte, Type> GetPacketTypes() {
            Type t = typeof(TPacket);
            return t.Assembly.GetTypes().Where(_ => _.IsSubclassOf(t))
                .ToDictionary(_ => ((TPacket) Activator.CreateInstance(_)).Id);
        }

        public virtual TPacket CreatePacket(byte id) {
            if (!PacketTypes.TryGetValue(id, out Type type))
                return null;
            return (TPacket) Activator.CreateInstance(type);
        }
    }
}