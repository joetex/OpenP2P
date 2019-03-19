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

        public event EventHandler<NetworkMessage> OnRequest = null;
        public event EventHandler<NetworkMessage> OnResponse = null;

        public virtual void WriteRequest(NetworkStream stream)
        {

        }

        public virtual void WriteResponse(NetworkStream stream)
        {

        }

        public virtual void ReadRequest(NetworkStream stream)
        {

        }
        public virtual void ReadResponse(NetworkStream stream)
        {

        }

        public virtual void InvokeOnRead(NetworkStream stream)
        {
            switch (responseType)
            {
                case ResponseType.Request:
                    ReadRequest(stream);
                    if (OnRequest != null)
                        OnRequest.Invoke(stream, this);
                    break;
                case ResponseType.Response:
                    ReadResponse(stream);
                    if (OnResponse != null)
                        OnResponse.Invoke(stream, this);
                    break;
            }
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
