using System;
using System.Text;
using System.Threading;

namespace BotTools {
    public class Randum {

        public enum RandomStringType {
            [StringValue("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")]
            LETTERS,

            [StringValue("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")]
            LETTS_AND_NUMBERS
        }

        private static int Seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> RandomPool =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref Seed)));

        private static Random GetRandom() {
            return RandomPool.Value;
        }

        public static uint NextUInt32() {
            return ((uint) Next(1 << 30) << 2) | (uint) Next(1 << 2);
        }

        public static double NextDouble() {
            return GetRandom().NextDouble();
        }

        public static int Next(int Max) {
            return GetRandom().Next(Max);
        }

        public static double Next(double Max) {
            return GetRandom().NextDouble() * Max;
        }

        public static int Next(int Min, int Max) {
            return GetRandom().Next(Min, Max);
        }

        public static double Next(double Min, double Max) {
            return Min + GetRandom().NextDouble() * (Max - Min);
        }

        public static string NextString(int min, int max, RandomStringType stringType) {
            int length = GetRandom().Next(min, max);
            StringBuilder stringBuilder = new StringBuilder();

            string charsToUse = EnumHelper.GetStringValue(stringType);

            for (int i = 0; i < length; i++)
                stringBuilder.Append(charsToUse[Next(charsToUse.Length)]);

            return stringBuilder.ToString();
        }
    }
}