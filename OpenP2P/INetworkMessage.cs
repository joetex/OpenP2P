using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public interface INetworkMessage
    {
        void OnReceive(NetworkStream stream);
    }
}
