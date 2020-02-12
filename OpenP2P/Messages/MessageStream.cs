using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /// <summary>
    /// NetworkMessageStream Packet format:
    /// |------------------------------------------------------------|
    /// |  Start Pos (4 bytes)                                       |
    /// |------------------------------------------------------------|
    /// |  Full Length (4 bytes) 1st time only                       |
    /// |------------------------------------------------------------|
    /// |  Command String (2 byte length) + (X bytes) 1st time only  |
    /// |------------------------------------------------------------|
    /// |  Segment Length (4 bytes)                                  |
    /// |------------------------------------------------------------|
    /// |  Segment Byte Data (Y bytes)                               |
    /// |------------------------------------------------------------|
    ///
    /// Continous streams
    ///     1) Trigger using command string
    ///     2) Ends when Segment Length = 0
    ///     
    /// Discreet streams
    ///     1) Use all segments
    ///     2) Ends when summation of Segment Length = Full Length
    /// 
    /// </summary>
    public class MessageStream : NetworkMessageStream
    {
        public MessageStream() : base()
        {
            command = "data";
        }

        
    }
}
