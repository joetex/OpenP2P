using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkProtocolBase
    {
        public Dictionary<int, INetworkMessage> messages = new Dictionary<int, INetworkMessage>();

        public bool isResponse = false;
        public bool isLittleEndian = false;
        

        public NetworkProtocolBase()
        {
        }

        public virtual void WriteHeader(NetworkStream stream, byte mt, bool isResp)
        {
            
        }

        public virtual byte ReadHeader(NetworkStream stream)
        {
            return 0;
        }

        public virtual void OnReceive(NetworkStream stream)
        {
            //Message msg = stream.ReadHeader();
            //messages[msg].OnReceive(stream);
        }

        public virtual void OnSend(NetworkStream stream)
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
