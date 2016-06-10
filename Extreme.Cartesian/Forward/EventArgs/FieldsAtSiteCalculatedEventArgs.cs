using System;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public class FieldsAtSiteCalculatedEventArgs : EventArgs
    {
        public ObservationSite ObservationSite { get; }
        public ComplexVector NormalField { get; }
        public ComplexVector AnomalyField { get; }

        public FieldsAtSiteCalculatedEventArgs(ObservationSite observationSite, ComplexVector normalField, ComplexVector anomalyField)
        {
            ObservationSite = observationSite;
            NormalField = normalField;
            AnomalyField = anomalyField;
        }
    }
}
