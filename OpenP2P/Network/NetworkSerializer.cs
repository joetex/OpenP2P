using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkSerializer
    {
        public byte[] buffer;
        public byte[] ByteBuffer { get { return buffer; } }
        public int byteLength = 0; //total size of data 
        public int bytePos = 0; //current read position
        public int byteSent = 0;

        public Dictionary<Type, Action<object>> writeDictionary = new Dictionary<Type, Action<object>>();
        public Dictionary<Type, Func<object>> readDictionary = new Dictionary<Type, Func<object>>();

        public NetworkSerializer(int initBufferSize)
        {
            buffer = new byte[initBufferSize];

            writeDictionary[typeof(int)] = (object val) => { Write((int)val); };
            writeDictionary[typeof(uint)] = (object val) => { Write((uint)val); };
            writeDictionary[typeof(long)] = (object val) => { Write((long)val); };
            writeDictionary[typeof(ulong)] = (object val) => { Write((ulong)val); };
            writeDictionary[typeof(byte)] = (object val) => { Write((byte)val); };
            writeDictionary[typeof(byte[])] = (object val) => { Write((byte[])val); };
            writeDictionary[typeof(string)] = (object val) => { Write((string)val); };
            writeDictionary[typeof(short)] = (object val) => { Write((short)val); };
            writeDictionary[typeof(ushort)] = (object val) => { Write((ushort)val); };
            writeDictionary[typeof(float)] = (object val) => { Write((float)val); };
            writeDictionary[typeof(double)] = (object val) => { Write((double)val); };


            readDictionary[typeof(int)] = () => { return ReadInt(); };
            readDictionary[typeof(uint)] = () => { return ReadUInt(); };
            readDictionary[typeof(long)] = () => { return ReadLong(); };
            readDictionary[typeof(ulong)] = () => { return ReadULong(); };
            readDictionary[typeof(byte)] = () => { return ReadByte(); };
            readDictionary[typeof(byte[])] = () => { return ReadBytes(); };
            readDictionary[typeof(string)] = () => { return ReadString(); };
            readDictionary[typeof(short)] = () => { return ReadShort(); };
            readDictionary[typeof(ushort)] = () => { return ReadUShort(); };
            readDictionary[typeof(float)] = () => { return ReadFloat(); };
            readDictionary[typeof(double)] = () => { return ReadDouble(); };
        }

        public void SetBufferLength(int length)
        {
            byteLength = length;
            bytePos = 0;
        }

        public void SerializeStruct<T>(T item)
        {
            FieldInfo[] fields = item.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(item);
                writeDictionary[field.GetType()](value);
            }
        }

        public void DeserializeStruct<T>(T item)
        {
            FieldInfo[] fields = item.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(item);
                field.SetValue(item, readDictionary[field.GetType()]());
            }
        }

        public byte[] ToArray()
        {
            byte[] arr = new byte[byteLength];
            Array.Copy(ByteBuffer, 0, arr, 0, byteLength);
            return arr;
        }

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

        public unsafe void Overwrite(byte[] val, int start)
        {
            Array.Copy(val, 0, ByteBuffer, start, val.Length); 
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
            //Console.WriteLine("Write Float bytes: " + ByteBuffer[byteLength + 0] + " " + ByteBuffer[byteLength + 1] + " " + ByteBuffer[byteLength + 2] + " " + ByteBuffer[byteLength + 3] + " "  );
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
        public unsafe void WriteASCII(string val)
        {
            Write((ushort)val.Length);
            Write(Encoding.ASCII.GetBytes(val));
        }
        public unsafe void Write(string val)
        {
            byte[] utfBytes = Encoding.UTF8.GetBytes(val);
            Write((ushort)utfBytes.Length);
            Write(utfBytes);

        }

        public unsafe void Write(string val, int len)
        {
            byte[] utfBytes = Encoding.UTF8.GetBytes(val);
            Write((ushort)len);
            Write(utfBytes);

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
            //Console.WriteLine("Read Float bytes: " + ByteBuffer[bytePos + 0] + " " + ByteBuffer[bytePos + 1] + " " + ByteBuffer[bytePos + 2] + " " + ByteBuffer[bytePos + 3]  );


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

        public string ReadStringASCII()
        {
            int cnt = ReadUShort();
            string result = Encoding.ASCII.GetString(ByteBuffer, bytePos, cnt);
            bytePos += cnt;
            return result;
        }

        public string ReadString()
        {
            int cnt = ReadUShort();
            string result = Encoding.UTF8.GetString(ByteBuffer, bytePos, cnt);
            bytePos += cnt;
            return result;
        }
        public string ReadString(int cnt)
        {
            string result = Encoding.UTF8.GetString(ByteBuffer, bytePos, cnt);
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


        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }
    }
}
