using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkTime
    {
        public static Stopwatch stopwatch = new Stopwatch();

        public static void Start()
        {
            stopwatch.Restart();
        }

        public static long Milliseconds()
        {
            return stopwatch.ElapsedMilliseconds;
            //return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            //return Environment.TickCount / TimeSpan.TicksPerMillisecond;
        }
    }
}
