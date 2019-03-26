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
        public const int MIN_POOL_COUNT = 10000;
        public const int BUFFER_LENGTH = 4000;
        public const int MAX_BUFFER_PACKET_COUNT = 1000;
        public int MAX_SENDRATE_PERFRAME = 5000;
        public const int RECEIVE_TIMEOUT = 1000;

        public int MAX_SEND_THREADS = 0;
        public int MAX_RECV_THREADS = 0;
        public int MAX_RELIABLE_THREADS = 0;
        //important to sleep more, since they are on infinite loops
        public const int EMPTY_SLEEP_TIME = 10;
        public const int MAXSEND_SLEEP_TIME = 0;

        public const int MIN_RELIABLE_SLEEP_TIME = 100;
        public const long RETRY_TIME = 1000;
        public const long RETRY_COUNT = 10;

        public const long MAX_WAIT_TIME = 1000;

        public NetworkStreamPool STREAMPOOL = new NetworkStreamPool(MIN_POOL_COUNT, BUFFER_LENGTH);

        public Queue<NetworkStream> SENDQUEUE = new Queue<NetworkStream>(MIN_POOL_COUNT);
        public Queue<NetworkStream> RECVQUEUE = new Queue<NetworkStream>(MIN_POOL_COUNT);
        public Queue<NetworkStream> RELIABLEQUEUE = new Queue<NetworkStream>(MIN_POOL_COUNT);
        public Dictionary<ulong, NetworkStream> ACKNOWLEDGED = new Dictionary<ulong, NetworkStream>();

        public List<Thread> SENDTHREADS = new List<Thread>();
        public List<Thread> RECVTHREADS = new List<Thread>();
        public List<Thread> RELIABLETHREADS = new List<Thread>();

        public void StartNetworkThreads(int sendThreads, int recvThreads, int reliableThreads)
        {
            MAX_SEND_THREADS = sendThreads;
            MAX_RECV_THREADS = recvThreads;
            MAX_RELIABLE_THREADS = reliableThreads;

            for (int i = 0; i < sendThreads; i++)
            {
                SENDTHREADS.Add(new Thread(SendThread));
                SENDTHREADS[i].Start();
            }
            for (int i = 0; i < recvThreads; i++)
            {
                RECVTHREADS.Add(new Thread(RecvThread));
                RECVTHREADS[i].Start();
            }

            for (int i = 0; i < reliableThreads; i++)
            {
                RELIABLETHREADS.Add(new Thread(ReliableThread));
                RELIABLETHREADS[i].Start();
            }
        }

        public void SendThread()
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
                    Thread.Sleep(EMPTY_SLEEP_TIME);
                    continue;
                }

                //avoid filling up the OS socket buffer or network card's RING buffer
                //important on cloud VPS/dedicated servers with lower buffer settings
                //google compute engine (cheapest one) gave packet loss at >75 sends/frame
                if (queueCount % MAX_SENDRATE_PERFRAME == 0)
                    Thread.Sleep(MAXSEND_SLEEP_TIME);

                
                stream.socket.SendInternal(stream);
            }
        }

        public NetworkStream recvStream = null;
        public void RecvThread()
        {
            NetworkStream stream = null;
            int queueCount = 0;

            while (true)
            {
                lock (RECVQUEUE)
                {
                    queueCount = RECVQUEUE.Count;
                    if (queueCount > 0)
                        stream = RECVQUEUE.Dequeue();
                }

                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0)
                {
                    Thread.Sleep(EMPTY_SLEEP_TIME);
                    continue;
                }
                
                stream.socket.ExecuteListen(stream);
            }
        }

        

        public void ReliableThread()
        {

            NetworkStream stream = null;
            int queueCount = 0;
            Queue<NetworkStream> tempQueue = new Queue<NetworkStream>(MIN_POOL_COUNT);
            long curtime = 0;
            long difftime = 0;

            while (true)
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
                    Thread.Sleep(EMPTY_SLEEP_TIME);
                    continue;
                }
                
                

                bool hasKey = false;
                lock (ACKNOWLEDGED)
                {
                    hasKey = ACKNOWLEDGED.ContainsKey(stream.ackkey);
                    
                    if (hasKey)
                    {
                        Console.WriteLine("Acknowledged: " + stream.ackkey);
                        ACKNOWLEDGED.Remove(stream.ackkey);
                    }
                }

                if( hasKey)
                {
                    stream.socket.Free(stream);
                    continue;
                }

                curtime = NetworkTime.Milliseconds();
                difftime = curtime - stream.sentTime;
                if (difftime > RETRY_TIME)
                {
                    
                    Console.WriteLine("Reliable message retry #" + stream.ackkey);

                    if ( stream.acknowledged || stream.retryCount >= NetworkThread.RETRY_COUNT)
                    {
                        Console.WriteLine("Retry count reached: " + stream.retryCount);
                        stream.socket.Free(stream);
                    }
                    else
                    {
                        stream.socket.Send(stream);
                    }
                }
                else
                {
                    lock (RELIABLEQUEUE)
                    {
                        Console.WriteLine("Waiting: " + stream.ackkey);
                        RELIABLEQUEUE.Enqueue(stream);
                    }
                }
                    


                Thread.Sleep(MIN_RELIABLE_SLEEP_TIME);
            }
        }
    }
}
