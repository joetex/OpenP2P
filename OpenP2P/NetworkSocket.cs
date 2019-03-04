using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkSocket
    {
        public Socket socket;
        public IPEndPoint remote;
        public IPEndPoint local;
        public IPEndPoint anyHost;

        public static NetworkSocketEventPool EVENTPOOL = new NetworkSocketEventPool(100000, 2000);

        public static Thread SENDTHREAD = new Thread(SendThread);
        public static Thread RECVTHREAD = new Thread(ListenThread);
        public static Queue<NetworkSocketEvent> SENDQUEUE = new Queue<NetworkSocketEvent>();
        public static Queue<NetworkSocketEvent> RECVQUEUE = new Queue<NetworkSocketEvent>();

        //track active events to this socket, so we can cleanup at any time
        public Dictionary<int, NetworkSocketEvent> activeEvents = new Dictionary<int, NetworkSocketEvent>();
        

        public EventHandler<SocketAsyncEventArgs> evtSocketCompleted = null;
        public event EventHandler<NetworkSocketEvent> OnReceive;
        public event EventHandler<NetworkSocketEvent> OnSend;

        public NetworkSocket(string remoteHost, int remotePort, int localPort)
        {
            Setup(remoteHost, remotePort, localPort);
        }

        public NetworkSocket(string remoteHost, int remotePort)
        {
            Setup(remoteHost, remotePort, 0);
        }

        public NetworkSocket(int localPort)
        {
            Setup("127.0.0.1", 0, localPort);
        }
        
        static void SendThread()
        {
            NetworkSocketEvent se = null;
            bool hasItems = false;
            while (true)
            {
                hasItems = true;
                lock(SENDQUEUE)
                {
                    if (SENDQUEUE.Count == 0)
                    {
                        hasItems = false;
                    }
                    else
                    {
                        se = SENDQUEUE.Dequeue();
                    }
                }
                if( !hasItems)
                {
                    Thread.Sleep(1);
                    continue;
                }
                se.socket.ExecuteSend(se);
            }
        }

        static void ListenThread()
        {
            NetworkSocketEvent se = null;
            bool hasItems = false;
            while (true)
            {
                hasItems = true;
                lock (RECVQUEUE)
                {
                    if (RECVQUEUE.Count == 0)
                    {
                        hasItems = false;
                    }
                    else
                    {
                        se = RECVQUEUE.Dequeue();
                    }
                }
                if (!hasItems)
                {
                    Thread.Sleep(1);
                    continue;
                }
                se.socket.ExecuteListen(se);
            }
        }

        /**
         * Setup the connection credentials and socket configuration
         */
        public void Setup(string remoteHost, int remotePort, int localPort)
        {
            if( !SENDTHREAD.IsAlive )
            {
                SENDTHREAD.Start();
            }
            if (!RECVTHREAD.IsAlive)
            {
                RECVTHREAD.Start();
            }
            //stream = new NetworkStream(this);
            evtSocketCompleted = new EventHandler<SocketAsyncEventArgs>(OnSocketCompleted);

            remote = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            local = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);
            anyHost = new IPEndPoint(IPAddress.Any, 0); 

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ExclusiveAddressUse = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(local);
        }

        /**
         * Event: OnSocketCompleted 
         * Called when an async receive/send has completed.
         */
        void OnSocketCompleted(object sender, SocketAsyncEventArgs e)
        {
            NetworkSocketEvent se = (NetworkSocketEvent)e.UserToken;

            // determine which type of operation just completed and call the associated handler
            switch (se.args.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom: OnSocketReceive(se); break;
                case SocketAsyncOperation.SendTo: OnSocketSend(se); break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /**
         * Event: OnSocketReceive
         * Called when data has been fully received from a remote connection.
         */
        void OnSocketReceive(NetworkSocketEvent se)
        {
            //int byteLength = se.args.BytesTransferred;
            //if (byteLength > 0 && se.args.SocketError == SocketError.Success)
            {
               //se.SetBufferLength(byteLength);

                if (OnReceive != null) //notify any event listeners
                    OnReceive.Invoke(this, se);
            }

            Listen(se); //listen again
            //Free(se);  //release socket event
        }

        /**
         * Event: OnSocketSend
         * Called after data has been sent to remote connection.
         */
        void OnSocketSend(NetworkSocketEvent se)
        {
            Free(se);
        }
        
        /**
         * Listen for single incoming UDP packet.
         */
        public void Listen(NetworkSocketEvent se)
        {
            if( se == null )
                se = Reserve();
            se.args.RemoteEndPoint = anyHost;


            lock (RECVQUEUE)
            {
                RECVQUEUE.Enqueue(se);
            }
            /*
            if (!socket.ReceiveFromAsync(se.args))
            {
                Console.WriteLine("ReceiveAsync Failed");
                OnSocketCompleted(this, se.args);
            }*/
        }

        public void ExecuteListen(NetworkSocketEvent se)
        {
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            int bytesReceived = socket.ReceiveFrom(se.stream.ByteBuffer, ref remoteEP);
            if( bytesReceived == 0 )
            {
                Console.WriteLine("Error Receiving bytes");
            }
            se.SetBufferLength(bytesReceived);
            OnSocketReceive(se);
        }

        /**
         * Begin Send
         * Starts the NetworkStream for writing data to byte buffer.
         */
        public NetworkStream BeginSend()
        {
            NetworkSocketEvent se = Reserve();
            se.args.RemoteEndPoint = remote;
            se.stream.BeginWrite();

            return se.stream;
        }

        /**
         * End Send
         * Ends the NetworkStream and sends to remote destination
         */
        public void EndSend(NetworkStream stream)
        {
            lock(SENDQUEUE)
            {
                SENDQUEUE.Enqueue(stream.socketEvent);
            }
            /*
            NetworkSocketEvent se = stream.socketEvent;
            stream.EndWrite();

            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, se);

            if (!socket.SendToAsync(se.args))
            {
                Console.WriteLine("SendToAsync Failed");
                //finished synchronously, process immediately
                OnSocketCompleted(this, se.args);
            }
            */
        }

        public void ExecuteSend(NetworkSocketEvent se)
        {
            se.stream.EndWrite();

            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, se);
           
            int result = socket.SendTo(se.stream.ByteBuffer, se.stream.byteLength, SocketFlags.None, se.args.RemoteEndPoint);

            if (result != 12)
                Console.WriteLine("Error sending bytes");
            OnSocketSend(se);

            /*if (!socket.SendToAsync(se.args))
            {
                Console.WriteLine("SendToAsync Failed");
                //finished synchronously, process immediately
                OnSocketCompleted(this, se.args);
            }*/
        }
        
        /**
         * Reserve a socket event from the pool.  
         * Setup the socket, completed callback and UserToken
         */
        public NetworkSocketEvent Reserve()
        {
            NetworkSocketEvent se = EVENTPOOL.Reserve();
            se.args.AcceptSocket = socket;
            se.args.Completed += evtSocketCompleted;
            se.args.UserToken = se;
            se.socket = this;
            //lock (activeEvents)
            {
               // activeEvents.Add(socketEvent.id, socketEvent);
            }
            return se;
        }

        /**
         * Free a socket event back into the pool.
         * Reset all the initial properties to null.
         */
        public void Free(NetworkSocketEvent se)
        {
            //lock (activeEvents)
            {
               // activeEvents.Remove(socketEvent.id);
            }
            
            se.args.AcceptSocket = null;
            se.args.RemoteEndPoint = null;
            se.args.UserToken = null;
            se.args.Completed -= evtSocketCompleted;
            se.socket = null;

            EVENTPOOL.Free(se);
        }


        /**
         * Clean any open socket events and shutdown socket.
         */
        public void Dispose()
        {
            try
            {
                foreach (KeyValuePair<int, NetworkSocketEvent> entry in activeEvents)
                {
                    Free(entry.Value);
                }
                
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
                socket.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
