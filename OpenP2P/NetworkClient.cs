using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkClient
    {
        public Socket udpTraversal;
        public Socket udpClient;

        public IPEndPoint serverEndPoint;
        public IPEndPoint localEndPoint;

        public NetworkClient(string serverHost)
        {
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverHost), 9000);
            localEndPoint = new IPEndPoint(IPAddress.Any, 9001);

            udpTraversal = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            SetupClients();

            
        }

        public void SetupClients()
        {
            udpTraversal.ExclusiveAddressUse = false;
            udpTraversal.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpTraversal.Bind(localEndPoint);

            udpClient.ExclusiveAddressUse = false;
            udpClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Bind(localEndPoint);
        }

        public void ConnectToServer()
        {

        }

        public string GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Failed to get local IP");
        }


    }
}
