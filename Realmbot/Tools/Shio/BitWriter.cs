﻿using System;
using System.Text;
using Bookkeeping;

namespace Stream {
    public class BitWriter {
        /// <summary>
        /// The default buffer length of BitWriter
        /// </summary>
        private const int Default_Length = 1024;

        /// <summary>
        /// Buffer of already written bits
        /// </summary>
        private readonly uint[] buffer = new uint[Default_Length];

        /// <summary>
        /// Scratch bits used to hold currently written bits
        /// </summary>
        private ulong scratch;

        /// <summary>
        /// The amount of bits still in the scratch value
        /// </summary>
        private int scratchBits;

        /// <summary>
        /// The amount of bits written into the buffer
        /// </summary>
        private uint bitsWritten;

        /// <summary>
        /// Expands the buffer by the default length
        /// </summary>
        private void ExpandBuffer() {
            uint[] newBuffer = new uint[buffer.Length + Default_Length];
            System.Buffer.BlockCopy(buffer, 0, newBuffer, 0, sizeof(uint) * buffer.Length);
        }

        /// <summary>
        /// Adds bits to the bit buffer
        /// </summary>
        /// <param name="bits"></param>
        private void AddToBuffer(uint bits) {
            uint index = bitsWritten / 32;
            if (index >= buffer.Length) ExpandBuffer();

            buffer[index] = bits;
        }

        /// <summary>
        /// Flushes the last bits from the scratch. Only flushes a maximum of 32 bits
        /// </summary>
        /// <returns></returns>
        private int FlushScratch() {
            int amount = Math.Min(scratchBits, 32);
            uint bits = (uint) scratch;

            scratch >>= amount;
            scratchBits -= amount;

            AddToBuffer(bits);
            return amount;
        }

        /// <summary>
        /// Returns a buffer containing all writen data.
        /// </summary>
        /// <returns></returns>
        public Buffer GetData() {
            uint bitCount = bitsWritten + (uint) scratchBits;
            Buffer getData = new Buffer((int) ((bitCount + 7) / 8));
            if (bitsWritten > 0) getData.AddData(buffer, 0, (int) (bitsWritten / 8));
            if (scratchBits > 0) {
                int scratchBytes = (scratchBits + 7) / 8;
                if (scratchBytes < 4 || !BitConverter.IsLittleEndian)
                    for (int i = scratchBytes - 1; i >= 0; i--) {
                        byte b = (byte) (scratch >> (i * 8));
                        getData.AddByte(b);
                    }
                else
                    for (int i = 0; i < scratchBytes; i++) {
                        byte b = (byte) (scratch >> (i * 8));
                        getData.AddByte(b);
                    }
            }

            return getData;
        }

        /// <summary>
        /// Writes bits to the buffer
        /// </summary>
        /// <param name="bits">The uint32 containing the bits</param>
        /// <param name="length">The amount of bits to write</param>
        public void Write(uint bits, byte length) {
            int cleanShift = 32 - length;
            ulong cleanBits = (bits << cleanShift) >> cleanShift;

            scratch |= cleanBits << scratchBits;
            scratchBits += length;

            if (scratchBits < 32) return;
            bitsWritten += (uint) FlushScratch();
        }

        public void Write(bool value) {
            Write(value ? (uint) 1 : 0, 1);
        }

        public void Write(byte value) {
            Write(value, 8);
        }

        public void Write(ushort value) {
            Write(value, 16);
        }

        public void Write(uint value) {
            Write(value, 32);
        }

        public void Write(ulong value) {
            Write((uint) value, 32);
            Write((uint) (value >> 32), 32);
        }

        public void Write(sbyte value) {
            Write((uint) value, 8);
        }

        public void Write(short value) {
            Write((uint) value, 16);
        }

        public void Write(int value) {
            Write((uint) value, 32);
        }

        public void Write(long value) {
            Write((uint) value, 32);
            Write((uint) (value >> 32), 32);
        }

        public void Write(float value) {
            uint intVal = 0;
            unsafe {
                intVal = *(uint*) &value;
            }

            Write(intVal, 32);
        }

        public void Write(double value) {
            ulong intVal = 0;
            unsafe {
                intVal = *(ulong*) &value;
            }

            Write(intVal);
        }

        public void Write(DateTime value) {
            Write(value.ToUniversalTime().Ticks);
        }

        public void Write(string str) {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            Write((ushort) bytes.Length);
            Write(bytes);
        }

        public void Write(byte[] bytes) {
            for (int i = 0; i < bytes.Length; i++)
                Write(bytes[i]);
        }

        public void LogData() {
            LogEntry entry = LogEntry.Init("Write Buffer: ");
            for (int i = 0; i < bitsWritten / 32; i++) {
                entry.Append(Convert.ToString(buffer[i], 2).PadLeft(32, '0'), ConsoleColor.Red);
                entry.Append("-", ConsoleColor.Red);
            }

            if (scratchBits > 0)
                entry.Append(Convert.ToString((uint) scratch, 2).PadLeft(scratchBits, '0'), ConsoleColor.Red);
            Log.Write(entry);
        }
    }
}