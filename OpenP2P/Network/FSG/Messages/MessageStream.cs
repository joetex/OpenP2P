using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /// <summary>
    /// NetworkMessageStream Packet format:
    /// |------------------------------------------------------------|
    /// |  Start Pos (4 bytes)                                       |
    /// |------------------------------------------------------------|
    /// |  Full Length (4 bytes) 1st time only                       |
    /// |------------------------------------------------------------|
    /// |  Command String (2 byte length) + (X bytes) 1st time only  |
    /// |------------------------------------------------------------|
    /// |  Segment Length (4 bytes)                                  |
    /// |------------------------------------------------------------|
    /// |  Segment Byte Data (Y bytes)                               |
    /// |------------------------------------------------------------|
    ///
    /// Continous streams
    ///     1) Trigger using command string
    ///     2) Ends when Segment Length = 0
    ///     
    /// Discreet streams
    ///     1) Use all segments
    ///     2) Ends when summation of Segment Length = Full Length
    /// 
    /// </summary>
    public class MessageStream : MessageFSG
    {
        public MessageStream() : base()
        {
            command = "data";

            Crc16();
            startPos = 0;
            segmentLen = 1;
        }
        
        public string command = "stream";
        public byte[] byteData = null;

        public uint startPos = 0;
        public uint segmentLen = 1;
        

        public byte[] GetBuffer()
        {
            return byteData;
        }
        public void SetBuffer(byte[] bytes)
        {
            byteData = bytes;
        }

        public void SetBuffer(byte[] bytes, uint start)
        {
            Array.Copy(bytes, 0, byteData, start, bytes.Length);
        }


        public override void WriteRequest(NetworkPacket packet)
        {
            packet.Write(startPos);


            //first stream segment
            if (startPos == 0)
            {
                packet.Write((uint)byteData.Length);
                packet.Write(command);
            }

            //all stream segments
            int headerSize = packet.byteLength + 4;
            segmentLen = (uint)byteData.Length - startPos;
            uint remaining = segmentLen;
            int maxLen = NetworkConfig.BufferMaxLength - headerSize;
            if (remaining > maxLen)
                segmentLen = (uint)maxLen;

            packet.Write(segmentLen);
            packet.Write(byteData, startPos, segmentLen);
            startPos += segmentLen;
        }


        public override void ReadRequest(NetworkPacket packet)
        {
            segmentLen = 0;
            startPos = packet.ReadUInt();

            //first stream segment
            if (startPos == 0)
            {
                uint totalBytes = packet.ReadUInt();
                command = packet.ReadString();
                byteData = new byte[totalBytes];
                segmentLen = packet.ReadUInt();

                byte[] bytes = packet.ReadBytes((int)segmentLen);
                SetBuffer(bytes, 0);

                return;
            }

            //subsequent stream segments
            segmentLen = packet.ReadUInt();
            byteData = packet.ReadBytes((int)segmentLen);
        }


        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write(startPos);
            packet.Write(segmentLen);
        }


        public override void ReadResponse(NetworkPacket packet)
        {
            startPos = packet.ReadUShort();
            segmentLen = packet.ReadUShort();
        }



        const ushort polynomial = 0xA001;
        static ushort[] table = null;

        public static ushort ComputeChecksum(byte[] bytes)
        {
            return ComputeChecksum(bytes, 0, bytes.Length);
        }

        public static ushort ComputeChecksum(byte[] bytes, int start, int len)
        {
            ushort crc = 0;
            for (int i = start; i < len; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        void Crc16()
        {
            if (table != null)
                return;

            table = new ushort[256];
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }

    }
}
