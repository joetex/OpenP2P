using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /// <summary>
    /// MessageServer Packet format:
    /// |------------------------------------------------------------|
    /// |  Command Type (1 byte)                                     |
    /// |------------------------------------------------------------|
    /// |  Command Length (2 bytes)                                  |
    /// |------------------------------------------------------------|
    /// |  Command Data (X bytes)                                    |
    /// |------------------------------------------------------------|
    ///
    /// </summary>
    public class MessageServer : NetworkMessage
    {
        public enum ServerMethod
        {
            CONNECT = 0,
            HEARTBEAT = 1
        }
        
        public struct RequestConnect
        {
            public string username;
        }
        
        public struct RequestHeartbeat
        {
        }

        public struct ResponseConnect
        {
            public bool connected;
            public int sendRate;
            public ushort peerId;

        }
        public struct ResponseHearbeat
        {
            public bool connected;
        }

        public struct Request
        {
            public ServerMethod method;
            public RequestConnect connect;
            public RequestHeartbeat hearbeat;
        }

        public struct Response
        {
            public ServerMethod method;
            public ResponseConnect connect;
            public ResponseHearbeat hearbeat;
        }

        public Request request;
        public Response response;

        public const int MAX_NAME_LENGTH = 32;
        
        public override void WriteRequest(NetworkPacket packet)
        {
            packet.Write((byte)request.method);

            switch(request.method)
            {
                case ServerMethod.CONNECT:
                    if (request.connect.username.Length > MAX_NAME_LENGTH)
                        request.connect.username = request.connect.username.Substring(0, MAX_NAME_LENGTH);

                    packet.Write(request.connect.username);
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
            
        }

        public override void ReadRequest(NetworkPacket packet)
        {
            ServerMethod type = (ServerMethod)packet.ReadByte();
            switch (type)
            {
                case ServerMethod.CONNECT:
                    request.connect.username = packet.ReadString();
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
        }


        public override void WriteResponse(NetworkPacket packet)
        {
            switch (response.method)
            {
                case ServerMethod.CONNECT:
                    packet.Write((byte)1);
                    packet.Write(response.connect.sendRate);
                    packet.Write(response.connect.peerId);
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
        }
        
        public override void ReadResponse(NetworkPacket packet)
        {
            switch (response.method)
            {
                case ServerMethod.CONNECT:
                    response.connect.connected = packet.ReadByte() != 0;
                    response.connect.sendRate = packet.ReadInt();
                    response.connect.peerId = packet.ReadUShort();
                    Console.WriteLine("Setting server send rate: {0}", response.connect.sendRate);
                    NetworkConfig.ThreadSendSleepPacketSizePerFrame = response.connect.sendRate;
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
        }
    }
}
