using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class MsgConnectToServer : NetworkMessage
    {
        public const int MAX_NAME_LENGTH = 32;
        public string msgUsername = "";
        public int msgNumber = 0;
        public short msgShort = 0;
        public bool msgBool = false;

        public bool responseConnected = false;
        public ushort responsePeerId = 0;

        public override void WriteMessage(NetworkPacket packet)
        {
            if (msgUsername.Length > MAX_NAME_LENGTH)
            {
                msgUsername = msgUsername.Substring(0, MAX_NAME_LENGTH);
            }
            
            packet.Write(msgUsername);
            packet.Write(msgNumber);
            packet.Write(msgShort);
            packet.Write((byte)(msgBool == true ? 1 : 0));
        }

        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write((byte)1);
            packet.Write(responsePeerId);
        }

        public override void ReadMessage(NetworkPacket packet)
        {
            msgUsername = packet.ReadString();
            msgNumber = packet.ReadInt();
            msgShort = packet.ReadShort();
            msgBool = packet.ReadByte() > 0 ? true : false;

        }

        public override void ReadResponse(NetworkPacket packet)
        {
            responseConnected = packet.ReadByte() != 0;
            responsePeerId = packet.ReadUShort();
        }
    }
}
