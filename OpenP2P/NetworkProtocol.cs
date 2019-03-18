
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public enum Message
    {
        NULL,

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
        ConnectTo,
        LAST
    }

    public enum ResponseType
    {
        ClientSend,
        ServerSend,
        ClientResponse,
        ServerResponse
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
    public class NetworkProtocol : NetworkProtocolBase
    {
        public NetworkProtocol(string remoteHost, int remotePort, int localPort)
        {
            socket = new NetworkSocket(remoteHost, remotePort, localPort);
            AttachListener(socket);
            BindMessages();
        }
        
        /// <summary>
        /// Bind Messages to our Message Dictionary
        /// This uses reflection to map our Enum to a Message class
        /// </summary>
        public void BindMessages()
        {
            string enumName = "";
            NetworkMessage msg = null;
            for (int i=0; i<(int)Message.LAST; i++)
            {
                enumName = Enum.GetName(typeof(Message), (Message)i);
                try
                {
                    msg = (NetworkMessage)GetInstance("OpenP2P.Message" + enumName);
                    msg.messageType = (Message)i;
                }
                catch(Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    msg = new MessageInvalid();
                }
                
                messages.Add(i, msg);
            }
        }

        public override void AttachListener(NetworkSocket _socket)
        {
            socket = _socket;
            socket.OnReceive += OnReceive;
            socket.OnSend += OnSend;
        }

        public object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public override NetworkMessage GetMessage(int id)
        {
            if (!messages.ContainsKey(id))
                return null;
            return messages[id];
        }

        public NetworkMessage Prepare(Message _msgType)
        {
            NetworkMessage msg = messages[(int)_msgType];
            return msg;
        }
        
        public void Send(NetworkMessage msg)
        {
            NetworkStream stream = socket.Prepare();
            stream.message = msg;
            stream.messageType = (int)msg.messageType;
            
            msg.Write(stream);
            socket.Send(stream);
        }

        public override void OnReceive(object sender, NetworkStream stream)
        {
            Message msg = (Message)ReadHeader(stream);
            messages[(int)msg].OnReceive(stream);
        }

        public override void OnSend(object sender, NetworkStream stream)
        {
            //messages[msg].OnReceive(stream);
        }

        public override void WriteHeader(NetworkStream stream, int mt, int _responseType)
        {
            stream.message = messages[mt];
            NetworkMessage message = (NetworkMessage)stream.message;
            message.WriteHeader(stream, (Message)mt, (ResponseType)_responseType);
        }

        

    }
}
