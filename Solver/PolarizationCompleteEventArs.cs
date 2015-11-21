using System;

namespace Porvem.Cartesian.Magnetotellurics
{

    public class PolarizationCompleteEventArs : EventArgs
    {
        public Polarization Polarization1 { get; }

        public PolarizationCompleteEventArs(Polarization polarization)
        {
            Polarization1 = polarization;
        }
    }
}
