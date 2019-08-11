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

        

        public NetworkPacketPool PACKETPOOL = new NetworkPacketPool(NetworkConfig.BufferPoolStartCount, NetworkConfig.BufferMaxLength);

        public Queue<NetworkPacket> SENDQUEUE = new Queue<NetworkPacket>(NetworkConfig.BufferPoolStartCount);
        //public static ConcurrentQueue<NetworkPacket> SENDQUEUE = new ConcurrentQueue<NetworkPacket>();
        public Queue<NetworkPacket> RECVQUEUE = new Queue<NetworkPacket>(NetworkConfig.BufferPoolStartCount);
        //public List<NetworkPacket> RECVQUEUE = new List<NetworkPacket>(NetworkConfig.BufferPoolStartCount);

        //public static ConcurrentQueue<NetworkPacket> RELIABLEQUEUE = new ConcurrentQueue<NetworkPacket>();
        public Queue<NetworkPacket> RELIABLEQUEUE = new Queue<NetworkPacket>(NetworkConfig.BufferPoolStartCount);

        //public static ConcurrentDictionary<ulong, NetworkPacket> ACKNOWLEDGED = new ConcurrentDictionary<ulong, NetworkPacket>();
        public Dictionary<uint, NetworkPacket> ACKNOWLEDGED = new Dictionary<uint, NetworkPacket>();

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
                //Thread t = new Thread(RecvProcessThread);
                //t.Priority = ThreadPriority.Highest;
                //RECVTHREADS.Add(t);
                //RECVTHREADS[i].Start();
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
            //NetworkPacket packet = null;
            while(true)
            {

            }
        }

        public void SendThread()
        {
            NetworkPacket packet;
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
                    Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                    continue;
                }

                //for(int i=0; i<queueCount; i++)
                {
                    lock (SENDQUEUE)
                    {
                        packet = SENDQUEUE.Dequeue();
                    }

                    packet.socket.SendFromThread(packet);
                }
                
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
        public void BeginRecvThread(NetworkPacket packet)
        {
            Thread t = new Thread(RecvThread);
            t.Priority = ThreadPriority.Highest;
            RECVTHREADS.Add(t);
            RECVTHREADS[RECVTHREADS.Count-1].Start(packet);
        }

        public NetworkPacket recvPacket = null;
        public int recvId = 0;
        public void RecvThread(object opacket)
        {
            NetworkPacket packet = (NetworkPacket)opacket;
         
            while (true)
            {
                //NetworkConfig.ProfileBegin("LISTEN");
                packet.socket.ExecuteListen(packet);
                //NetworkConfig.ProfileEnd("LISTEN");

                packet.socket.InvokeOnRecieve(packet);
                /*
                lock (RECVQUEUE)
                {
                    RECVQUEUE.Enqueue(packet);
                }
                
                packet = packet.socket.Reserve();
                packet.Reset();*/
                //Thread.Sleep(0);
            }
        }

        //Turns out this is slow...
        public void RecvProcessThread()
        {
            int queueCount;
            NetworkPacket packet;

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
                    packet = RECVQUEUE.Dequeue();
                }
                packet.socket.InvokeOnRecieve(packet);

                packet.socket.Free(packet);
                //Thread.Sleep(0);
            }
            
        }

        public int failedReliableCount = 0;

        public void ReliableThread()
        {

            NetworkPacket packet;
            //NetworkPacket acknowledgePacket = null;
            int queueCount;
            //Queue<NetworkPacket> tempQueue = new Queue<NetworkPacket>(NetworkConfig.BufferPoolStartCount);
            long curtime;
            long difftime;
            bool isAcknowledged;
            //while (true)
            {
                //lock (RELIABLEQUEUE)
                {
                    queueCount = RELIABLEQUEUE.Count;
                }

                if (queueCount == 0)
                {
                    return;
                }

                //for (int i = 0; i < queueCount; i++)
                {
                    //lock (RELIABLEQUEUE)
                    {
                        packet = RELIABLEQUEUE.Dequeue();
                    }
                        
                
                    lock (ACKNOWLEDGED)
                    {
                        isAcknowledged = ACKNOWLEDGED.Remove(packet.ackkey);
                    }

                    if (isAcknowledged)
                    {
                        packet.socket.Free(packet);
                        return;//return queueCount;
                    }
                    curtime = NetworkTime.Milliseconds();
                    difftime = curtime - packet.sentTime;
                    if (difftime > NetworkConfig.SocketReliableRetryDelay)
                    {
                        //Console.WriteLine("Reliable message retry #" + packet.ackkey);

                        if ( packet.retryCount >= NetworkConfig.SocketReliableRetryAttempts)
                        {
                            //Console.WriteLine("Retry count reached: " + packet.retryCount);

                            if( packet.header.messageChannel == MessageChannel.ConnectToServer )
                            {
                                packet.socket.Failed(NetworkErrorType.ErrorConnectToServer, "Unable to connect to server.", packet);
                            }

                            failedReliableCount++;
                            packet.socket.Failed(NetworkErrorType.ErrorReliableFailed, "Failed to deliver " + packet.retryCount + " packets ("+failedReliableCount+") times.", packet);
                            
                            packet.socket.Free(packet);
                            return;//return queueCount;
                        }
                    
                        packet.socket.Send(packet);
                        return;//return queueCount;
                    }

                    //lock (RELIABLEQUEUE)
                    {
                        //Console.WriteLine("Waiting: " + packet.ackkey);
                        RELIABLEQUEUE.Enqueue(packet);
                    }
                }
                //Thread.Sleep(MIN_RELIABLE_SLEEP_TIME);
                return;//return queueCount;
            }
        }
    }
}
