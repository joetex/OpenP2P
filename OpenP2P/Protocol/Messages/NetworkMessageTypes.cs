using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public enum MessageType
    {
        Invalid,

        Server,
        Peer,
        //interest mapping data sent to server
        //Peers will be connected together at higher priorities based on the 
        // "interest" mapping to a QuadTree (x, y, width, height) 
        Stream, //used for large data transfer
        STUN,
        Event,
        RPC,
        LAST
    }

    public partial class NetworkMessageFactory
    {

        public Dictionary<uint, Func<NetworkMessage>> constructors = new Dictionary<uint, Func<NetworkMessage>>()
        {
            {(uint)MessageType.Invalid, Create<MessageInvalid> },
            {(uint)MessageType.Server, Create<MessageServer>},
            {(uint)MessageType.Peer, Create<MessageInvalid> },
            {(uint)MessageType.Stream, Create<MessageStream> },
            {(uint)MessageType.STUN, Create<MessageSTUN> },
            {(uint)MessageType.Event, Create<MessageInvalid>},
            {(uint)MessageType.RPC, Create<MessageInvalid>},
            {(uint)MessageType.LAST, Create<MessageInvalid>}
        };

    }
}
