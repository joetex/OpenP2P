using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkMessage
    {
        public bool isLittleEndian = true;
        public ResponseType responseType = 0;
        public Message messageType = Message.NULL;
        
        /// <summary>
        /// Response Flags: 0 = Client Send, 1 = Server Send, 2 = Client Response, 3 = Server Response
        /// </summary>
        const int ResponseFlags = (3 << 6); //bits 6 and 7
        const int BigEndianFlag = (1 << 8); //bit 8

        public virtual void OnReceive(NetworkStream stream) { }
        public virtual void OnSend(NetworkStream stream) { }
        
        public virtual void SetResponseType(ResponseType _responseType)
        {
            responseType = _responseType;
        }

        public virtual ResponseType GetResponseType()
        {
            return responseType;
        }

        public virtual void Write(NetworkStream stream)
        {
            WriteHeader(stream, messageType, responseType);
        }

        public virtual void WriteHeader(NetworkStream stream, Message _msgType, ResponseType _responseType)
        {
            int msgType = (int)_msgType;
            if (msgType < 0 || msgType >= (int)Message.LAST)
                msgType = 0;

            //add responseType to bits 6 and 7
            msgType |= (int)responseType << 6;

            //add little endian to bit 8
            if (!BitConverter.IsLittleEndian)
                msgType |= BigEndianFlag;

            responseType = _responseType;
            isLittleEndian = BitConverter.IsLittleEndian;

            stream.Write((byte)msgType);
        }

        public virtual int ReadHeader(NetworkStream stream)
        {
            int msgBits = stream.ReadByte();

            //check little endian flag on bit 8
            if ((msgBits & BigEndianFlag) == 0)
                isLittleEndian = true;

            //grab response bits 6 and 7 as an integer between [0-3]
            if ((msgBits & ResponseFlags) > 0)
                responseType = (ResponseType)(((msgBits & ~BigEndianFlag) & ResponseFlags) >> 6);

            //remove response and endian bits
            msgBits = msgBits & ~(BigEndianFlag | ResponseFlags);

            if (msgBits < 0 || msgBits >= (int)Message.LAST)
                msgBits = 0;

            return msgBits;
        }
    }
}
