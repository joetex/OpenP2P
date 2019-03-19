
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
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, NetworkThread.BUFFER_LENGTH * NetworkThread.MAX_BUFFER_PACKET_COUNT);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, NetworkThread.BUFFER_LENGTH * NetworkThread.MAX_BUFFER_PACKET_COUNT);
            //if (localPort != 0)
                socket.Bind(local);
        }

        /**
         * Request a Listen on the RecvThread
         */
        public void Listen(NetworkStream stream)
        {
            if (stream == null)
                stream = Reserve();

            stream.Reset();

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
            stream.Reset();

            try
            {
                int bytesReceived = socket.ReceiveFrom(stream.ByteBuffer, ref stream.remoteEndPoint);
                stream.SetBufferLength(bytesReceived);

                if (OnReceive != null) //notify any event listeners
                    OnReceive.Invoke(this, stream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Listen(stream); //listen again
        }
        
        /**
         * Begin Send
         * Starts the NetworkStream for writing data to byte buffer.
         */
        public NetworkStream Prepare(EndPoint endPoint)
        {
            NetworkStream stream = Reserve();
            stream.remoteEndPoint = endPoint;
            stream.SetBufferLength(0);
            return stream;
        }
        
        /**
         * End Send
         * Finish writing the stream and push to send queue for SendThread
         */
        public void Send(NetworkStream stream)
        {
            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, stream);

            stream.Complete();

            lock (NetworkThread.SENDQUEUE)
            {
                NetworkThread.SENDQUEUE.Enqueue(stream);
            }
        }

        /**
         * Send Internal
         * Thread triggers send to remote point
         */
        public void SendInternal(NetworkStream stream)
        {
            try
            {
                stream.byteSent = socket.SendTo(stream.ByteBuffer, stream.byteLength, SocketFlags.DontRoute, stream.remoteEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
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
            return stream;
        }

        /**
         * Free a socket event back into the pool.
         * Reset all the initial properties to null.
         */
        public void Free(NetworkStream stream)
        {
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
