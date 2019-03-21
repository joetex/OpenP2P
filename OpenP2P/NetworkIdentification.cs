using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class NetworkIdentification
    {
        public NetworkIdentification() { }

        public ConcurrentDictionary<string, int> peerIds = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<int, EndPoint> peerEndpoints = new ConcurrentDictionary<int, EndPoint>();
        public ConcurrentDictionary<int, string> usedIds = new ConcurrentDictionary<int, string>();
        public MD5 md5Hasher = MD5.Create();

        
        public void RegisterPeer(string ip, int port)
        {
            string ipport = ip + port;
            int id = GeneratePeerId(ipport);
            EndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);

            int maxTries = 100;
            while (!usedIds.TryAdd(id, ipport) && --maxTries > 0) { }
            if (maxTries < 99)
                Console.WriteLine("Failed to add usedId: " + id);

            maxTries = 100;
            while (!peerIds.TryAdd(ipport, id) && --maxTries > 0) { }
            if (maxTries < 99)
                Console.WriteLine("Failed to add peerIds: " + id);

            maxTries = 100;
            while (!peerEndpoints.TryAdd(id, ep) && --maxTries > 0) { }
            if (maxTries < 99)
                Console.WriteLine("Failed to add peerEndpoints: " + id);
        }

        public int GeneratePeerId(string endpoint)
        {
            byte[] hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(endpoint));
            int id = BitConverter.ToInt32(hashed, 0);

            while (usedIds.ContainsKey(id))
            {
                id += 1;
            }
            return id;
        }

        public int GetPeerId(string endpoint)
        {
            if (!peerIds.ContainsKey(endpoint))
                return 0;
            return peerIds[endpoint];
        }

        public string GetPeerEndpoint(int id)
        {
            if (!usedIds.ContainsKey(id))
                return null;
            return usedIds[id];
        }

    }
}
