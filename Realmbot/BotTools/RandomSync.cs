using System;

namespace BotTools {
    //from nr-core
    public class RandomSync {
        private uint _seed;

        public RandomSync(uint seed) {
            _seed = seed;
        }

        public uint NextInt() {
            return Gen();
        }

        public double NextDouble() {
            return Gen() / 2147483647.0;
        }

        public double NextNormal(double min = 0, double max = 1) {
            uint j = Gen() / 2147483647;
            uint k = Gen() / 2147483647;
            double l = Math.Sqrt(-2 * Math.Log(j)) * Math.Cos(2 * k * Math.PI);
            return min + l * max;
        }

        public uint NextIntRange(uint min, uint max) {
            uint ret = min == max ? min : min + Gen() % (max - min);
            return ret;
        }

        public double NextDoubleRange(double min, double max) {
            return min + (max - min) * NextDouble();
        }

        private uint Gen() {
            uint lb = 16807 * (_seed & 0xFFFF);
            uint hb = 16807 * (uint) ((int) _seed >> 16);
            lb = lb + ((hb & 32767) << 16);
            lb = lb + (uint) ((int) hb >> 15);
            if (lb > 2147483647) lb = lb - 2147483647;
            return _seed = lb;
        }
    }
}