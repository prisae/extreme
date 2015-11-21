using System;
using Extreme.Cartesian.Core;

namespace Extreme.Cartesian.Forward
{
    public class CieSolverFinishedEventArgs : EventArgs
    {
        public AnomalyCurrent Chi { get; }

        public CieSolverFinishedEventArgs(AnomalyCurrent chi)
        {
            if (chi == null) throw new ArgumentNullException(nameof(chi));
            Chi = chi;
        }
    }
}
