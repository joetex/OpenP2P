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

        public string string100Bytes = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        public override void WriteMessage(NetworkPacket packet)
        {
            packet.Write(timestamp);
            packet.Write(string100Bytes);
        }
        public override void ReadMessage(NetworkPacket packet)
        {
            timestamp = packet.ReadLong();
            string longStr = packet.ReadString();
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
