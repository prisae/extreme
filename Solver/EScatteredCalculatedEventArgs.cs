using System;
using Extreme.Cartesian.Core;

namespace Porvem.Cartesian.Magnetotellurics
{
    public class EScatteredCalculatedEventArgs : EventArgs
    {
        public AnomalyCurrent NormalFieldAtAnomaly { get; }
        public AnomalyCurrent EScattered { get; }

        public EScatteredCalculatedEventArgs(AnomalyCurrent normalFieldAtAnomaly, AnomalyCurrent eScattered)
        {
            NormalFieldAtAnomaly = normalFieldAtAnomaly;
            EScattered = eScattered;
        }
    }
}
