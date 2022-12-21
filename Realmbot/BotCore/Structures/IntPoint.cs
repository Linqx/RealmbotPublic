using System;
using System.Collections.Generic;
using BotCore.Structures;

namespace RotMGCore.Structures.Game {
    public struct IntPointComparer : IEqualityComparer<IntPoint> {
        public bool Equals(IntPoint x, IntPoint y) {
            return x.X == y.X && x.Y == y.Y;
        }

        public int GetHashCode(IntPoint obj) {
            return (obj.X * 23) << (16 + obj.Y * 17);
        }
    }

    public struct IntPoint {
        public int X;
        public int Y;

        public IntPoint(int x, int y) {
            X = x;
            Y = y;
        }

        public IntPoint(int xy) {
            X = xy;
            Y = xy;
        }

        public double Length => Math.Sqrt(X * X + Y * Y);
        public int LengthSqr => X * X + Y * Y;

        public Vector2 ToVector() {
            return new Vector2(X, Y);
        }

        public static IntPoint Zero = new IntPoint();
        public static IntPoint One = new IntPoint(1);

        public static IntPoint Add(IntPoint a, IntPoint b) {
            return new IntPoint(a.X + b.X, a.Y + b.Y);
        }

        public static IntPoint Add(IntPoint a, int value) {
            return new IntPoint(a.X + value, a.Y + value);
        }

        public static IntPoint Subtract(IntPoint a, IntPoint b) {
            return new IntPoint(a.X - b.X, a.Y - b.Y);
        }

        public static IntPoint Subtract(IntPoint a, int value) {
            return new IntPoint(a.X - value, a.Y - value);
        }

        public static IntPoint Multiply(IntPoint a, IntPoint b) {
            return new IntPoint(a.X * b.X, a.Y * b.Y);
        }

        public static IntPoint Multiply(IntPoint a, int value) {
            return new IntPoint(a.X * value, a.Y * value);
        }

        public static IntPoint Divide(IntPoint a, IntPoint b) {
            return new IntPoint(a.X / b.X, a.Y / b.Y);
        }

        public static IntPoint Divide(IntPoint a, int value) {
            return new IntPoint(a.X / value, a.Y / value);
        }

        public static bool operator ==(IntPoint a, IntPoint b) {
            return !(a != b);
        }

        public static bool operator !=(IntPoint a, IntPoint b) {
            return a.X != b.X || a.Y != b.Y;
        }

        public static IntPoint operator +(IntPoint a, IntPoint b) {
            return Add(a, b);
        }

        public static IntPoint operator +(IntPoint a, int b) {
            return Add(a, b);
        }

        public static IntPoint operator -(IntPoint a, IntPoint b) {
            return Subtract(a, b);
        }

        public static IntPoint operator -(IntPoint a, int b) {
            return Subtract(a, b);
        }

        public static IntPoint operator *(IntPoint a, IntPoint b) {
            return Multiply(a, b);
        }

        public static IntPoint operator *(IntPoint a, int b) {
            return Multiply(a, b);
        }

        public static IntPoint operator /(IntPoint a, IntPoint b) {
            return Divide(a, b);
        }

        public static IntPoint operator /(IntPoint a, int b) {
            return Divide(a, b);
        }

        public static readonly IntPoint[] SurroundingPoints = {
            new IntPoint(1, 0),
            new IntPoint(1, 1),
            new IntPoint(0, 1),
            new IntPoint(-1, 1),
            new IntPoint(-1, 0),
            new IntPoint(-1, -1),
            new IntPoint(0, -1),
            new IntPoint(1, -1)
        };

        public override bool Equals(object obj) {
            if (obj is IntPoint point) return point.X == X && point.Y == Y;
            return false;
        }

        public override int GetHashCode() {
            return (X * 23) << (16 + Y * 17);
        }

        public override string ToString() {
            return $"X: {X} Y: {Y}";
        }
    }
}