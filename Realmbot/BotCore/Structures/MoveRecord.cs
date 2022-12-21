using Shio;

namespace BotCore.Structures {
    public class MoveRecord {
        public int Time { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public void Write(PacketWriter wtr) {
            wtr.Write(Time);
            wtr.Write(X);
            wtr.Write(Y);
        }
    }
}