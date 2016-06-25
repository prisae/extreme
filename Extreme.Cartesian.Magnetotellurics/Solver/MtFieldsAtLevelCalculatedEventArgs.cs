//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Forward;
using Extreme.Core;

namespace Extreme.Cartesian.Magnetotellurics
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
