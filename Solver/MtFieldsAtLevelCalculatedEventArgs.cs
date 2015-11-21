using System;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Forward;
using Extreme.Core;

namespace Porvem.Cartesian.Magnetotellurics
{
    public class MtFieldsAtLevelCalculatedEventArgs : FieldsAtLevelCalculatedEventArgs
    {
        public Polarization Polarization { get; }

        public MtFieldsAtLevelCalculatedEventArgs(Polarization polarization, ObservationLevel level, AnomalyCurrent normalField, AnomalyCurrent anomalyField)
            : base(level, normalField, anomalyField)
        {
            Polarization = polarization;
        }

    }
}
