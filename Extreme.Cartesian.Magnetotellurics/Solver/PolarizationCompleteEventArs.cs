//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

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
