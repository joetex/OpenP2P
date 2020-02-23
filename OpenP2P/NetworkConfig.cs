using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkConfig
    {
        public const int MAXCLIENTS = 1;
        public const int MAXSEND = 200000;

        public const int MAX_SEND_THREADS = 6;
        public const int MAX_RECV_THREADS = 1;
        public const int MAX_RELIABLE_THREADS = 1;

        public const int MessagePoolInitialCount = 1000;
        public const int PacketPoolBufferInitialCount = 1000;

        public const int PacketPoolBufferMaxLength = 1500;

        
        public const int BufferMaxLength = 15000;
        public const int SocketBufferCount = 15000;
        public const int SocketSendRate = 1000;
        public const int SocketReceiveTimeout = 0;

        //important to sleep more, since they are on infinite loops
        public const int ThreadSendSleepPacketSizePerFrame = 20000;
        public const int ThreadSendSleepPacketsPerFrame = 500;
        public const int ThreadWaitingSleepTime = 1;
        public const int ThreadSendRateSleepTime = 1;
        public const int ThreadReliableSleepTime = 1;
        public const int ThreadRecvProcessSleepTime = 1;

        public static long SocketReliableRetryDelay = 200;
        public static long SocketReliableRetryAttempts = 2;

        public static long NetworkSendRate = 200;

        public static Stopwatch profiler = new Stopwatch();
        public static Dictionary<string, long> profileTimes = new Dictionary<string, long>();
        public static Dictionary<string, long> profileStart = new Dictionary<string, long>();
        public static Dictionary<string, long> profileEnd = new Dictionary<string, long>();

        static NetworkConfig() { }

        public static void ProfileEnable()
        {
            profiler.Start();
        }
        public static void ProfileBegin(string name)
        {
            //lock(profileStart)
            //{
            //    if (!profileStart.ContainsKey(name))
            //    {
            //        profileStart.Add(name, profiler.ElapsedMilliseconds);
            //    }
            //    else
            //    {
            //        profileStart[name] = profiler.ElapsedMilliseconds;
            //    }
            //}
        }

        public static void ProfileEnd(string name)
        {
            //lock (profileTimes)
            //{
            //    if (!profileTimes.ContainsKey(name))
            //    {
            //        profileTimes.Add(name, 0);
            //    }

            //    long saved = profileTimes[name];
            //    long start = profileStart[name];
            //    long end = profiler.ElapsedMilliseconds;
            //    long diff = end - start;
            //    profileTimes[name] = diff + saved;
            //}
        }

        public static void ProfileReport(string name)
        {
            double total = ((double)profileTimes[name]) / 1000f;
            Console.WriteLine(name + " took " + total + " seconds");
        }

        public static void ProfileReportAll()
        {
            //lock(profileTimes)
            {
                foreach (KeyValuePair<string, long> pair in profileTimes)
                {
                    ProfileReport(pair.Key);
                }
            }

        }

        public static string GetPublicIP()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

            request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    publicIPAddress = reader.ReadToEnd();
                }
            }

            return publicIPAddress.Replace("\n", "");
        }

        public static String GetPublicIP2()
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply pingResult = ping.Send("www.google.com");
                    if (pingResult?.Status == IPStatus.Success)
                    {
                        pingResult = ping.Send(pingResult.Address, 3000, Encoding.ASCII.GetBytes("ping"), new PingOptions { Ttl = 2 });

                        bool isRealIp = !IsLocalIp(pingResult.Address);

                        Console.WriteLine(pingResult?.Address == null
                            ? $"Has {(isRealIp ? string.Empty : "no ")}real IP, status: {pingResult?.Status}"
                            : $"Has {(isRealIp ? string.Empty : "no ")}real IP, response from: {pingResult.Address}, status: {pingResult.Status}");

                        Console.WriteLine($"ISP assigned REAL EXTERNAL IP to your router, response from: {pingResult?.Address}, status: {pingResult?.Status}");
                        return pingResult.Address.ToString();
                    }
                    else
                    {
                        Console.WriteLine($"Your router appears to be behind ISP networks, response from: {pingResult?.Address}, status: {pingResult?.Status}");
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Failed to resolve external ip address by ping");
                Console.WriteLine(exc.ToString());
            }
            return "127.0.0.1";
        }

        public static bool IsLocalIp(IPAddress ip)
        {
            var ipParts = ip.ToString().Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

            return (ipParts[0] == 192 && ipParts[1] == 168)
                || (ipParts[0] == 172 && ipParts[1] >= 16 && ipParts[1] <= 31)
                || ipParts[0] == 10;
        }
    }
}
