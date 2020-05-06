using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public enum MessagePolicy
    {
        Unreliable,
        Reliable,
        Stream
    }

    public class MessageFSG : NetworkMessage
    {
        public class HeaderFSG : NetworkMessageHeader
        {
            //encoded into packet
            public bool isReliable = false;
            public bool isStream = false;
            public bool isSTUN = false;
            public SendType sendType = 0;
            public MessageType channelType = MessageType.Invalid;
            public ushort sequence = 0;
            

            //packet status
            public uint ackkey = 0;
            public long sentTime = 0;
            public int retryCount = 0;
        }

        public HeaderFSG header = new HeaderFSG();

    }
}
