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

        ConnectToServer,
        ConnectToPeer,
        DisconnectFromServer,
        DisconnectFromPeer,
        //DisconnectFromServer,

        //interest mapping data sent to server
        //Peers will be connected together at higher priorities based on the 
        // "interest" mapping to a QuadTree (x, y, width, height) 
        Heartbeat,

        Raw,
        Event,
        RPC,

        //GetPeers,
        //ConnectTo,
        LAST
    }

    public enum SendType
    {
        Message,
        Response
    }

    public enum NetworkErrorType
    {
        None,
        ErrorReliableFailed,
        ErrorConnectToServer,
        ErrorMaxIdentitiesReached,
    }

    class NetworkProtocolTypes { } //dummy class
}
