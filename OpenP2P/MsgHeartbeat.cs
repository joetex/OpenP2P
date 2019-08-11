using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MsgHeartbeat : NetworkMessage
    {
        public long timestamp = 0;


        public override void WriteMessage(NetworkPacket packet)
        {
            packet.Write(timestamp);
        }

        public override void WriteResponse(NetworkPacket packet)
        {
        }

        public override void ReadMessage(NetworkPacket packet)
        {
            timestamp = packet.ReadLong();
        }

        public override void ReadResponse(NetworkPacket packet)
        {

        }
    }
}
