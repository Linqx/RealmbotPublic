using System;

namespace Stream {
    public class Buffer {
        public int RemainingLength => MaxSize - Size;

        public byte[] Data;
        public int MaxSize;
        public int Size;

        public Buffer(int maxSize) {
            MaxSize = maxSize;
            Data = new byte[maxSize];
            Size = 0;
        }

        public void AddData(byte[] data, int offset, int length) {
            Array.Copy(data, offset, Data, Size, length);
            Size += length;
        }

        public void AddData(Array data, int offset, int length) {
            System.Buffer.BlockCopy(data, offset, Data, Size, length);
            Size += length;
        }

        public void AddByte(byte b) {
            Data[Size] = b;
            Size++;
        }

        public void Reset(int size) {
            Data = new byte[size];
            MaxSize = size;
            Size = 0;
        }
    }
}