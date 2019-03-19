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

        public event EventHandler<NetworkMessage> OnReceiveMessage;
        public event EventHandler<NetworkMessage> OnSendMessage;

        public virtual void Write(NetworkStream stream)
        {

        }

        public virtual void Read(NetworkStream stream) {
            
        }

        public virtual void OnRead(NetworkStream stream)
        {
            OnReceiveMessage.Invoke(stream, this);
        }

        public virtual void OnReceiveFromClient(NetworkMessage msg, NetworkStream stream)
        {

        }

        public virtual void OnReceiveFromServer(NetworkStream stream)
        {

        }
        
        public virtual void OnSend(NetworkStream stream) { }
        
        public virtual void SetResponseType(ResponseType _responseType)
        {
            responseType = _responseType;
        }

        public virtual ResponseType GetResponseType()
        {
            return responseType;
        }
        

        
    }
}
