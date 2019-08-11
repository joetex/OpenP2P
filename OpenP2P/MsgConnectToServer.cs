using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MsgConnectToServer : NetworkMessage
    {
        public const int MAX_NAME_LENGTH = 32;
        public string msgUsername = "";

        public bool responseConnected = false;
        public ushort responsePeerId = 0;

        public override void WriteMessage(NetworkStream stream)
        {
            if (msgUsername.Length > MAX_NAME_LENGTH)
            {
                msgUsername = msgUsername.Substring(0, MAX_NAME_LENGTH);
            }
            
            stream.Write(msgUsername);
        }

        public override void WriteResponse(NetworkStream stream)
        {
            stream.Write((byte)1);
            stream.Write(responsePeerId);
        }

        public override void ReadMessage(NetworkStream stream)
        {
            msgUsername = stream.ReadString();
        }

        public override void ReadResponse(NetworkStream stream)
        {
            responseConnected = stream.ReadByte() != 0;
            responsePeerId = stream.ReadUShort();
        }
    }
}
