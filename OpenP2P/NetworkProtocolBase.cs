using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkProtocolBase
    {
        public Dictionary<int, NetworkMessage> messages = new Dictionary<int, NetworkMessage>();
        public Dictionary<int, NetworkMessage> awaitingResponse = new Dictionary<int, NetworkMessage>();

        public NetworkSocket socket = null;

        

        public int responseType = 0;
        public bool isLittleEndian = false;
        
        public NetworkProtocolBase()
        {
        }

        public virtual void AttachListener(NetworkSocket socket)
        {

        }

        public virtual NetworkMessage GetMessage(int id)
        {
            return null;
        }

        public virtual void WriteHeader(NetworkStream stream, int mt, int _responseType)
        {
            
        }

        public virtual byte ReadHeader(NetworkStream stream)
        {
            return 0;
        }

        public virtual void OnReceive(object sender, NetworkStream stream)
        {
            //Message msg = stream.ReadHeader();
            //messages[msg].OnReceive(stream);
        }

        public virtual void OnSend(object sender, NetworkStream stream)
        {
            //messages[msg].OnReceive(stream);
        }

        public virtual void OnReceiveMessage(int msg, NetworkStream stream)
        {
            //Message msg = stream.ReadHeader();
            //messages[msg].OnReceiveMessage(stream);
        }

    }
}
