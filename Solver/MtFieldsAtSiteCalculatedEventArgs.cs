﻿using System;
using Extreme.Cartesian;
using Extreme.Cartesian.Forward;
using Extreme.Core;

namespace Extreme.Cartesian.Magnetotellurics
{
    public class MtFieldsAtSiteCalculatedEventArgs : FieldsAtSiteCalculatedEventArgs
    {
        public Polarization Polarization { get; }

        public MtFieldsAtSiteCalculatedEventArgs(Polarization polarization, ObservationSite observationSite,
            ComplexVector normalField, ComplexVector anomalyField)
            : base(observationSite, normalField, anomalyField)
        {
            Polarization = polarization;
        }
    }
}
