using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    interface INetworkListener
    {
        void OnReceivePacket();
        void OnSendPacket();
        void OnParsePacket();

    }
}
