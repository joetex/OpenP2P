using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public partial class ProtocolFSG
    { 

        public NetworkPacket SendReliableMessage(EndPoint ep, MessageFSG message)
        {
            IPEndPoint ip = NetworkSocket.GetIPv6(ep);
            NetworkPacket packet = socket.Prepare(ep);

            message.header.destination = ep;
            message.header.channelType = messageFactory.GetMessageType(message);
            message.header.isReliable = true;
            message.header.sendType = SendType.Message;
            message.header.id = ident.local.id;

            if (message.header.retryCount == 0)
                message.header.sequence = ident.local.NextSequence(message);

            Send(packet, message);

            return packet;
        }

        public List<NetworkPacket> SendStream(EndPoint ep, MessageFSG message)
        {
            IPEndPoint ip = NetworkSocket.GetIPv6(ep);
            MessageStream stream = (MessageStream)message;
            List<NetworkPacket> packets = new List<NetworkPacket>();

            stream.header.channelType = messageFactory.GetMessageType(stream);
            stream.header.isReliable = true;
            stream.header.isStream = true;
            stream.header.sendType = SendType.Message;
            stream.header.sequence = ident.local.NextSequence(stream);
            stream.header.id = ident.local.id;

            while (stream.segmentLen > 0 && stream.startPos < stream.byteData.Length)
            {
                NetworkPacket packet = socket.Prepare(ep);
                packet.messages.Add(stream);

                WriteHeader(packet, stream);

                stream.WriteRequest(packet);

                socket.Send(packet);
                Console.WriteLine("Sent " + (stream.segmentLen) + " bytes");
            }

            return packets;
        }

        public NetworkPacket SendMessage(EndPoint ep, MessageFSG message)
        {
            IPEndPoint ip = NetworkSocket.GetIPv6(ep);
            NetworkPacket packet = socket.Prepare(ep);

            message.header.channelType = messageFactory.GetMessageType(message);
            message.header.isReliable = false;
            message.header.sendType = SendType.Message;
            message.header.sequence = ident.local.NextSequence(message);
            message.header.id = ident.local.id;

            Send(packet, message);

            return packet;
        }


        public NetworkPacket SendResponse(MessageFSG requestMessage, MessageFSG response)
        {
            NetworkPacket packet = socket.Prepare(requestMessage.header.source);

            if (requestMessage.header.source.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                packet.networkIPType = NetworkSocket.NetworkIPType.IPv4;
            else
                packet.networkIPType = NetworkSocket.NetworkIPType.IPv6;

            response.header.channelType = requestMessage.header.channelType;
            response.header.isReliable = requestMessage.header.isReliable;
            response.header.sendType = SendType.Response;
            response.header.sequence = requestMessage.header.sequence;
            response.header.id = requestMessage.header.id;
            response.header.ackkey = requestMessage.header.ackkey;

            Send(packet, response);

            return packet;
        }


        public void Send(NetworkPacket packet, MessageFSG message)
        {
            packet.messages.Add(message);// = message;
            WriteHeader(packet, message);
            switch (message.header.sendType)
            {
                case SendType.Message: message.WriteRequest(packet); break;
                case SendType.Response: message.WriteResponse(packet); break; 
            }

            net.SendPacket(packet);
        }

        public uint GenerateAckKey(NetworkPacket packet, MessageFSG message)
        {
            uint sequence = message.header.sequence;
            uint id = message.header.id;

            uint key = sequence | (id << 16);
            return key;
        }

    }
}
