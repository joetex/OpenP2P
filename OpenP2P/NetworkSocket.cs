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
            remote = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            local = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);
            anyHost = new IPEndPoint(IPAddress.Any, 0); 

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ExclusiveAddressUse = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(local);
        }
        
        
        /**
         * Request a Listen on the RecvThread
         */
        public void Listen(NetworkStream stream)
        {
            if(stream == null )
                stream = Reserve();
            stream.remoteEndPoint = anyHost;
            
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
            int bytesReceived = 0;
            try
            {
                bytesReceived = socket.ReceiveFrom(stream.ByteBuffer, ref stream.remoteEndPoint);
            }
            catch(Exception e) {}

            stream.SetBufferLength(bytesReceived);
            OnSocketReceive(stream);
        }

        /**
         * Begin Send
         * Starts the NetworkStream for writing data to byte buffer.
         */
        public NetworkStream BeginSend(IPEndPoint endPoint)
        {
            NetworkStream stream = Reserve();
            stream.remoteEndPoint = endPoint;
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
            return stream;
        }

        /**
         * End Send
         * Finish writing the stream and push to send queue
         */
        public void EndSend(NetworkStream stream)
        {
            lock (NetworkThread.SENDQUEUE)
            {
                NetworkThread.SENDQUEUE.Enqueue(stream);
            }
        }

        /**
         * Execute Send
         * Thread is attempting to send data through socket.
         */
        public void ExecuteSend(NetworkStream stream)
        {
            try
            {
                int result = socket.SendTo(stream.ByteBuffer, stream.byteLength, SocketFlags.None, stream.remoteEndPoint);
            }
            catch(Exception e) { }
            
            OnSocketSend(stream);
        }


        /**
         * Event: OnSocketReceive
         * Called when data has been fully received from a remote connection.
         */
        void OnSocketReceive(NetworkStream stream)
        {
            if (OnReceive != null) //notify any event listeners
                OnReceive.Invoke(this, stream);

            Listen(stream); //listen again
        }

        /**
         * Event: OnSocketSend
         * Called after data has been sent to remote connection.
         */
        void OnSocketSend(NetworkStream stream)
        {
            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, stream);

            Free(stream);
        }

        /**
         * Reserve a socket event from the pool.  
         * Setup the socket, completed callback and UserToken
         */
        public NetworkStream Reserve()
        {
            NetworkStream stream = NetworkThread.STREAMPOOL.Reserve();
            stream.socket = this;
            stream.SetBufferLength(0);
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
