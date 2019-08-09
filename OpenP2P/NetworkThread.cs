using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkThread
    {
        public NetworkProtocol protocol = null;

        

        public NetworkStreamPool STREAMPOOL = new NetworkStreamPool(NetworkConfig.BufferPoolStartCount, NetworkConfig.BufferMaxLength);

        public Queue<NetworkStream> SENDQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        //public static ConcurrentQueue<NetworkStream> SENDQUEUE = new ConcurrentQueue<NetworkStream>();
        public Queue<NetworkStream> RECVQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        //public List<NetworkStream> RECVQUEUE = new List<NetworkStream>(NetworkConfig.BufferPoolStartCount);

        //public static ConcurrentQueue<NetworkStream> RELIABLEQUEUE = new ConcurrentQueue<NetworkStream>();
        public Queue<NetworkStream> RELIABLEQUEUE = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
        //public static ConcurrentDictionary<ulong, NetworkStream> ACKNOWLEDGED = new ConcurrentDictionary<ulong, NetworkStream>();
        public Dictionary<ulong, NetworkStream> ACKNOWLEDGED = new Dictionary<ulong, NetworkStream>();

        //public Thread mainThread = new Thread(MainThread);
        public List<Thread> SENDTHREADS = new List<Thread>();
        public List<Thread> RECVTHREADS = new List<Thread>();
        public List<Thread> RELIABLETHREADS = new List<Thread>();


        //public static NetworkThread(NetworkProtocol p)
        //{
        //    protocol = p;
        //}

        public void StartNetworkThreads()
        {
            
            
            for (int i = 0; i < NetworkConfig.MAX_SEND_THREADS; i++)
            {
                Thread t = new Thread(SendThread);
                SENDTHREADS.Add(t);
                SENDTHREADS[i].Start();
            }
            for (int i = 0; i < NetworkConfig.MAX_RECV_THREADS; i++)
            {
                Thread t = new Thread(RecvProcessThread);
                t.Priority = ThreadPriority.Highest;
                RECVTHREADS.Add(t);
                RECVTHREADS[i].Start();
            }

            for (int i = 0; i < NetworkConfig.MAX_RELIABLE_THREADS; i++)
            {
                //RELIABLETHREADS.Add(new Thread(ReliableThread));
                //RELIABLETHREADS[i].Start();
            }

            

            //mainThread.Start();
        }

        public void MainThread()
        {
            NetworkStream stream = null;
            while(true)
            {

            }
        }

        public void SendThread()
        {
            NetworkStream stream;
            int queueCount;

            while (true)
            {
                ReliableThread();

                lock (SENDQUEUE)
                {
                    queueCount = SENDQUEUE.Count;
                }

                if (queueCount == 0)
                {
                    //Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                    continue;
                }

                //for(int i=0; i<queueCount; i++)
                {
                    lock (SENDQUEUE)
                    {
                        stream = SENDQUEUE.Dequeue();
                    }

                    stream.socket.SendInternal(stream);
                }
                //Thread.Sleep(0);
            }
        }

        public void UpdatePriority()
        {
            Process p = Process.GetCurrentProcess();
            foreach (ProcessThread pt in p.Threads)
            {
                pt.IdealProcessor = 0;
                pt.ProcessorAffinity = (IntPtr)1;
            }
        }
        public void BeginRecvThread(NetworkStream stream)
        {
            Thread t = new Thread(RecvThread);
            t.Priority = ThreadPriority.Highest;
            RECVTHREADS.Add(t);
            RECVTHREADS[RECVTHREADS.Count-1].Start(stream);
        }

        public NetworkStream recvStream = null;
        public int recvId = 0;
        public void RecvThread(object ostream)
        {
            NetworkStream stream = (NetworkStream)ostream;
         
            while (true)
            {
                //NetworkConfig.ProfileBegin("LISTEN");
                stream.socket.ExecuteListen(stream);
                //NetworkConfig.ProfileEnd("LISTEN");

               // stream.socket.InvokeOnRecieve(stream);
                
                lock (RECVQUEUE)
                {
                    RECVQUEUE.Enqueue(stream);
                }
                
                stream = stream.socket.Reserve();
                stream.Reset();
                //Thread.Sleep(0);
            }
        }

        //Turns out this is slow...
        public void RecvProcessThread()
        {
            int queueCount;
            NetworkStream stream;

            while(true)
            {
                lock (RECVQUEUE)
                {
                    queueCount = RECVQUEUE.Count;
                }

                if (queueCount == 0)
                {
                    //Thread.Sleep(NetworkConfig.ThreadRecvProcessSleepTime);
                    continue;
                }
                lock (RECVQUEUE)
                {
                    stream = RECVQUEUE.Dequeue();
                }
                stream.socket.InvokeOnRecieve(stream);

                stream.socket.Free(stream);
                //Thread.Sleep(0);
            }
            
        }

        public void ReliableThread()
        {

            NetworkStream stream;
            //NetworkStream acknowledgeStream = null;
            int queueCount;
            //Queue<NetworkStream> tempQueue = new Queue<NetworkStream>(NetworkConfig.BufferPoolStartCount);
            long curtime;
            long difftime;
            bool isAcknowledged;
            //while (true)
            {
                lock (RELIABLEQUEUE)
                {
                    queueCount = RELIABLEQUEUE.Count;
                }

                if (queueCount == 0)
                {
                    return;
                }

                //for (int i = 0; i < queueCount; i++)
                {
                    lock (RELIABLEQUEUE)
                    {
                        stream = RELIABLEQUEUE.Dequeue();
                    }
                        
                
                    lock (ACKNOWLEDGED)
                    {
                        isAcknowledged = ACKNOWLEDGED.Remove(stream.ackkey);
                    }

                    if (isAcknowledged)
                    {
                        stream.socket.Free(stream);
                        return;//return queueCount;
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
                            return;//return queueCount;
                        }
                    
                        stream.socket.Send(stream);
                        return;//return queueCount;
                    }

                    lock (RELIABLEQUEUE)
                    {
                        //Console.WriteLine("Waiting: " + stream.ackkey);
                        RELIABLEQUEUE.Enqueue(stream);
                    }
                }
                //Thread.Sleep(MIN_RELIABLE_SLEEP_TIME);
                return;//return queueCount;
            }
        }
    }
}
