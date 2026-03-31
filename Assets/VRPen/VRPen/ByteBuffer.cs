using System;
using System.Text;
using UnityEngine;

namespace VRPen.VRPenPro
{
    public class ByteBuffer
    {
        public  byte[] Array = new byte[32];
        private int    count;

        private int posRead;
        public  int posWrite;

        public void Clear()
        {
            count    = 0;
            posRead  = 0;
            posWrite = 0;
        }

        public void WriteFloat(float value)
        {
            int bites = BitConverter.SingleToInt32Bits(value);
            WriteInt(bites);
        }

        public void WriteColor(Color32 color)
        {
            WriteByte(color.r);
            WriteByte(color.g);
            WriteByte(color.b);
            WriteByte(color.a);
        }
        
        public void WriteVector3(Vector3 point)
        {
            WriteFloat(point.x);
            WriteFloat(point.y);
            WriteFloat(point.z);
        }
        
        public void WriteQuaternion(Quaternion point)
        {
            WriteFloat(point.x);
            WriteFloat(point.y);
            WriteFloat(point.z);
            WriteFloat(point.w);
        }
        
        public void WriteInt(int value) => WriteUInt(unchecked((uint)value));

        public void WriteUInt(uint value)
        {
            unchecked
            {
                int index = posWrite;
                Advance(4);

                // Write to highest index first so the JIT skips bounds checks on subsequent writes.
                Array[index + 3] = (byte)value;
                Array[index + 2] = (byte)(value >> 8);
                Array[index + 1] = (byte)(value >> 16);
                Array[index + 0] = (byte)(value >> 24);
            }
        }

        public void WriteByte(byte value)
        {
            Advance(1);
            Array[count - 1] = value;
        }
        
        public void WriteBool(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteInt(0);
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(bytes.Length + 1);
            WriteBytes(bytes, 0, bytes.Length);
        }

        public void WriteBytes(byte[] src, int srcIndex, int count)
        {
            int position = posWrite;
            Advance(count);
            System.Array.Copy(
                src, srcIndex,
                Array, position,
                count);
        }
        
        public void WriteBytesWithCount(byte[] src)
        {
            WriteInt(src.Length);
            WriteBytes(src, 0, src.Length);
        }

        //=============

        private void Advance(int count)
        {
            this.count += count;
            posWrite   += count;

            if (Array.Length < this.count)
            {
                var newArray = new byte[this.count * 2];
                System.Array.Copy(Array, 0, newArray, 0, Array.Length);
                Array = newArray;
            }
        }

        //==========

        public float ReadFloat()
        {
            var raw   = ReadInt();
            var value = BitConverter.Int32BitsToSingle(raw);
            return value;
        }
        
        public Vector3 ReadVector3()
        {
            return new Vector3(
                ReadFloat(),
                ReadFloat(),
                ReadFloat()
            );
        }
        
        public Quaternion ReadQuaternion()
        {
            return new Quaternion(
                ReadFloat(),
                ReadFloat(),
                ReadFloat(),
                ReadFloat()
            );
        }

        public int ReadInt()
        {
            var raw   = ReadUInt();
            var value = unchecked((int)raw);
            return value;
        }

        public uint ReadUInt()
        {
            int result = 0;
            result |= Array[posRead + 3];
            result |= Array[posRead + 2] << 8;
            result |= Array[posRead + 1] << 16;
            result |= Array[posRead + 0] << 24;

            posRead += 4;

            var value = (uint)result;

            return value;
        }

        public byte ReadByte()
        {
            var value = Array[posRead];
            posRead++;
            return value;
        }
        
        public bool ReadBool()
        {
            return ReadByte() != 0;
        }

        public Color32 ReadColor()
        {
            return new Color32(
                ReadByte(),
                ReadByte(),
                ReadByte(),
                ReadByte()
            );
        }

        public string ReadString()
        {
            var lenght = ReadInt();
            if (lenght == 0) return null;
            lenght -= 1;
            var str = Encoding.UTF8.GetString(Array, posRead, lenght);
            posRead += lenght;
            return str;
        }

        public void ReadBytes(byte[] dst, int dstIndex, int count)
        {
            System.Array.Copy(
                Array,
                posRead,
                dst,
                dstIndex,
                count);

            posRead += count;
        }
        
        public byte[] ReadBytesWithCount()
        {
            int count = ReadInt();
            var bytes = new byte[count];
            ReadBytes(bytes, 0, count);
            return bytes;
        }

        //================

        public void CopyFrom(byte[] array)
        {
            Clear();
            if (Array.Length < array.Length)
            {
                var newArray = new byte[array.Length];
                Array = newArray;
            }

            System.Array.Copy(array, 0, Array, 0, array.Length);
            count = array.Length;
        }
    }
}