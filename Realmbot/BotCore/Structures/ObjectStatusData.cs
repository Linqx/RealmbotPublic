using System.Collections.Generic;
using RotMGCore.Structures.Game;
using Shio;

namespace BotCore.Structures {
    public class ObjectStatusData {
        public int ObjectId { get; set; }
        public WorldPosData Position { get; set; }
        public Dictionary<StatDataType, object> Stats { get; set; }

        public void Read(PacketReader rdr) {
            ObjectId = rdr.ReadInt32();
            Position = WorldPosData.Parse(rdr);

            int statsCount = rdr.ReadInt16();
            Stats = new Dictionary<StatDataType, object>();
            for (int i = 0; i < statsCount; i++) {
                StatDataType statData = (StatDataType) rdr.ReadByte();

                if (StatDataUtils.IsUTF(statData))
                    Stats[statData] = rdr.ReadUTF();
                else
                    Stats[statData] = rdr.ReadInt32();
            }
        }
    }
}