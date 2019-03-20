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
        public string requestUsername = "";
        public bool responseConnected = false;

        public override void WriteRequest(NetworkStream stream)
        {
            if (requestUsername.Length > MAX_NAME_LENGTH)
            {
                requestUsername = requestUsername.Substring(0, MAX_NAME_LENGTH);
            }
            
            stream.Write(requestUsername);
        }

        public override void WriteResponse(NetworkStream stream)
        {
            stream.Write((byte)1);
        }

        public override void ReadRequest(NetworkStream stream)
        {
            requestUsername = stream.ReadString();
        }

        public override void ReadResponse(NetworkStream stream)
        {
            responseConnected = stream.ReadByte() != 0;
        }
    }
}
