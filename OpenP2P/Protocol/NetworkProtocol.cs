using OpenP2P.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P.Protocol
{
    public interface IMessage
    {
        void Request(NetworkStream stream);
        void Response(NetworkStream stream);
        void OnReceive(NetworkStream stream);
    }

    public enum Message
    {
        ConnectToServer,
        DisconnectFromServer,

        //interest mapping data sent to server
        //Peers will be connected together at higher priorities based on the 
        // "interest" mapping to a QuadTree (x, y, width, height) 
        Heartbeat,

        Raw,
        Event,
        RPC,

        GetPeers,
        ConnectTo
    }

    /// <summary>
    /// Network Protocol for header defining the type of message
    /// 
    /// Single Byte Format:
    ///     0 0 000000
    ///     1st left most bit: 0 = Request, 1 = Response (are we making a request or are we responding to a request?)
    ///     2nd left most bit: 0 = Little Endian, 1 = Big Endian  (iOS/Mac uses Big Endian, others use Little)
    ///     6 right bits: Message Type, up to 64 different message types
    /// </summary>
    public class NetworkProtocol
    {
        public static Dictionary<Message, IMessage> messages = new Dictionary<Message, IMessage>();

        public static void Start()
        {
            messages.Add(Message.ConnectToServer, new MessageConnectToServer());
            messages.Add(Message.DisconnectFromServer, new MessageConnectToServer());
            messages.Add(Message.Heartbeat, new MessageHeartbeat());
            messages.Add(Message.Raw, new MessageConnectToServer());
            messages.Add(Message.Event, new MessageConnectToServer());
            messages.Add(Message.RPC, new MessageConnectToServer());
            messages.Add(Message.GetPeers, new MessageConnectToServer());
            messages.Add(Message.ConnectTo, new MessageConnectToServer());
        }

        public static void Request(Message mt, NetworkStream stream)
        {
            messages[mt].Request(stream);
        }

        public static void Response(Message mt, NetworkStream stream)
        {
            messages[mt].Response(stream);
        }


    }
}
