using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public interface INetworkProtocol
    {
        void BeginWrite(NetworkPacket packet, NetworkMessage message);
        void WriteHeader(NetworkPacket packet, NetworkMessage message);
        void WriteMessage(NetworkPacket packet, NetworkMessage message);

        void BeginRead(NetworkPacket packet, NetworkMessage message);
        NetworkMessage ReadHeader(NetworkPacket packet);
        void ReadMessage(NetworkPacket packet, NetworkMessage message);

        void OnSocketSend(object sender, NetworkPacket packet);
        void OnSocketReceive(object sender, NetworkPacket packet);

    }
}
