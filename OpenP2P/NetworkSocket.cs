using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkSocket
    {
        public NetworkStream stream;
        public Socket rawSocket;
        public IPEndPoint remote;
        public IPEndPoint local;
        public IPEndPoint anyHost;

        public static NetworkSocketEventPool EVENTPOOL = new NetworkSocketEventPool(10, 2000);

        //track active events to this socket, so we can cleanup at any time
        public Dictionary<int, NetworkSocketEvent> activeEvents = new Dictionary<int, NetworkSocketEvent>();
        
        public bool isSending = false;
        public bool isReceiving = false;

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

        /**
         * Setup the connection credentials and socket configuration
         */
        public void Setup(string remoteHost, int remotePort, int localPort)
        {
            stream = new NetworkStream(this);
            evtSocketCompleted = new EventHandler<SocketAsyncEventArgs>(OnSocketCompleted);

            remote = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            local = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);
            anyHost = new IPEndPoint(IPAddress.Any, 0); 

            rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            rawSocket.ExclusiveAddressUse = false;
            rawSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            rawSocket.Bind(local);
        }

        /**
         * Event: OnSocketCompleted 
         * Called when an async receive/send has completed.
         */
        void OnSocketCompleted(object sender, SocketAsyncEventArgs e)
        {
            NetworkSocketEvent socketEvent = (NetworkSocketEvent)e.UserToken;

            // determine which type of operation just completed and call the associated handler
            switch (socketEvent.args.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom: OnSocketReceive(socketEvent); break;
                case SocketAsyncOperation.SendTo: OnSocketSend(socketEvent); break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /**
         * Event: OnSocketReceive
         * Called when data has been fully received from a remote connection.
         */
        void OnSocketReceive(NetworkSocketEvent socketEvent)
        {
            isReceiving = false;
            Listen(); //listen again

            int byteLength = socketEvent.args.BytesTransferred;
            if (byteLength > 0 && socketEvent.args.SocketError == SocketError.Success)
            {
                socketEvent.SetBufferLength(byteLength);
                
                if(OnReceive != null) //notify any event listeners
                    OnReceive.Invoke(this, socketEvent);
            }
            Free(socketEvent);  //release socket event
        }

        /**
         * Event: OnSocketSend
         * Called after data has been sent to remote connection.
         */
        void OnSocketSend(NetworkSocketEvent socketEvent)
        {
            isSending = false;
            Free(socketEvent);
        }
        
        /**
         * Listen for single incoming UDP packet.
         */
        public void Listen()
        {
            //if (isReceiving)
            //    return;

            isReceiving = true;
            NetworkSocketEvent socketEvent = Reserve();
            socketEvent.args.RemoteEndPoint = anyHost;
            
            if (!rawSocket.ReceiveFromAsync(socketEvent.args))
            {
                Console.WriteLine("ReceiveAsync Failed");
                OnSocketCompleted(this, socketEvent.args);
            }
        }
        
        /**
         * Prepare for sending. 
         * First, insert data into the socket event's byte buffer, which is reused from a buffer pool.
         * Then, follow up with calling ExecuteSend. 
         * Example:
         *  NetworkSocketEvent se = socket.PrepareSend();
         *  byte[] data = se.GetByteBuffer();
         *  //... add your custom byte message to the "data" byte array 
         *  socket.ExecuteSend(se);
         *  
         * This allows us to stream data directly to the pooled byte buffer and save cpu cycles.
         */
        public NetworkSocketEvent PrepareSend()
        {
            NetworkSocketEvent socketEvent = Reserve();
            socketEvent.args.RemoteEndPoint = remote;
            return socketEvent;
        }

        /**
         * Execute a send using a prepared socket event.
         */
        public void ExecuteSend(NetworkSocketEvent socketEvent)
        {
            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, socketEvent);

            if (!rawSocket.SendToAsync(socketEvent.args))
            {
                Console.WriteLine("SendToAsync Failed");
                //finished synchronously, process immediately
                OnSocketCompleted(this, socketEvent.args);
            }
        }

        /**
         * Send raw ASCII string message
         */
        public void Send(string msg)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            Send(data);
        }

        /**
         * Send raw byte data.  
         * Warning: Copies the data bytes to the socket byte buffer. 
         * This is for customized byte data outside the intended byte streaming method with Prepare/Execute.
         */
        public void Send(byte[] data)
        {
            NetworkSocketEvent socketEvent = Reserve();
            socketEvent.SetBufferBytes(data);
            socketEvent.args.RemoteEndPoint = remote;
            
            if( OnSend != null) //notify any event listeners
                OnSend.Invoke(this, socketEvent);

            if (!rawSocket.SendToAsync(socketEvent.args))
            {
                Console.WriteLine("SendToAsync Failed");
                OnSocketCompleted(this, socketEvent.args); //finished synchronously, process immediately
            }
        }
       
        /**
         * Reserve a socket event from the pool.  
         * Setup the socket, completed callback and UserToken
         */
        public NetworkSocketEvent Reserve()
        {
            
                NetworkSocketEvent socketEvent = EVENTPOOL.Reserve();
                socketEvent.socket = this;
                socketEvent.args.AcceptSocket = rawSocket;
                socketEvent.args.Completed += evtSocketCompleted;
                socketEvent.args.UserToken = socketEvent;
                socketEvent.InitStream();
            //lock (activeEvents)
            {
               // activeEvents.Add(socketEvent.id, socketEvent);
            }
            return socketEvent;
            
            
        }

        /**
         * Free a socket event back into the pool.
         * Reset all the initial properties to null.
         */
        public void Free(NetworkSocketEvent socketEvent)
        {
            //lock (activeEvents)
            {
               // activeEvents.Remove(socketEvent.id);
            }
            socketEvent.socket = null;
                socketEvent.args.AcceptSocket = null;
                socketEvent.args.RemoteEndPoint = null;
                socketEvent.args.UserToken = null;
                socketEvent.args.Completed -= evtSocketCompleted;

                EVENTPOOL.Free(socketEvent);
            
            
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
                
                rawSocket.Shutdown(SocketShutdown.Both);
                rawSocket.Disconnect(false);
                rawSocket.Close();
                rawSocket.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
