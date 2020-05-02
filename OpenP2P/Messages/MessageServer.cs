using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /// <summary>
    /// MessageServer Packet format:
    /// |------------------------------------------------------------|
    /// |  Command Type (1 byte)                                     |
    /// |------------------------------------------------------------|
    /// |  Command Length (2 bytes)                                  |
    /// |------------------------------------------------------------|
    /// |  Command Data (X bytes)                                    |
    /// |------------------------------------------------------------|
    ///
    /// </summary>
    public class MessageServer : NetworkMessage
    {
        public const int MAX_NAME_LENGTH = 32;
        public string msgUsername = "";
        public int msgNumber = 0;
        public short msgShort = 0;
        public bool msgBool = false;

        public bool responseConnected = false;
        public ushort responsePeerId = 0;
        public string string100Bytes = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

        public override void WriteMessage(NetworkPacket packet)
        {
            if (msgUsername.Length > 1436)
            {
                //msgUsername = msgUsername.Substring(0, MAX_NAME_LENGTH);
            }

            double test = 5.1234;
            float test2 = 5.4321f;

            //Console.WriteLine("Sending double: " + test);
            //Console.WriteLine("Sending float: " + test2);
            packet.Write(string100Bytes);
            packet.Write(test);
            packet.Write(test2);

            packet.Write(msgUsername);
            packet.Write(msgNumber);
            packet.Write(msgShort);
            packet.Write((byte)(msgBool == true ? 1 : 0));
            //packet.Write(string100Bytes);
        }

        public override void ReadMessage(NetworkPacket packet)
        {
            string bytes1000 = packet.ReadString();
            double test = packet.ReadDouble();
            double test2 = packet.ReadFloat();

            //Console.WriteLine("Recv double: " + test);
            //Console.WriteLine("Recv float: " + test2);
            msgUsername = packet.ReadString();
            msgNumber = packet.ReadInt();
            msgShort = packet.ReadShort();
            msgBool = packet.ReadByte() > 0 ? true : false;
            //string longStr = packet.ReadString();
        }


        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write((byte)1);
            packet.Write(responsePeerId);
        }

        

        public override void ReadResponse(NetworkPacket packet)
        {
            responseConnected = packet.ReadByte() != 0;
            responsePeerId = packet.ReadUShort();
        }
    }
}
