using System;
using Shio;

namespace BotCore.Structures {
    public struct WorldPosData {
        public double X;
        public double Y;

        public float xF;
        public float yF;

        public static WorldPosData Add(WorldPosData a, WorldPosData b) {
            return new WorldPosData(a.X + b.X, a.Y + b.Y);
        }

        public static WorldPosData Add(WorldPosData a, double value) {
            return new WorldPosData(a.X + value, a.Y + value);
        }

        public static WorldPosData Subtract(WorldPosData a, WorldPosData b) {
            return new WorldPosData(a.X - b.X, a.Y - b.Y);
        }

        public static WorldPosData Subtract(WorldPosData a, double value) {
            return new WorldPosData(a.X - value, a.Y - value);
        }

        public static WorldPosData Multiply(WorldPosData a, WorldPosData b) {
            return new WorldPosData(a.X * b.X, a.Y * b.Y);
        }

        public static WorldPosData Multiply(WorldPosData a, double value) {
            return new WorldPosData(a.X * value, a.Y * value);
        }

        public static WorldPosData Divide(WorldPosData a, WorldPosData b) {
            return new WorldPosData(a.X / b.X, a.Y / b.Y);
        }

        public static WorldPosData Divide(WorldPosData a, double value) {
            return new WorldPosData(a.X / value, a.Y / value);
        }

        public static WorldPosData operator +(WorldPosData a, WorldPosData b) {
            return Add(a, b);
        }

        public static WorldPosData operator +(WorldPosData a, double b) {
            return Add(a, b);
        }

        public static WorldPosData operator -(WorldPosData a, WorldPosData b) {
            return Subtract(a, b);
        }

        public static WorldPosData operator -(WorldPosData a, double b) {
            return Subtract(a, b);
        }

        public static WorldPosData operator *(WorldPosData a, WorldPosData b) {
            return Multiply(a, b);
        }

        public static WorldPosData operator *(WorldPosData a, double b) {
            return Multiply(a, b);
        }

        public static WorldPosData operator *(WorldPosData a, int b) {
            return Multiply(a, b);
        }

        public static WorldPosData operator *(int a, WorldPosData b) {
            return Multiply(b, a);
        }

        public static WorldPosData operator /(WorldPosData a, WorldPosData b) {
            return Divide(a, b);
        }

        public static WorldPosData operator /(WorldPosData a, double b) {
            return Divide(a, b);
        }

        public static bool operator ==(WorldPosData a, WorldPosData b) {
            return !(a != b);
        }

        public static bool operator !=(WorldPosData a, WorldPosData b) {
            return Math.Abs(a.X - b.X) > 0.001 || Math.Abs(a.Y - b.Y) > 0.001;
        }

        public static WorldPosData Zero = new WorldPosData(0);
        public double Length => Math.Sqrt(X * X + Y * Y);

        public double Distance(WorldPosData other) {
            return (other - this).Length;
        }

        public double DotProduct(WorldPosData other) {
            return X * other.X + Y * other.Y;
        }

        public WorldPosData Absolute() {
            return new WorldPosData(Math.Abs(X), Math.Abs(Y));
        }

        public double GetAngle(WorldPosData pos) {
            double deltaX = pos.X - X;
            double deltaY = pos.Y - Y;
            return Math.Atan2(deltaY, deltaX);
        }

        public WorldPosData(double xy) {
            X = xy;
            Y = xy;
            xF = (float) xy;
            yF = (float) xy;
        }

        public WorldPosData(double x, double y) {
            X = x;
            Y = y;

            xF = (float) x;
            yF = (float) y;
        }

        public static WorldPosData Parse(PacketReader rdr) {
            WorldPosData ret = new WorldPosData {
                xF = rdr.ReadSingle(),
                yF = rdr.ReadSingle()
            };

            ret.X = ret.xF;
            ret.Y = ret.yF;
            return ret;
        }

        public void Write(PacketWriter wtr) {
            wtr.Write((float) X);
            wtr.Write((float) Y);
        }

        public override string ToString() {
            return $"X: {X}, Y: {Y}";
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}