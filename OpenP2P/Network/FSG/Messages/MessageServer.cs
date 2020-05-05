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
    public class MessageServer : MessageFSG
    {
        public enum ServerMethod
        {
            CONNECT = 0,
            HEARTBEAT = 1
        }
        
        public struct RequestConnect
        {
            public string username;

            public void Write(NetworkPacket packet)
            {
                if (username.Length > MAX_NAME_LENGTH)
                    username = username.Substring(0, MAX_NAME_LENGTH);

                packet.Write(username);
            }

            public void Read(NetworkPacket packet)
            {
                username = packet.ReadString();
            }
        }
        
        public struct RequestHeartbeat
        {
        }

        public struct ResponseConnect
        {
            public bool connected;
            public int sendRate;
            public ushort peerId;

            public void Write(NetworkPacket packet)
            {
                packet.Write((byte)1);
                packet.Write(sendRate);
                packet.Write(peerId);
            }

            public void Read(NetworkPacket packet)
            {
                connected = packet.ReadByte() != 0;
                sendRate = packet.ReadInt();
                peerId = packet.ReadUShort();
                Console.WriteLine("Setting server send rate: {0}", sendRate);
            }
        }
        public struct ResponseHearbeat
        {
            public bool connected;
        }

        public struct Request
        {
            public RequestConnect connect;
            public RequestHeartbeat hearbeat;
        }

        public struct Response
        {
            public ResponseConnect connect;
            public ResponseHearbeat hearbeat;
        }
        public ServerMethod method;
        public Request request;
        public Response response;

        public const int MAX_NAME_LENGTH = 32;
        
        public override void WriteRequest(NetworkPacket packet)
        {
            packet.Write((byte)method);

            switch(method)
            {
                case ServerMethod.CONNECT:
                    request.connect.Write(packet);
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
            
        }

        public override void ReadRequest(NetworkPacket packet)
        {
            method = (ServerMethod)packet.ReadByte();
            switch (method)
            {
                case ServerMethod.CONNECT:
                    request.connect.Read(packet);
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
        }


        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write((byte)method);

            switch (method)
            {
                case ServerMethod.CONNECT:
                    response.connect.Write(packet);
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
        }
        
        public override void ReadResponse(NetworkPacket packet)
        {
            method = (ServerMethod)packet.ReadByte();
            switch (method)
            {
                case ServerMethod.CONNECT:
                    response.connect.Read(packet);
                    break;
                case ServerMethod.HEARTBEAT:

                    break;
            }
        }
    }
}
