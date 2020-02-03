using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public partial class NetworkPacket
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
            //if (BitConverter.IsLittleEndian)
            //{
                //Array.Reverse(val, 0, val.Length);
            //}
            Array.Copy(val, 0, ByteBuffer, byteLength, val.Length);
            byteLength += val.Length;
        }
        public unsafe void Write(byte[] val, int start, int length)
        {
            Array.Copy(val, start, ByteBuffer, byteLength, length);
            byteLength += length;
        }

        public unsafe void Write(byte[] val, uint start, uint length)
        {
            Array.Copy(val, start, ByteBuffer, byteLength, length);
            byteLength += (int)length;
        }

        public unsafe void Write(int val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((int*)b) = val;

            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 4);
            byteLength += 4;
        }
        public unsafe void Write(uint val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((uint*)b) = val;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 4);
            byteLength += 4;
        }
        public unsafe void Write(long val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((long*)b) = val;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 8);
            byteLength += 8;
        }
        public unsafe void Write(ulong val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((ulong*)b) = val;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 8);
            byteLength += 8;
        }
        public unsafe void Write(short val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((short*)b) = val;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 2);
            byteLength += 2;
        }
        public unsafe void Write(ushort val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((ushort*)b) = val;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 2);
            byteLength += 2;
        }
        public unsafe void Write(float val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((float*)b) = val;
            Console.WriteLine("Write Float bytes: " + ByteBuffer[byteLength + 0] + " " + ByteBuffer[byteLength + 1] + " " + ByteBuffer[byteLength + 2] + " " + ByteBuffer[byteLength + 3] + " "  );
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 4);
            byteLength += 4;
        }
        public unsafe void Write(double val)
        {
            fixed (byte* b = &ByteBuffer[byteLength])
                *((double*)b) = val;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 8);
            byteLength += 8;
        }
        public unsafe void Write(string val)
        {
            Write((ushort)val.Length);
            Write(Encoding.ASCII.GetBytes(val));
        }


        public long ReadTimestamp()
        {
            long time = ReadLong();// BitConverter.ToInt64(ByteBuffer, bytePos);
            
            return time;
        }

        public int ReadInt()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 4);
            int val = BitConverter.ToInt32(ByteBuffer, bytePos);
            
            bytePos += 4;
            return val;
        }
        public uint ReadUInt()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 4);
            uint val = BitConverter.ToUInt32(ByteBuffer, bytePos);
            bytePos += 4;
            return val;
        }
        public long ReadLong()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 8);
            long val = BitConverter.ToInt64(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }
        public ulong ReadULong()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 8);
            ulong val = BitConverter.ToUInt64(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }
        public short ReadShort()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 2);
            short val = BitConverter.ToInt16(ByteBuffer, bytePos);
            bytePos += 2;

            return val;
        }
        public ushort ReadUShort()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 2);
            //NetworkConfig.ProfileBegin("ReadUShort");
            ushort val = BitConverter.ToUInt16(ByteBuffer, bytePos);
            bytePos += 2;
            //NetworkConfig.ProfileEnd("ReadUShort");
            return val;
        }

        public float ReadFloat()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 4);
            Console.WriteLine("Read Float bytes: " + ByteBuffer[bytePos + 0] + " " + ByteBuffer[bytePos + 1] + " " + ByteBuffer[bytePos + 2] + " " + ByteBuffer[bytePos + 3]  );


            float val = BitConverter.ToSingle(ByteBuffer, bytePos);
            bytePos += 4;
            return val;
        }
        public double ReadDouble()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 8);
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

        /*public byte[] ReadBytes(int len)
        {
            byte[] result = new byte[len];
            uint startPos = bytePos;
            uint endPos = bytePos + len;
            for (int i = startPos; i < endPos; i++)
            {
                result[i - startPos] = ByteBuffer[bytePos++];
            }
            return result;
        }*/
        public byte[] ReadBytes(int len)
        {
            byte[] result = new byte[len];
            int startPos = bytePos;
            int endPos = bytePos + len;
            for (int i = startPos; i < endPos; i++)
            {
                result[i - startPos] = ByteBuffer[bytePos++];
            }
            return result;
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
