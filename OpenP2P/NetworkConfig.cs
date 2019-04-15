using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkConfig
    {
        public const int BufferPoolStartCount = 100;
        public const int BufferMaxLength = 1400;
        public const int SocketBufferCount = 1000;
        public const int SocketSendRate = 5000;
        public const int SocketReceiveTimeout = 1000;

        //important to sleep more, since they are on infinite loops
        public const int ThreadWaitingSleepTime = 1;
        public const int ThreadSendRateSleepTime = 0;
        public const int ThreadReliableSleepTime = 1;

        public const long SocketReliableRetryDelay = 700;
        public const long SocketReliableRetryAttempts = 15;
    }
}
