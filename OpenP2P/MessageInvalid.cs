using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MessageInvalid : NetworkMessage
    {
        public NetworkProtocol.Message id = NetworkProtocol.Message.NULL;

        public int GetResponseType()
        {
            throw new NotImplementedException();
        }

        public void OnReceive(NetworkStream stream)
        {
            
        }

        public void OnSend(NetworkStream stream)
        {

        }

        public void Write(NetworkStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
