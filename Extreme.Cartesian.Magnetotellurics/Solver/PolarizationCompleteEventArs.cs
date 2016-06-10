using System;

namespace Extreme.Cartesian.Magnetotellurics
{

    public class PolarizationCompleteEventArs : EventArgs
    {
        public Polarization Polarization { get; }

        public PolarizationCompleteEventArs(Polarization polarization)
        {
            Polarization = polarization;
        }
    }
}
