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
        
        //track active events to this socket, so we can cleanup at any time
        public Dictionary<int, NetworkStream> activeEvents = new Dictionary<int, NetworkStream>();
        
        public event EventHandler<NetworkStream> OnReceive;
        public event EventHandler<NetworkStream> OnSend;
        public EventHandler<SocketAsyncEventArgs> evtSocketCompleted = null;

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
            //evtSocketCompleted = new EventHandler<SocketAsyncEventArgs>(OnSocketCompleted);

            remote = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            local = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);
            anyHost = new IPEndPoint(IPAddress.Any, 0); 

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ExclusiveAddressUse = false;
            //socket.NoDelay = true;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, NetworkThread.MAX_BUFFER_SIZE * 10);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, NetworkThread.MAX_BUFFER_SIZE * 10);
            if ( localPort != 0 )
                socket.Bind(local);
        }
        
        
        /**
         * Request a Listen on the RecvThread
         */
        public void Listen(NetworkStream stream)
        {
            if(stream == null )
                stream = Reserve();
            
            stream.Reset();
            
            //ExecuteListen(stream);
            lock (NetworkThread.RECVQUEUE)
            {
                NetworkThread.RECVQUEUE.Enqueue(stream);
            }
        }

        /**
         * Execute Listen
         * Enters from RecvThread, and begins listening for packets.
         */
        public void ExecuteListen(NetworkStream stream)
        {
            

            try
            {
                /*if (!socket.ReceiveFromAsync(stream.args))
                {
                    Console.WriteLine("ReceiveAsync Failed");
                    OnSocketCompleted(this, stream.args);
                }*/
                
                int bytesReceived = socket.ReceiveFrom(stream.ByteBuffer, ref stream.remoteEndPoint);
                stream.SetBufferLength(bytesReceived);
            }
            catch(Exception e) {
                Console.WriteLine(e.ToString());
            }

            
            OnSocketReceive(stream);
        }

        /**
         * Begin Send
         * Starts the NetworkStream for writing data to byte buffer.
         */
        public NetworkStream BeginSend(IPEndPoint endPoint)
        {
            NetworkStream stream = Reserve();
            stream.remoteEndPoint = remote;
            //stream.args.RemoteEndPoint = remote;
            stream.SetBufferLength(0);
            return stream;
        }

        /**
         * Begin Send
         * Starts the NetworkStream for writing data to byte buffer.
         */
        public NetworkStream BeginSend()
        {
            NetworkStream stream = Reserve();
            stream.remoteEndPoint = remote;
            //stream.args.RemoteEndPoint = remote;
            stream.SetBufferLength(0);
            return stream;
        }

        /**
         * End Send
         * Finish writing the stream and push to send queue
         */
        public void EndSend(NetworkStream stream)
        {
            stream.Complete();
            

            //ExecuteSend(stream);
            lock (NetworkThread.SENDQUEUE)
            {
                NetworkThread.SENDQUEUE.Enqueue(stream);
            }
        }
        /*
        public void ExecuteSend(NetworkStream stream)
        {
            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, stream);

            if (!socket.SendToAsync(stream.args))
            {
                Console.WriteLine("SendToAsync Failed");
                //finished synchronously, process immediately
                OnSocketCompleted(this, stream.args);
            }
        }
        */
        /**
         * Execute Send
         * Thread is attempting to send data through socket.
         */
         
        public void ExecuteSend(NetworkStream stream)
        {
            try
            {
                stream.byteSent = socket.SendTo(stream.ByteBuffer, stream.byteLength, SocketFlags.DontRoute, stream.remoteEndPoint);
            }
            catch(Exception e) {
                Console.WriteLine(e.ToString());
            }

            OnSocketSend(stream);

            Free(stream);
        }

        /**
        * Event: OnSocketCompleted 
        * Called when an async receive/send has completed.
        *//*
        void OnSocketCompleted(object sender, SocketAsyncEventArgs e)
        {
            NetworkStream stream = (NetworkStream)e.UserToken;

            // determine which type of operation just completed and call the associated handler
            switch (stream.args.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom: OnSocketReceive(stream); break;
                case SocketAsyncOperation.SendTo: OnSocketSend(stream); break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }*/

        /**
         * Event: OnSocketReceive
         * Called when data has been fully received from a remote connection.
         */
        void OnSocketReceive(NetworkStream stream)
        {
            try
            {
                //int byteLength = stream.args.BytesTransferred;
                //if (byteLength > 0 && stream.args.SocketError == SocketError.Success)
                {
                    //stream.Complete(stream.byteLength);

                    if (OnReceive != null) //notify any event listeners
                        OnReceive.Invoke(this, stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //Listen(stream); //listen again
        }

        /**
         * Event: OnSocketSend
         * Called after data has been sent to remote connection.
         */
        void OnSocketSend(NetworkStream stream)
        {
            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, stream);

            
        }

        public void OnCompleted(Object sender, SocketAsyncEventArgs args)
        {

        }

        /**
         * Reserve a socket event from the pool.  
         * Setup the socket, completed callback and UserToken
         */
        public NetworkStream Reserve()
        {
            NetworkStream stream = NetworkThread.STREAMPOOL.Reserve();
            stream.socket = this;
            //stream.args.AcceptSocket = socket;
            //stream.args.Completed += evtSocketCompleted;
            //stream.args.UserToken = stream;

            return stream;
        }

        /**
         * Free a socket event back into the pool.
         * Reset all the initial properties to null.
         */
        public void Free(NetworkStream stream)
        {
            //lock (activeEvents)
            {
               // activeEvents.Remove(socketEvent.id);
            }
            stream.socket = null;
            //stream.args.AcceptSocket = null;
            //stream.args.Completed -= evtSocketCompleted;
            //stream.args.UserToken = null;
            //stream.args.RemoteEndPoint = null;

            NetworkThread.STREAMPOOL.Free(stream);
        }


        /**
         * Clean any open socket events and shutdown socket.
         */
        public void Dispose()
        {
            try
            {
                foreach (KeyValuePair<int, NetworkStream> entry in activeEvents)
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
