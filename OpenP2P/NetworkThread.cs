using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkThread
    {
        public NetworkProtocol protocol = null;

        public static int MAX_SEND_THREADS = 1;
        public static int MAX_RECV_THREADS = 2;
        public static int MAX_RELIABLE_THREADS = 1;

        public static NetworkStreamPool STREAMPOOL = new NetworkStreamPool(NetworkConfig.BufferPoolStartCount, NetworkConfig.BufferMaxLength);

        public static Queue<NetworkStream> SENDQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        //public static Queue<NetworkStream> RECVQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        public static List<NetworkStream> RECVQUEUE = new List<NetworkStream>(NetworkConfig.BufferPoolStartCount);

        public static Queue<NetworkStream> RELIABLEQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        public static Dictionary<ulong, NetworkStream> ACKNOWLEDGED = new Dictionary<ulong, NetworkStream>();

        public static List<Thread> SENDTHREADS = new List<Thread>();
        public static List<Thread> RECVTHREADS = new List<Thread>();
        public static List<Thread> RELIABLETHREADS = new List<Thread>();

        //public static NetworkThread(NetworkProtocol p)
        //{
        //    protocol = p;
        //}

        public static void StartNetworkThreads()
        {

            for (int i = 0; i < MAX_SEND_THREADS; i++)
            {
                SENDTHREADS.Add(new Thread(SendThread));
                SENDTHREADS[i].Start();
            }
            for (int i = 0; i < MAX_RECV_THREADS; i++)
            {
                RECVTHREADS.Add(new Thread(RecvThread));
                RECVTHREADS[i].Start();
            }

            for (int i = 0; i < MAX_RELIABLE_THREADS; i++)
            {
               // RELIABLETHREADS.Add(new Thread(ReliableThread));
                //RELIABLETHREADS[i].Start();
            }
        }

        public static void SendThread()
        {
            NetworkStream stream = null;
            int queueCount = 0;

            while (true)
            {
                lock (SENDQUEUE)
                {
                    queueCount = SENDQUEUE.Count;
                    if (queueCount > 0)
                        stream = SENDQUEUE.Dequeue();
                }

                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0)
                {
                    if (ReliableThread() > 0)
                        continue;

                    Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                    continue;
                }

                //avoid filling up the OS socket buffer or network card's RING buffer
                if (queueCount % NetworkConfig.SocketSendRate == 0)
                    Thread.Sleep(NetworkConfig.ThreadSendRateSleepTime);

                
                stream.socket.SendInternal(stream);
            }
        }

        public static NetworkStream recvStream = null;
        public static void RecvThread()
        {
            //NetworkSocket.NetworkIPType ipType = (NetworkSocket.NetworkIPType)data;
            NetworkStream stream = null;
            int queueCount = 0;

            while (true)
            {
                /*lock (RECVQUEUE)
                {
                    queueCount = RECVQUEUE.Count;
                    if (queueCount > 0)
                        stream = RECVQUEUE.Dequeue();
                }

                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0)
                {
                    Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                    continue;
                }
                //if( stream == null )
                //    stream = protocol.socket.Reserve();
                //stream.networkIPType = ipType;
                stream.socket.ExecuteListen(stream);
                */
                
                lock(RECVQUEUE)
                {
                    for (int i = 0; i < RECVQUEUE.Count; i++)
                    {
                        stream = RECVQUEUE[i];
                        stream.socket.ExecuteListen(stream);
                    }
                }
                
            }
        }

        

        public static int ReliableThread()
        {

            NetworkStream stream = null;
            int queueCount = 0;
            Queue<NetworkStream> tempQueue = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
            long curtime = 0;
            long difftime = 0;

            //while (true)
            {
                lock (RELIABLEQUEUE)
                {
                    queueCount = RELIABLEQUEUE.Count;
                    if (queueCount > 0)
                        stream = RELIABLEQUEUE.Dequeue();
                }

                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0)
                {
                    //Thread.Sleep(EMPTY_SLEEP_TIME);
                    return queueCount;
                }
                
                bool hasKey = false;
                lock (ACKNOWLEDGED)
                {
                    hasKey = ACKNOWLEDGED.ContainsKey(stream.ackkey);
                    if (hasKey)
                    {
                        //Console.WriteLine("Acknowledged: " + stream.ackkey);
                        ACKNOWLEDGED.Remove(stream.ackkey);
                    }
                }

                if (hasKey)
                {
                    stream.socket.Free(stream);
                    return queueCount;
                }

                curtime = NetworkTime.Milliseconds();
                difftime = curtime - stream.sentTime;
                if (difftime > NetworkConfig.SocketReliableRetryDelay)
                {
                    Console.WriteLine("Reliable message retry #" + stream.ackkey);

                    if ( stream.retryCount >= NetworkConfig.SocketReliableRetryAttempts)
                    {
                        Console.WriteLine("Retry count reached: " + stream.retryCount);

                        if( stream.header.messageType == MessageType.ConnectToServer )
                        {
                            stream.socket.Failed(NetworkErrorType.ErrorConnectToServer, stream);
                        }
                        stream.socket.Failed(NetworkErrorType.ErrorReliableFailed, stream);

                        stream.socket.Free(stream);
                        return queueCount;
                    }
                    
                    
                    stream.socket.Send(stream);
                    return queueCount;
                }

                lock (RELIABLEQUEUE)
                {
                    //Console.WriteLine("Waiting: " + stream.ackkey);
                    RELIABLEQUEUE.Enqueue(stream);
                }

                //Thread.Sleep(MIN_RELIABLE_SLEEP_TIME);
                return queueCount;
            }
        }
    }
}
