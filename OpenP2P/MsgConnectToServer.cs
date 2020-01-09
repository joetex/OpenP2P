﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class MsgConnectToServer : NetworkMessage
    {
        public const int MAX_NAME_LENGTH = 32;
        public string msgUsername = "";
        public int msgNumber = 0;
        public short msgShort = 0;
        public bool msgBool = false;

        public bool responseConnected = false;
        public ushort responsePeerId = 0;
        public string string100Bytes = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

        public override void WriteMessage(NetworkPacket packet)
        {
            if (msgUsername.Length > 1436)
            {
                //msgUsername = msgUsername.Substring(0, MAX_NAME_LENGTH);
            }
            
            //packet.Write(msgUsername);
            //packet.Write(msgNumber);
            //packet.Write(msgShort);
            //packet.Write((byte)(msgBool == true ? 1 : 0));
            packet.Write(string100Bytes);
        }

        public override void ReadMessage(NetworkPacket packet)
        {
            msgUsername = "test";// packet.ReadString();
            //msgNumber = packet.ReadInt();
            //msgShort = packet.ReadShort();
            //msgBool = packet.ReadByte() > 0 ? true : false;
            string longStr = packet.ReadString();
        }


        public override void WriteResponse(NetworkPacket packet)
        {
            packet.Write((byte)1);
            packet.Write(responsePeerId);
        }

        

        public override void ReadResponse(NetworkPacket packet)
        {
            responseConnected = packet.ReadByte() != 0;
            responsePeerId = packet.ReadUShort();
        }
    }
}
