using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Core
{
    public interface ITransceiverElement
    {
        decimal StartingDepth { get; }
        decimal Thickness { get; }
        int Index { get; }
    }
}
