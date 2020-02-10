using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    

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
        ErrorNoResponseSTUN,
        ErrorMaxIdentitiesReached,
    }

    class NetworkProtocolTypes { } //dummy class
}
