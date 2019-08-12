using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkTime
    {
        public static long Milliseconds()
        {
            return Environment.TickCount / TimeSpan.TicksPerMillisecond;
        }
    }
}
