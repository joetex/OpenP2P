using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MessageHeartbeat : INetworkMessage
    {
        public void OnReceive(NetworkStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
