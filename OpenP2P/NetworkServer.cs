using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkServer
    {
        public NetworkSocket socket = null;
        public Dictionary<string, string> connections = new Dictionary<string, string>();
    
        public NetworkServer(int localPort)
        {
            Setup(localPort);
        }

        public void Setup(int localPort)
        {
            socket = new NetworkSocket(localPort);
        }
    }
}
