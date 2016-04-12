using System;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green.Tensor;
namespace Extreme.Cartesian.Forward
{
    public class CieSolverFinishedEventArgs : EventArgs
    {
        public AnomalyCurrent Chi { get; }
		public GreenTensor Gt=null;

        public CieSolverFinishedEventArgs(AnomalyCurrent chi)
        {
            if (chi == null) throw new ArgumentNullException(nameof(chi));
            Chi = chi;
			Gt = null;
        }
		public CieSolverFinishedEventArgs(AnomalyCurrent chi, GreenTensor gt)
		{
			if (chi == null) throw new ArgumentNullException(nameof(chi));
			Chi = chi;
			Gt = gt;
		}
    }
}
