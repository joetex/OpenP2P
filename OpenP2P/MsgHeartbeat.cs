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


        public override void WriteRequest(NetworkStream stream)
        {
            stream.Write(timestamp);
        }

        public override void WriteResponse(NetworkStream stream)
        {
        }

        public override void ReadRequest(NetworkStream stream)
        {
            timestamp = stream.ReadLong();
        }

        public override void ReadResponse(NetworkStream stream)
        {

        }
    }
}
