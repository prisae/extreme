using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Extreme.Core
{
    public interface IAnomalyLayer
    {
        decimal Depth { get; }
        decimal Thickness { get; }
    }
}
