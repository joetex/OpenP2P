using StringPrep;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public uint magicCookie = 0x2112A442;

        public bool integrity = false;

        public NetworkSerializer serializer = new NetworkSerializer(NetworkConfig.BufferMaxLength);
        public List<STUNAttribute> attributeTypes = new List<STUNAttribute>();
        public List<byte[]> attributeBytes = new List<byte[]>();
        public Dictionary<STUNAttribute, object> response = new Dictionary<STUNAttribute, object>();

        public string username = "joel";
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
            packet.Write((ushort)0);

            //transaction id 
            header.ackkey = BitConverter.ToUInt32(transactionID, 0);
            packet.Write(transactionID);

            //attributes
            for(int i=0; i<attributeBytes.Count; i++)
            {
                LogAttribute(i);
                packet.Write(attributeBytes[i]);
            }

            //update message length
            int totalLength = packet.byteLength - 20;
            int lastPos = packet.byteLength;
            if (integrity)
                totalLength += 24;
            packet.byteLength = 2;
            packet.Write((ushort)totalLength);
            packet.byteLength = lastPos;

            //method integrity goes here
            if ( integrity )
            {
                //GenerateMessageIntegrity(packet);
                AddMessageIntegrity(packet);
            }
            
            Console.WriteLine("Message Length = " + totalLength);
            Console.WriteLine("Total Bytes: " + packet.byteLength);

            //cleanup
            attributeBytes.Clear();
        }
     
        public void LogAttribute(int index)
        {
            STUNAttribute attr = attributeTypes[index];
            string valueStr = "";
            if (response.ContainsKey(attr))
            {
                object value = response[attr];

                if (value is string v)
                    valueStr = v;
                else if (value is byte[] b)
                    valueStr = NetworkSerializer.ByteArrayToHexString(b);
                else
                    valueStr = value.ToString();
            }
            Console.WriteLine("Write Attribute: " + Enum.GetName(typeof(STUNAttribute), attr) + " = " + valueStr);
        }

        


        public void WriteChangeRequest(bool changeIP, bool changePort) {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)STUNAttribute.ChangeRequest);
            serializer.Write((ushort)4);

            int flags = (!changeIP ? 0 : (1 << 2)) | (!changePort ? 0 : (1 << 1));
            serializer.Write(flags);

            attributeTypes.Add(STUNAttribute.ChangeRequest);
            attributeBytes.Add(serializer.ToArray());
        }

        public void WriteBytes(STUNAttribute attr, byte[] bytes)
        {
            serializer.SetBufferLength(0);
            serializer.Write((ushort)attr);
            serializer.Write((ushort)bytes.Length);
            serializer.Write(bytes);

            int len = bytes.Length;
            while (((len++) % 4) != 0)
                serializer.Write((byte)0);

            Console.WriteLine("Attribute: " + Enum.GetName(typeof(STUNAttribute), attr) + " = " + NetworkSerializer.ByteArrayToHexString((byte[])bytes));
            response.Add(attr, bytes);
            attributeTypes.Add(attr);
            attributeBytes.Add(serializer.ToArray());

        }

        public void WriteString(STUNAttribute attr, string text)
        {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)attr);
            int len = Encoding.UTF8.GetByteCount(text);
            int padding = (len % 4);
            //serializer.Write((ushort)len);

            int startLen = serializer.byteLength;
            serializer.Write(text, len);
            //int textLen = serializer.byteLength - startLen - 2;
    
            //pad to multiple of 4
            while (((len++) % 4) != 0) 
                serializer.Write((byte)0);

            response.Add(attr, text);
            attributeTypes.Add(attr);
            attributeBytes.Add(serializer.ToArray());
        }

        public void WriteUInt(STUNAttribute attr, uint value)
        {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)attr);
            serializer.Write((ushort)4);
            serializer.Write(value);

            response.Add(attr, value);
            attributeTypes.Add(attr);
            attributeBytes.Add(serializer.ToArray());
        }

        public void WriteEmpty(STUNAttribute attr)
        {
            serializer.SetBufferLength(0);

            serializer.Write((ushort)attr);
            serializer.Write((ushort)0);

            response.Add(attr, "");
            attributeTypes.Add(attr);
            attributeBytes.Add(serializer.ToArray());
        }

       
        public void WriteMessageIntegrity()
        {
            integrity = true;
        }

        

        public void AddMessageIntegrity(NetworkPacket packet)
        {
            string saslPassword = new SASLprep().Prepare(password);
            byte[] hmacSha1Key;
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                String valueToHashMD5 = string.Format("{0}:{1}:{2}",username, realm, saslPassword);
                hmacSha1Key = md5.ComputeHash(Encoding.UTF8.GetBytes(valueToHashMD5));
            }


            byte[] hmacBytes = null;// this.ComputeHMAC(packet, hmacSha1Key);
            using (HMACSHA1 hmacSha1 = new HMACSHA1(hmacSha1Key))
            {
                hmacBytes = hmacSha1.ComputeHash(packet.ByteBuffer, 0, packet.byteLength);
            }

            int hmacLen = hmacBytes.Length;

            packet.Write((ushort)STUNAttribute.MessageIntegrity);
            packet.Write((ushort)hmacBytes.Length);
            packet.Write(hmacBytes);
       
            Console.WriteLine("Write Attribute: MessageIntegrity(2) MD5 = " + NetworkSerializer.ByteArrayToHexString(hmacSha1Key));
            Console.WriteLine("Write Attribute: MessageIntegrity(2) HMAC = " + NetworkSerializer.ByteArrayToHexString(hmacBytes));
        }

        /// <summary>
        /// Computes a HMAC SHA1 based on this StunMessage attributes
        /// </summary>
        /// <param name="hmacSha1Key">The key of HMAC SHA1 computation algorithm</param>
        /// <returns>The HMAC computed value of this StunMessage</returns>
        private byte[] ComputeHMAC(NetworkPacket packet, byte[] hmacSha1Key)
        {
            byte[] hashed = null;

            using (HMACSHA1 hmacSha1 = new HMACSHA1(hmacSha1Key))
            {
                hashed = hmacSha1.ComputeHash(packet.ByteBuffer, 0, packet.byteLength);
            }

            return hashed;
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
                attributeTypes.Add(attrType);
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
                    case STUNAttribute.Realm:
                        response.Add(attrType, ReadString(packet));
                        break;
                    case STUNAttribute.Username:
                        response.Add(attrType, ReadString(packet));
                        break;
                    default:
                        ushort attrLen = packet.ReadUShort();
                        byte[] bytes = packet.ReadBytes(attrLen);
                        response.Add(attrType, bytes);

                        while (((attrLen++) % 4) != 0)
                            packet.ReadByte();

                        break;
                }
            }

        }


        public string ReadString(NetworkPacket packet)
        {
            string value = packet.ReadString();
            int len = Encoding.UTF8.GetByteCount(value);
            while( ((len++) % 4) != 0 )
                packet.ReadByte();
            
            return value;
        }

        public string ReadErrorCode(NetworkPacket packet)
        {
            ushort attrLength = packet.ReadUShort();
            uint bits = packet.ReadUInt();
            uint code = bits & 0xFF;
            uint codeClass = (bits & 0x700) >> 8;
            string phrase = packet.ReadString(attrLength-4);
            while ((attrLength++) % 4 != 0)
                packet.ReadByte();
            return "Error (" + codeClass + code.ToString("D2") + "): " + phrase;
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
            
            ushort xorFlag16 = (ushort)(magicCookie >> 16);
            byte empty = packet.ReadByte();
            sa.family = packet.ReadByte();
            sa.port = (ushort)(packet.ReadUShort() ^ xorFlag16);

            Console.WriteLine("XorMappedAddress - family: " + sa.family);
            Console.WriteLine("XorMappedAddress - port: " + sa.port );

            //sa.port = (ushort)(sa.port ^ xorFlag);
            switch (sa.family)
            {
                case 1:
                    byte[] xorFlagBytes = new byte[4];
                    Array.Copy(packet.ByteBuffer, 4, xorFlagBytes, 0, 4);
                    Array.Reverse(xorFlagBytes);
                    uint xorFlag32 = BitConverter.ToUInt32(xorFlagBytes, 0);

                    sa.address = new byte[4];
                    uint address = packet.ReadUInt() ^ xorFlag32;
                    sa.address[0] = (byte)((address & 0xff000000) >> 24);
                    sa.address[1] = (byte)((address & 0x00ff0000) >> 16);
                    sa.address[2] = (byte)((address & 0x0000ff00) >> 8);
                    sa.address[3] = (byte) (address & 0x000000ff);
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
        
        

    }
}
