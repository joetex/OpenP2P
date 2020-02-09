using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class MessageSTUN : NetworkMessage
    {
        public static Random random = new Random();
        public STUNMethod method = STUNMethod.None;
        public ushort methodLength = 0;
        public ushort methodId = 0;
        public byte[] transactionID = new byte[16];
        public bool integrity = false;

        public NetworkSerializer serializer = new NetworkSerializer(NetworkConfig.BufferMaxLength);
        public List<byte[]> attributeBytes = new List<byte[]>();
        public Dictionary<STUNAttribute, object> response = new Dictionary<STUNAttribute, object>();

        public string username = "joe";
        public string password = "test";
        public string realm = "test";

        public object Get(STUNAttribute attr)
        {
            if (!response.ContainsKey(attr))
                return null;
            object value = response[attr];
            return value;
        }

        public string GetString(STUNAttribute attr)
        {
            object obj = Get(attr);
            if (obj == null)
                return "";
            return obj.ToString();
        }

        public override void WriteMessage(NetworkPacket packet)
        {
            
            //method id
            packet.Write((ushort)method);

            //method length
            int lengthPos = packet.byteLength;
            packet.Write((ushort)0);

            //transaction id 
            header.ackkey = BitConverter.ToUInt32(transactionID, 0);
            packet.Write(transactionID);

            //attributes
            int totalAttrLength = 0;
            for(int i=0; i<attributeBytes.Count; i++)
            {
                byte[] attr = attributeBytes[i];
                totalAttrLength += attr.Length;
                packet.Write(attr);
            }
            
            //method integrity goes here
            if( integrity )
            {
                GenerateMessageIntegrity(packet);
            }

            //update method length
            int lastPos = packet.byteLength;
            packet.byteLength = lengthPos;
            packet.Write((ushort)totalAttrLength);
            packet.byteLength = lastPos;

            //Console.WriteLine("Message Length = " + totalAttrLength);
            //Console.WriteLine("Total Bytes: " + packet.byteLength);

            //cleanup
            attributeBytes.Clear();
        }
     

        public override void ReadResponse(NetworkPacket packet)
        {
            methodId = (ushort)((uint)packet.ReadUShort() & 0x3FFF); //0x3F (0011 1111) sets left most 2 bits to 00
            methodLength = packet.ReadUShort();
            transactionID = packet.ReadBytes(16);
            header.ackkey = BitConverter.ToUInt32(transactionID, 0);

            method = (STUNMethod)methodId;

            //Console.WriteLine("STUN Response address: " + packet.remoteEndPoint.ToString());
            //Console.WriteLine("STUN Response Method: " + Enum.GetName(typeof(STUNMethod), method));
            //Console.WriteLine("STUN Response Length: " + methodLength);
            
            while (packet.bytePos < packet.byteLength)
            {
                STUNAddress address;
                STUNAttribute attrType = (STUNAttribute)packet.ReadUShort();
                //Console.WriteLine("STUN Attr Type: " + Enum.GetName(typeof(STUNAttribute), attrType));
                switch (attrType)
                {
                    case STUNAttribute.MappedAddress:
                        address = ReadMappedAddress(packet);
                        response.Add(attrType, address);
                        //Console.WriteLine("Mapped Address: " + address.address[0] + "." + address.address[1] + "." + address.address[2] + "." + address.address[3] + ":" + address.port);
                        break;
                    case STUNAttribute.XorMappedAddress:
                        address = ReadXorMappedAddress(packet);
                        response.Add(attrType, address);
                        //Console.WriteLine("Xor Mapped Address: " + address.address[0] + "." + address.address[1] + "." + address.address[2] + "." + address.address[3] + ":" + address.port);
                        break;
                    case STUNAttribute.SourceAddress:
                        address = ReadMappedAddress(packet);
                        response.Add(attrType, address);
                        //Console.WriteLine("Source Address: " + address.address[0] + "." + address.address[1] + "." + address.address[2] + "." + address.address[3] + ":" + address.port);
                        break;
                    case STUNAttribute.ChangedAddress:
                        address = ReadMappedAddress(packet);
                        response.Add(attrType, address);
                        //Console.WriteLine("Changed Address: " + address.address[0] + "." + address.address[1] + "." + address.address[2] + "." + address.address[3] + ":" + address.port);
                        break;
                    case STUNAttribute.ErrorCode:
                        response.Add(attrType, ReadErrorCode(packet));
                        //Console.WriteLine("Changed Address: " + address.address[0] + "." + address.address[1] + "." + address.address[2] + "." + address.address[3] + ":" + address.port);
                        break;
                    case STUNAttribute.UnknownAttribute:
                        response.Add(attrType, ReadUnknownAttributes(packet));
                        //Console.WriteLine("Changed Address: " + address.address[0] + "." + address.address[1] + "." + address.address[2] + "." + address.address[3] + ":" + address.port);
                        break;
                    case STUNAttribute.ServerName:
                        response.Add(attrType, ReadString(packet));
                        //Console.WriteLine("Changed Address: " + address.address[0] + "." + address.address[1] + "." + address.address[2] + "." + address.address[3] + ":" + address.port);
                        break;
                    default:
                        ushort attrLen = packet.ReadUShort();
                        byte[] bytes = packet.ReadBytes(attrLen);
                        response.Add(attrType, bytes);
                        break;
                }
            }

        }

        


        public void WriteChangeRequest(bool changeIP, bool changePort) {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)STUNAttribute.ChangeRequest);
            serializer.Write((ushort)4);

            int flags = (!changeIP ? 0 : (1 << 2)) | (!changePort ? 0 : (1 << 1));
            serializer.Write(flags);

            attributeBytes.Add(serializer.ToArray());
        }


        public void WriteString(STUNAttribute attr, string text)
        {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)attr);
            int startLen = serializer.byteLength;
            serializer.Write(text);
            int textLen = serializer.byteLength - startLen - 2;
            //pad to multiple of 4
            int padCount = 0;
            while (((textLen++) % 4) != 0) padCount++;

            for(int i=0; i<padCount; i++) {
                serializer.Write((byte)0);
            }

            attributeBytes.Add(serializer.ToArray());
        }

        public void WriteUInt(STUNAttribute attr, uint value)
        {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)attr);
            serializer.Write((ushort)4);
            serializer.Write(value);

            attributeBytes.Add(serializer.ToArray());
        }

        public void WriteEmpty(STUNAttribute attr)
        {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)attr);
            serializer.Write((ushort)0);

            attributeBytes.Add(serializer.ToArray());
        }

       
        public void WriteMessageIntegrity()
        {
            integrity = true;
        }

        public void GenerateMessageIntegrity(NetworkPacket packet)
        {
            string keyStr = username + ":" + realm + ":" + password;
            byte[] key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(keyStr));
            byte[] msgBytes = new byte[packet.byteLength];
            Array.Copy(packet.ByteBuffer, 0, msgBytes, 0, packet.byteLength);
            string hmac = Encode(key, msgBytes);
            byte[] hmacBytes = Encoding.UTF8.GetBytes(hmac);

            int hmacLen = hmacBytes.Length;
            int padCount = 0;
            while (((hmacLen++) % 64) != 0) padCount++;
            
            packet.Write((ushort)STUNAttribute.MessageIntegrity);
            packet.Write((ushort)hmacBytes.Length);
            for (int i = 0; i < padCount; i++)
                packet.Write((byte)0);
        }

        public string ReadString(NetworkPacket packet)
        {
            string value = packet.ReadString();
            int len = Encoding.UTF8.GetByteCount(value);
            int padCount = 4 - ((len+4) % 4);
            for(int i=0; i<padCount; i++)
            {
                packet.ReadByte();
            }
            return value;
        }

        public string ReadErrorCode(NetworkPacket packet)
        {
            ushort attrLength = packet.ReadUShort();
            uint bits = packet.ReadUInt();
            uint code = bits & 0xFF;
            uint codeClass = (bits & 0x700) >> 8;
            string phrase = packet.ReadString(attrLength-4);
            return "Error (" + codeClass + code + "): " + phrase;
        }

        public uint ReadUInt(NetworkPacket packet)
        {
            ushort attrLength = packet.ReadUShort();
            uint value = packet.ReadUInt();
            return value;
        }

        public string ReadUnknownAttributes(NetworkPacket packet)
        {
            ushort attrLength = packet.ReadUShort();
            string attrs = "";// new string[attrLength / 2];
            attrLength += (ushort)(attrLength % 4);
            for(int i=0; i< attrLength ; i+=2)
            {
                if (i > 0)
                    attrs += ", ";
                ushort attrId = packet.ReadUShort();
                try
                {
                    attrs += Enum.GetName(typeof(STUNAttribute), (STUNAttribute)attrId);
                }
                catch(Exception e)
                {
                    attrs += "" + attrId;
                }
            }
            if( attrLength % 4 != 0 )
            {
                int temp = packet.ReadUShort();
                Console.WriteLine("Extra Unknown Attr: " + temp);
            }
            return attrs;
        }

        public STUNAddress ReadMappedAddress(NetworkPacket packet)
        {
            ushort attrLength = packet.ReadUShort();

            STUNAddress sa = new STUNAddress();
            byte empty = packet.ReadByte();
            sa.family = packet.ReadByte();
            sa.port = packet.ReadUShort();
            
            switch(sa.family)
            {
                case 1:
                    sa.address = new byte[4];
                    
                    break;
                case 2:
                    sa.address = new byte[16];
                    break;
            }

            for (int i = 0; i < sa.address.Length; i++)
            {
                sa.address[i] = packet.ReadByte();
            }
            return sa;
        }

        public STUNAddress ReadXorMappedAddress(NetworkPacket packet)
        {
            ushort attrLength = packet.ReadUShort();

            STUNAddress sa = new STUNAddress();
            
            uint xorFlag = ((uint)BitConverter.ToUInt16(packet.ByteBuffer,4) >> 16);
            byte empty = packet.ReadByte();
            sa.family = packet.ReadByte();
            sa.port = (ushort)(packet.ReadUShort() ^ xorFlag);

            switch (sa.family)
            {
                case 1:
                    sa.address = new byte[4];
                    int address = packet.ReadInt() ^ (int)xorFlag;
                    sa.address[0] = (byte)((address & 0xf000) >> 24);
                    sa.address[1] = (byte)((address & 0x0f00) >> 16);
                    sa.address[2] = (byte)((address & 0x00f0) >> 8);
                    sa.address[3] = (byte)(address & 0x000f);
                    break;
                case 2:
                    sa.address = new byte[16];
                    byte[] xorFlags = new byte[16];

                    //byte[] cookieBytes = BitConverter.GetBytes(magicCookie);
                    //Array.Copy(cookieBytes, 0, xorFlags, 0, cookieBytes.Length);
                    Array.Copy(transactionID, 0, xorFlags, xorFlags.Length, transactionID.Length);

                    for (int i = 0; i < sa.address.Length; i++)
                    {
                        sa.address[i] = (byte)(packet.ReadByte() ^ xorFlags[i]);
                    }
                    break;
            }
            
            return sa;
        }


        public static string Encode(byte[] key, byte[] msgBytes)
        {
            HMACSHA1 myhmacsha1 = new HMACSHA1(key);
            //byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new MemoryStream(msgBytes);
            return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }

        

    }
}
