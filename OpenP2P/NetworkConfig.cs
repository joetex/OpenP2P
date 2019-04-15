using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkConfig
    {
        public const int MAX_SEND_THREADS = 1;
        public const int MAX_RECV_THREADS = 1;
        public const int MAX_RELIABLE_THREADS = 1;

        public const int BufferPoolStartCount = 4000;
        public const int BufferMaxLength = 1420;
        public const int SocketBufferCount = 2000;
        public const int SocketSendRate = 5000;
        public const int SocketReceiveTimeout = 0;

        //important to sleep more, since they are on infinite loops
        public const int ThreadWaitingSleepTime = 1;
        public const int ThreadSendRateSleepTime = 0;
        public const int ThreadReliableSleepTime = 1;

        public const long SocketReliableRetryDelay = 300;
        public const long SocketReliableRetryAttempts = 4;
    }
}
