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
        public Socket socket;
        public IPEndPoint remote;
        public IPEndPoint local;

        public static NetworkSocketEventPool EVENTPOOL = new NetworkSocketEventPool(10, 1500);
        public Dictionary<int, NetworkSocketEvent> activeEvents = new Dictionary<int, NetworkSocketEvent>();

        public bool isSending = false;
        public bool isReceiving = false;

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

        public void Setup(string remoteHost, int remotePort, int localPort)
        {
            remote = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            local = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ExclusiveAddressUse = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(local);
        }

        public void Listen()
        {
            if (isReceiving)
                return;
            isReceiving = true;
            NetworkSocketEvent socketEvent = ReserveSocketEvent(true);
           
            if (!socket.ReceiveAsync(socketEvent.args))
            {
                Console.WriteLine("ReceiveAsync Failed");
                //finished synchronously, process immediately
                ProcessEvent(socketEvent);
            }
        }

        public void Send(string msg)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            Send(data);
        }

        public void Send(byte[] data)
        {
            NetworkSocketEvent socketEvent = ReserveSocketEvent(false);
            socketEvent.SetBufferBytes(data);
            socketEvent.args.RemoteEndPoint = remote;
            if ( !socket.SendToAsync(socketEvent.args ))
            {
                Console.WriteLine("SendToAsync Failed");
                //finished synchronously, process immediately
                ProcessEvent(socketEvent);
            }
        }

        void OnReceive(NetworkSocketEvent socketEvent)
        {
            if (socketEvent.args.BytesTransferred > 0
                && socketEvent.args.SocketError == SocketError.Success)
            {
                byte[] data = new byte[socketEvent.args.BytesTransferred];
                for (int i = 0; i < socketEvent.args.BytesTransferred; i++)
                {
                    data[i] = socketEvent.args.Buffer[i];
                }
                Console.WriteLine("Received message: " + System.Text.Encoding.ASCII.GetString(data));
            }
            
        }

        void OnSend(NetworkSocketEvent socketEvent)
        {

        }

        public NetworkSocketEvent ReserveSocketEvent(bool withBuffer)
        {
            NetworkSocketEvent socketEvent = EVENTPOOL.Reserve(withBuffer);
            socketEvent.args.AcceptSocket = socket;
            socketEvent.args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSocketCompleted);
            socketEvent.args.UserToken = socketEvent;
            //Console.WriteLine("Reserving Socket: " + socketEvent.id);
            activeEvents.Add(socketEvent.id, socketEvent);

            return socketEvent;
        }

        public void FreeSocketEvent(NetworkSocketEvent socketEvent)
        {
            //Console.WriteLine("Freeing Socket: " + socketEvent.id);
            socketEvent.args.AcceptSocket = null;
            activeEvents.Remove(socketEvent.id);
            EVENTPOOL.Free(socketEvent);
        }

        void OnSocketCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessEvent((NetworkSocketEvent)e.UserToken);
        }

        void ProcessEvent(NetworkSocketEvent socketEvent)
        {
            // determine which type of operation just completed and call the associated handler
            switch (socketEvent.args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                    isReceiving = false;
                    Listen(); //listen again
                    OnReceive(socketEvent);


                    break;
                case SocketAsyncOperation.Send:
                case SocketAsyncOperation.SendTo:
                    isSending = false;
                    OnSend(socketEvent);

                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

            FreeSocketEvent(socketEvent);
        }

        

        public void Dispose()
        {
            try
            {
                foreach (KeyValuePair<int, NetworkSocketEvent> entry in activeEvents)
                {
                    FreeSocketEvent(entry.Value);
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
