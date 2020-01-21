using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkMessageStream : NetworkMessage
    {
        //public ushort partsCount = 0;
        //public string byteDataCRC = null;
        public string command = "stream";
        public byte[] byteData = null;

        public uint startPos = 0;
        public uint segmentLen = 0;
        //public string[] sentCRC = null;
        //public ushort sentPartIndex = 0;
        
        //public int recvSize = 0;
       // public SortedList<uint, string> recvCRC = new SortedList<uint, string>();
       // public ushort recvPartIndex = 0;
        //public SortedList<uint, byte[]> recvData = new SortedList<uint, byte[]>();

        //public ushort responsePartIndex = 0;
        //public string responseCRC = "";

        //public static byte streamIndex = 0;
        //public byte recvStreamIndex = 0;

        public NetworkMessageStream()
        {
            Crc16();
        }

        public byte[] GetBuffer()
        {
            return byteData;
        }
        public void SetBuffer(byte[] bytes)
        {
            byteData = bytes;
            //partsCount = (ushort)Math.Ceiling((float)byteData.Length / (float)NetworkConfig.BufferMaxLength);
        }

        public void SetBuffer(byte[] bytes, uint start)
        {
            Array.Copy(bytes, 0, byteData, start, bytes.Length);
        }

        public override void StreamMessage(NetworkPacket packet)
        {
           
        }

        public override void WriteMessage(NetworkPacket packet)
        {
            //uint packetCount = (ushort)Math.Ceiling((float)byteData.Length / (float)NetworkConfig.BufferMaxLength);

            packet.Write(startPos);


            if (startPos == 0)
            {
                
                //streamIndex++;

                //sentCRC = new string[packetCount];
                //byteDataCRC = ComputeChecksum(byteData, 0, byteData.Length).ToString("x2");
                
                //packet.Write(byteDataCRC);
                packet.Write((uint)byteData.Length);
                packet.Write(command);
            }

            int headerSize = packet.byteLength + 4;
            segmentLen = (uint)byteData.Length - startPos;
            uint remaining = segmentLen;
            int maxLen = NetworkConfig.BufferMaxLength - headerSize;
            if (remaining > maxLen)
                segmentLen = (uint)maxLen;

            
            packet.Write(segmentLen);
            packet.Write(byteData, startPos, segmentLen);
            
            //sentCRC[sentPartIndex++] = ComputeChecksum(byteData, sentBytePos, len).ToString("x2");
            startPos += segmentLen;
        }

        public override void ReadMessage(NetworkPacket packet)
        {
            //uint dataLen = packet.ReadUInt();
            segmentLen = 0;
            startPos = packet.ReadUInt();

            if (startPos == 0 )
            {
                //recvData = new byte[dataLen];
                //byteDataCRC = packet.ReadString();
                uint totalBytes = packet.ReadUInt();
                command = packet.ReadString();
                byteData = new byte[totalBytes];
                //int maxPartCount = (int)Math.Ceiling((float)byteData.Length / (float)NetworkConfig.BufferMaxLength);
                //recvData = new SortedList<uint,byte[]>(maxPartCount);
                //recvCRC = new SortedList<uint, string>(byteData.Length);
                segmentLen = packet.ReadUShort();
                
                byte[] bytes = packet.ReadBytes((int)segmentLen);
                SetBuffer(bytes, 0);

                return;
            }


            segmentLen = packet.ReadUInt();
            byteData = packet.ReadBytes((int)segmentLen);

            //recvPartIndex = header.sequence


            //recvData.Add(recvPartIndex, bytes);
            //recvCRC.Add(recvPartIndex, ComputeChecksum(bytes).ToString("x2"));

            //Array.Copy(bytes, 0, byteData, recvSize, bytes.Length);

            //recvSize += bytes.Length;
        }


        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write(startPos);
            packet.Write(segmentLen);
            //responseCRC = recvCRC[recvCRC.Count - 1].Substring(0, 6);

            //packet.Write(responsePartIndex);
            //packet.Write(responseCRC);

            //packet.Write(responseTimestamp);
        }
        public override void ReadResponse(NetworkPacket packet)
        {
            startPos = packet.ReadUShort();
            segmentLen = packet.ReadUShort();
            //responsePartIndex = packet.ReadUShort();
            //responseCRC = packet.ReadString();
            //responseTimestamp = packet.ReadLong();
        }

        public void Complete()
        {
            /*int currentSize = 0;
            for (int i = 0; i < recvData.Count; i++)
            {
                byte[] bytes = recvData[i];
                if (bytes == null)
                    continue;
                Array.Copy(bytes, 0, byteData, currentSize, bytes.Length);
                currentSize += bytes.Length;
            }*/
        }

        public bool VerifyPart()
        {
            /*if (responseCRC.Length == 0)
                return false;

            if (responsePartIndex >= sentCRC.Length)
                return false;

            if (sentCRC[responsePartIndex] != responseCRC)
                return false;
                */
            return true;
        }

        public bool VerifyData()
        {
            /*if (recvSize != byteData.Length)
                return false;

            string receivedCRC = ComputeChecksum(byteData, 0, byteData.Length).ToString("x2");
            if (byteDataCRC != receivedCRC)
                return false;
                */
            return true;
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
