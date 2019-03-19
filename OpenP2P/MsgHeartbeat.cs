using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MsgHeartbeat : NetworkMessage
    {
        public int GetResponseType()
        {
            throw new NotImplementedException();
        }

        public void OnReceive(NetworkStream stream)
        {
            throw new NotImplementedException();
        }

        public void OnSend(NetworkStream stream)
        {

        }

        public void Write(NetworkStream stream)
        {

        }
    }
}
