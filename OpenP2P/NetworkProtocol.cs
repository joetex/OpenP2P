
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        Request,
        Response,
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
        /// <summary>
        /// Response Flags: 0 = Client Send, 1 = Server Send, 2 = Client Response, 3 = Server Response
        /// </summary>
        const int ResponseFlags = (3 << 5); //bits 6 and 7
        const int BigEndianFlag = (1 << 7); //bit 8

        public NetworkProtocol(string remoteHost, int remotePort, int localPort)
        {
            socket = new NetworkSocket(remoteHost, remotePort, localPort);
            AttachSocketListener(socket);
            BindMessages();
        }
        
        /// <summary>
        /// Bind Messages to our Message Dictionary
        /// This uses reflection to map our Enum to a Message class
        /// </summary>
        public void BindMessages()
        {
            string enumName = "";
            NetworkMessage message = null;
            for (int i=0; i<(int)Message.LAST; i++)
            {
                enumName = Enum.GetName(typeof(Message), (Message)i);
                try
                {
                    message = (NetworkMessage)GetInstance("OpenP2P.Msg" + enumName);
                    message.messageType = (Message)i;
                }
                catch(Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    message = new MsgInvalid();
                }
                
                messages.Add(i, message);
            }
        }

        public override void AttachSocketListener(NetworkSocket _socket)
        {
            socket = _socket;
            socket.OnReceive += OnReceive;
            socket.OnSend += OnSend;
        }

        public override void AttachRequestListener(Message msgType, EventHandler<NetworkMessage> func)
        {
            GetMessage((int)msgType).OnRequest += func;
        }
        public override void AttachResponseListener(Message msgType, EventHandler<NetworkMessage> func)
        {
            GetMessage((int)msgType).OnResponse += func;
        }

        public NetworkMessage Create(Message _msgType)
        {
            NetworkMessage message = GetMessage((int)_msgType);
            return message;
        }
        
        public void Listen()
        {
            socket.Listen(null);
        }

        public void SendRequest(EndPoint ep, NetworkMessage message)
        {
            message.responseType = ResponseType.Request;
            Send(ep, message);
        }
        public void SendResponse(EndPoint ep, NetworkMessage message)
        {
            message.responseType = ResponseType.Response;
            Send(ep, message);
        }
        /*
        public void ClientResponse(NetworkMessage message)
        {
            message.responseType = ResponseType.ClientResponse;
            Send(message);
        }
        public void ServerResponse(NetworkMessage message)
        {
            message.responseType = ResponseType.ServerResponse;
            Send(message);
        }*/

        public void Send(EndPoint ep, NetworkMessage message)
        {
            NetworkStream stream = socket.Prepare(ep);
            stream.message = message;
            stream.messageType = (int)message.messageType;

            WriteHeader(stream);
            switch(message.responseType)
            {
                case ResponseType.Request: message.WriteRequest(stream); break;
                case ResponseType.Response: message.WriteResponse(stream); break;
            }
            
            socket.Send(stream);
        }

        public override void OnReceive(object sender, NetworkStream stream)
        {
            NetworkMessage message = ReadHeader(stream);
            
            message.InvokeOnRead(stream);
            
            /*
            switch(message.responseType)
            {
                case ResponseType.ClientResponse: ; break;
            }*/
        }

        public override void OnSend(object sender, NetworkStream stream)
        {
            //messages[message].OnReceive(stream);
        }

        public override void WriteHeader(NetworkStream stream)
        {
            NetworkMessage message = (NetworkMessage)stream.message;

            int msgBits = (int)message.messageType;
            if (msgBits < 0 || msgBits >= (int)Message.LAST)
                msgBits = 0;

            //add responseType to bits 6 and 7
            msgBits |= (int)message.responseType << 5;

            //add little endian to bit 8
            if (!BitConverter.IsLittleEndian)
                msgBits |= BigEndianFlag;
            
            message.isLittleEndian = BitConverter.IsLittleEndian;

            stream.Write((byte)msgBits);
        }

        public override NetworkMessage ReadHeader(NetworkStream stream)
        {
            int msgBits = stream.ReadByte();

            bool isLittleEndian = false;
            ResponseType responseType = ResponseType.ClientResponse;
            //check little endian flag on bit 8
            if ((msgBits & BigEndianFlag) == 0)
                isLittleEndian = true;

            //grab response bits 6 and 7 as an integer between [0-3]
            //if ((msgBits & ResponseFlags) > 0)
                responseType = (ResponseType)(((msgBits & ~BigEndianFlag) & ResponseFlags) >> 5);

            //remove response and endian bits
            msgBits = msgBits & ~(BigEndianFlag | ResponseFlags);

            if (msgBits < 0 || msgBits >= (int)Message.LAST)
                return GetMessage(0);

            NetworkMessage message = GetMessage(msgBits);
            message.isLittleEndian = isLittleEndian;
            message.responseType = responseType;

            return message;
        }


        public object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public override NetworkMessage GetMessage(int id)
        {
            if (!messages.ContainsKey(id))
                return messages[0];
            return messages[id];
        }
    }
}
