
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    
    /// <summary>
    /// Network Protocol for header defining the type of message
    /// 
    /// Single Byte Format:
    ///     0 0 000000
    ///     1st left most bit: 0 = Request, 1 = Response (are we making a request or are we responding to a request?)
    ///     2nd left most bit: 0 = Little Endian, 1 = Big Endian  (iOS/Mac uses Big Endian, others use Little)
    ///     6 right bits: Message Type, up to 64 different message types
    /// </summary>
    public class NetworkProtocol : NetworkProtocolBase
    {
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

        const int ResponseFlag = (1 << 8);
        const int BigEndianFlag = (1 << 7);

        public Dictionary<Message, INetworkMessage> messages = new Dictionary<Message, INetworkMessage>();

        public NetworkProtocol()
        {
            Start();
        }

        public void Start()
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

        public void OnReceive(object sender, NetworkStream stream)
        {
            //Message msg = stream.ReadHeader();
            //messages[msg].OnReceive(stream);
        }

        public void OnSend(object sender, NetworkStream stream)
        {
            //messages[msg].OnReceive(stream);
        }


        public override void WriteHeader(NetworkStream stream, byte mt, bool isResp)
        {
            int msgType = (int)mt;

            if (isResp)
                msgType |= ResponseFlag;

            if (!BitConverter.IsLittleEndian)
                msgType |= BigEndianFlag;

            isResponse = isResp;
            isLittleEndian = BitConverter.IsLittleEndian;

            stream.Write((byte)msgType);
        }

        public override byte ReadHeader(NetworkStream stream)
        {
            byte msgType = stream.ReadByte();
            Message msg = (Message)msgType;
            messages[msg].OnReceive(stream);

            return msgType;
        }

    }
}
