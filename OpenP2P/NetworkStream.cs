using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /**
     * Network Stream
     * Read/Write directly to the socket's byte buffer for sending and receiving pipeline.
     * Extensions may be made to support more types.
     */
    public partial class NetworkStream
    {
        public NetworkSocket socket = null;
        public NetworkSocketEvent socketEvent = null;
        public byte[] ByteBuffer { get { return socketEvent.GetByteBuffer(); } }
        public int byteLength = 0;
        public int bytePos = 0;

        public NetworkStream(NetworkSocket s)
        {
            socket = s;
        }

        public void Attach(NetworkSocketEvent se)
        {
            socketEvent = se;
            socket = se.socket;
        }

        public void BeginWrite()
        {
            socketEvent = socket.PrepareSend();
            byteLength = 0;
            bytePos = 0;
        }

        public void EndWrite()
        {
            socketEvent.SetBufferLength(byteLength);
            socket.ExecuteSend(socketEvent);
        }

        public void SetBufferLength(int length)
        {
            byteLength = length;
            bytePos = 0;
        }
        
        public void WriteHeader(NetworkProtocol.MessageType mt)
        {
            ByteBuffer[0] = (byte)mt;
            byteLength += 1;
        }

        public void WriteTimestamp()
        {
            long time = System.DateTime.Now.Ticks;
            //Console.WriteLine("WriteTimestamp: " + time);
            Write(BitConverter.GetBytes(time));
        }

        public void Write(byte val)
        {
            ByteBuffer[byteLength++] = val;
        }
        public void Write(byte[] val)
        {
            //ByteBuffer[byteLength++] = (byte)val.Length;
            if (BitConverter.IsLittleEndian)
            {
                //Array.Reverse(val, 0, val.Length);
            }

            for (int i = 0; i < val.Length; i++) {
                ByteBuffer[byteLength++] = val[i];
            }
            
        }
        public void Write(int val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(uint val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(long val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(ulong val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(short val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(ushort val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(float val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(double val)
        {
            Write(BitConverter.GetBytes(val));
        }
        public void Write(string val)
        {
            Write((ushort)val.Length);
            Write(Encoding.ASCII.GetBytes(val));
        }

        public NetworkProtocol.MessageType ReadHeader()
        {
            return (NetworkProtocol.MessageType)ByteBuffer[bytePos++];
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
            ushort val = BitConverter.ToUInt16(ByteBuffer, bytePos);
            bytePos += 2;
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
            for(int i= startPos; i<endPos; i++)
            {
                result[i - startPos] = ByteBuffer[bytePos++];
            }
            return result;
        }
    }
}
