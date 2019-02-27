using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkBuffer
    {
        //used to index/find in NetworkBufferPool
        public int id = 0;      

        //the reserved space for our network data
        public byte[] buffer;   

        public NetworkBuffer(int _id, int bufferLen)
        {
            id = _id;
            buffer = new byte[bufferLen];
        }
    }
}
