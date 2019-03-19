using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MsgInvalid : NetworkMessage
    {

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
