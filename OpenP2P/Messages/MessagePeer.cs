using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /// <summary>
    /// MessagePeer Packet format:
    /// |------------------------------------------------------------|
    /// |  Static Input (X bytes)                                    |
    /// |------------------------------------------------------------|
    /// |  Static State (Y bytes)                                    |
    /// |------------------------------------------------------------|
    /// |  Static Animation (Z bytes)                                |
    /// |------------------------------------------------------------|
    /// |  Dynamic Extras (W bytes)                                  |
    /// |------------------------------------------------------------|
    ///
    /// </summary>
    public class MessagePeer : NetworkMessage
    {
        public long timestamp = 0;

        public long responseTimestamp = 0;

        public override void WriteRequest(NetworkPacket packet)
        {
            packet.Write(timestamp);

        }
        public override void ReadRequest(NetworkPacket packet)
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
