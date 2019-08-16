using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class MsgHeartbeat : NetworkMessage
    {
        public long timestamp = 0;

        public long responseTimestamp = 0;

        public override void WriteMessage(NetworkPacket packet)
        {
            packet.Write(timestamp);
        }
        public override void ReadMessage(NetworkPacket packet)
        {
            timestamp = packet.ReadLong();
        }


        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write(responseTimestamp);
        }
        public override void ReadResponse(NetworkPacket packet)
        {
            responseTimestamp = packet.ReadLong();
        }
    }
}
