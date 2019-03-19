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
        public string userName = "";
        public bool connected = false;

        public override void Write(NetworkStream stream)
        {
            if( userName.Length > MAX_NAME_LENGTH )
            {
                userName = userName.Substring(0, MAX_NAME_LENGTH);
            }
            
            stream.Write(userName);
        }

        public override void Read(NetworkStream stream)
        {
            userName = stream.ReadString();
            base.Read(stream);
        }
        
    }
}
