using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class MsgDataContent : NetworkMessage
    {
        public byte[] sendData = null;
        public int sentSize = 0;
        public int recvSize = 0;
        public string dataCRC = null;
        public string[] sentCRC = null;
        public string recvCRC = null;
        public ushort sentPartIndex = 0;
        public ushort recvPartIndex = 0;
        public byte[] recvData = null;
        public ushort packetCount = 0;

        public MsgDataContent()
        {
            Crc16();
        }

        public void SetBuffer(byte[] bytes)
        {
            sendData = bytes;
            packetCount = (ushort)Math.Ceiling((float)sendData.Length / (float)NetworkConfig.BufferMaxLength);
        }

        public override void StreamMessage(NetworkPacket packet)
        {
           
        }

        public override void WriteMessage(NetworkPacket packet)
        {
            uint packetCount = (ushort)Math.Ceiling((float)sendData.Length / (float)NetworkConfig.BufferMaxLength);
            int len = sendData.Length;
            int remaining = len - sentSize;
            if (remaining > NetworkConfig.BufferMaxLength)
                len = NetworkConfig.BufferMaxLength;

            if (sentCRC == null)
            {
                sentCRC = new string[packetCount];
                dataCRC = ComputeChecksum(sendData, 0, sendData.Length).ToString("x2");
                packet.Write(dataCRC);
            }
            else
            {
                packet.Write(dataCRC.Substring(0, 4));
            }

            packet.Write(sendData.Length);
            packet.Write(sentPartIndex);
            packet.Write(len);
            packet.Write(sendData, sentSize, len);
            
            sentCRC[sentPartIndex++] = ComputeChecksum(sendData, sentSize, len).ToString("x2");
            sentSize += len;
        }

        public override void ReadMessage(NetworkPacket packet)
        {
            uint dataLen = packet.ReadUInt();

            if ( recvData == null )
            {
                recvData = new byte[dataLen];
            }
           
            recvPartIndex = packet.ReadUShort();
            int len = packet.ReadUShort();
            byte[] receivedBytes = packet.ReadBytes(len);
            recvSize += receivedBytes.Length;
            recvCRC = ComputeChecksum(receivedBytes).ToString("x2");
            Array.Copy(receivedBytes, 0, recvData, recvPartIndex * NetworkConfig.BufferMaxLength, len);

        }


        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write(recvCRC);
            //packet.Write(responseTimestamp);

        }
        public override void ReadResponse(NetworkPacket packet)
        {
            recvCRC = packet.ReadString();
            //responseTimestamp = packet.ReadLong();
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
