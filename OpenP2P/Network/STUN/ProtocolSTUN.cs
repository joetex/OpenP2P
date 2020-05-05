using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class ProtocolSTUN : NetworkProtocol
    {
        public ProtocolSTUN(NetworkManager _manager) : base(_manager)
        {

        }

        public void BeginRead(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public void BeginWrite(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public NetworkMessage ReadHeader(NetworkPacket packet)
        {
            throw new NotImplementedException();
        }

        public void ReadMessage(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public void WriteHeader(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public void WriteMessage(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
