using System;
using System.Collections.Concurrent;
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

        

        public static NetworkStreamPool STREAMPOOL = new NetworkStreamPool(NetworkConfig.BufferPoolStartCount, NetworkConfig.BufferMaxLength);

        public static Queue<NetworkStream> SENDQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        //public static ConcurrentQueue<NetworkStream> SENDQUEUE = new ConcurrentQueue<NetworkStream>();
        //public static Queue<NetworkStream> RECVQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        public static List<NetworkStream> RECVQUEUE = new List<NetworkStream>(NetworkConfig.BufferPoolStartCount);

        //public static ConcurrentQueue<NetworkStream> RELIABLEQUEUE = new ConcurrentQueue<NetworkStream>();
        public static Queue<NetworkStream> RELIABLEQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        //public static ConcurrentDictionary<ulong, NetworkStream> ACKNOWLEDGED = new ConcurrentDictionary<ulong, NetworkStream>();
        public static Dictionary<ulong, NetworkStream> ACKNOWLEDGED = new Dictionary<ulong, NetworkStream>();

        public static Thread mainThread = new Thread(MainThread);
        public static List<Thread> SENDTHREADS = new List<Thread>();
        public static List<Thread> RECVTHREADS = new List<Thread>();
        public static List<Thread> RELIABLETHREADS = new List<Thread>();

        //public static NetworkThread(NetworkProtocol p)
        //{
        //    protocol = p;
        //}

        public static void StartNetworkThreads()
        {

            for (int i = 0; i < NetworkConfig.MAX_SEND_THREADS; i++)
            {
                SENDTHREADS.Add(new Thread(SendThread));
                SENDTHREADS[i].Start();
            }
            for (int i = 0; i < NetworkConfig.MAX_RECV_THREADS; i++)
            {
                //Thread t = new Thread(RecvThread);
                //t.Priority = ThreadPriority.Highest;
                //RECVTHREADS.Add(t);
                //RECVTHREADS[i].Start();
            }

            for (int i = 0; i < NetworkConfig.MAX_RELIABLE_THREADS; i++)
            {
               // RELIABLETHREADS.Add(new Thread(ReliableThread));
                //RELIABLETHREADS[i].Start();
            }

            //mainThread.Start();
        }

        public static void MainThread()
        {
            NetworkStream stream = null;
            while(true)
            {

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

                    if (queueCount == 0)
                    {
                        if (ReliableThread() > 0)
                            continue;

                        Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                        continue;
                    }

                    stream = SENDQUEUE.Dequeue();
                }
                
                stream.socket.SendInternal(stream);
            }
        }

        public static void BeginRecvThread(NetworkStream stream)
        {
            Thread t = new Thread(NetworkThread.RecvThread);
            t.Priority = ThreadPriority.Highest;
            RECVTHREADS.Add(t);
            RECVTHREADS[RECVTHREADS.Count-1].Start(stream);
        }

        public static NetworkStream recvStream = null;
        public static int recvId = 0;
        public static void RecvThread(object ostream)
        {
            //NetworkSocket.NetworkIPType ipType = (NetworkSocket.NetworkIPType)data;
            NetworkStream stream = (NetworkStream)ostream;
            int queueCount = 0;
            int currentId = 0;

            while (true)
            {
                NetworkConfig.ProfileBegin("LISTEN");
                stream.socket.ExecuteListen(stream);
                NetworkConfig.ProfileEnd("LISTEN");
            }
        }

        

        public static int ReliableThread()
        {

            NetworkStream stream = null;
            NetworkStream acknowledgeStream = null;
            int queueCount = 0;
            //Queue<NetworkStream> tempQueue = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
            long curtime = 0;
            long difftime = 0;

            //while (true)
            {
                lock (RELIABLEQUEUE)
                {
                    queueCount = RELIABLEQUEUE.Count;
                    //sleep if empty, to avoid 100% cpu
                    if (queueCount == 0)
                    {
                        //Thread.Sleep(EMPTY_SLEEP_TIME);
                        return queueCount;
                    }

                    if ( ACKNOWLEDGED.Remove(stream.ackkey))
                    {
                        stream.socket.Free(stream);
                    }

                    
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
