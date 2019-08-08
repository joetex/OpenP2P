using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public partial class NetworkStream
    {
        public unsafe void WriteTimestamp()
        {
            long time = System.DateTime.Now.Ticks;
            Write(time);
        }

        public unsafe void Write(byte val)
        {
            ByteBuffer[byteLength++] = val;
        }
        public unsafe void Write(byte[] val)
        {
            if (BitConverter.IsLittleEndian)
            {
                //Array.Reverse(val, 0, val.Length);
            }
            Array.Copy(val, 0, ByteBuffer, byteLength, val.Length);
            byteLength += val.Length;
        }

        public unsafe void Write(int val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((int*)b) = val;
            byteLength += 4;
        }
        public unsafe void Write(uint val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((uint*)b) = val;
            byteLength += 4;
        }
        public unsafe void Write(long val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((long*)b) = val;
            byteLength += 8;
        }
        public unsafe void Write(ulong val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((ulong*)b) = val;
            byteLength += 8;
        }
        public unsafe void Write(short val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((short*)b) = val;
            byteLength += 2;
        }
        public unsafe void Write(ushort val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((ushort*)b) = val;
            byteLength += 2;
        }
        public unsafe void Write(float val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((float*)b) = val;
            byteLength += 4;
        }
        public unsafe void Write(double val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((double*)b) = val;
            byteLength += 8;
        }
        public unsafe void Write(string val)
        {
            Write((ushort)val.Length);
            Write(Encoding.ASCII.GetBytes(val));
        }


        public long ReadTimestamp()
        {
            long time = BitConverter.ToInt64(ByteBuffer, bytePos);
            bytePos += 8;
            return time;
        }

        public int ReadInt()
        {
            int val = BitConverter.ToInt32(ByteBuffer, bytePos);
            bytePos += 4;
            return val;
        }
        public uint ReadUInt()
        {
            uint val = BitConverter.ToUInt32(ByteBuffer, bytePos);
            bytePos += 4;
            return val;
        }
        public long ReadLong()
        {
            long val = BitConverter.ToInt64(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }
        public ulong ReadULong()
        {
            ulong val = BitConverter.ToUInt64(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }
        public short ReadShort()
        {

            short val = BitConverter.ToInt16(ByteBuffer, bytePos);
            bytePos += 2;

            return val;
        }
        public ushort ReadUShort()
        {
            //NetworkConfig.ProfileBegin("ReadUShort");
            ushort val = BitConverter.ToUInt16(ByteBuffer, bytePos);
            bytePos += 2;
            //NetworkConfig.ProfileEnd("ReadUShort");
            return val;
        }

        public float ReadFloat()
        {
            float val = BitConverter.ToSingle(ByteBuffer, bytePos);
            bytePos += 4;
            return val;
        }
        public double ReadDouble()
        {
            double val = BitConverter.ToDouble(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }

        public string ReadString()
        {
            int cnt = ReadUShort();
            string result = Encoding.ASCII.GetString(ByteBuffer, bytePos, cnt);
            bytePos += cnt;
            return result;
        }

        public byte ReadByte()
        {
            return ByteBuffer[bytePos++];
        }

        public byte[] ReadBytes()
        {
            byte cnt = ByteBuffer[bytePos++];

            byte[] result = new byte[cnt];
            int startPos = bytePos;
            int endPos = bytePos + cnt;
            for (int i = startPos; i < endPos; i++)
            {
                result[i - startPos] = ByteBuffer[bytePos++];
            }
            return result;
        }
    }
}
