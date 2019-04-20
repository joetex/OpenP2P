using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkConfig
    {
        public const int MAX_SEND_THREADS = 1;
        public const int MAX_RECV_THREADS = 1;
        public const int MAX_RELIABLE_THREADS = 1;

        public const int BufferPoolStartCount = 10000;
        public const int BufferMaxLength = 100;
        public const int SocketBufferCount = 1000;
        public const int SocketSendRate = 1000;
        public const int SocketReceiveTimeout = 0;

        //important to sleep more, since they are on infinite loops
        public const int ThreadWaitingSleepTime = 1;
        public const int ThreadSendRateSleepTime = 0;
        public const int ThreadReliableSleepTime = 1;

        public const long SocketReliableRetryDelay = 300;
        public const long SocketReliableRetryAttempts = 4;

        public static Stopwatch profiler = new Stopwatch();
        public static Dictionary<string, long> profileTimes = new Dictionary<string, long>();
        public static Dictionary<string, long> profileStart = new Dictionary<string, long>();
        public static Dictionary<string, long> profileEnd = new Dictionary<string, long>();

        public static void ProfileEnable()
        {
            profiler.Start();
        }
        public static void ProfileBegin(string name)
        {
            lock(profileStart)
            {
                if (!profileStart.ContainsKey(name))
                {
                    profileStart.Add(name, profiler.ElapsedMilliseconds);
                }
                else
                {
                    profileStart[name] = profiler.ElapsedMilliseconds;
                }
            }
        }

        public static void ProfileEnd(string name)
        {
            lock (profileTimes)
            {
                if (!profileTimes.ContainsKey(name))
                {
                    profileTimes.Add(name, 0);
                }

                long saved = profileTimes[name];
                long start = profileStart[name];
                long end = profiler.ElapsedMilliseconds;
                long diff = end - start;
                profileTimes[name] = diff + saved;
            }
        }

        public static void ProfileReport(string name)
        {
            double total = ((double)profileTimes[name]) / 1000f;
            Console.WriteLine(name + " took " + total + " seconds");
        }

        public static void ProfileReportAll()
        {
            lock(profileTimes)
            {
                foreach (KeyValuePair<string, long> pair in profileTimes)
                {
                    ProfileReport(pair.Key);
                }
            }

        }
    }
}
