using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Extreme.Core
{
    public interface IAnomalyLayer
    {
        /// <summary>
        /// Provides zeta value at specified index
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        Complex GetZeta(int i, int j);

        decimal Depth { get; }
        decimal Thickness { get; }
    }
}
