using System;
using Extreme.Cartesian.Core;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public class FieldsAtLevelCalculatedEventArgs : EventArgs
    {
        public ObservationLevel Level { get; }
        public AnomalyCurrent NormalField { get; }
        public AnomalyCurrent AnomalyField { get; }

        public FieldsAtLevelCalculatedEventArgs(ObservationLevel level, AnomalyCurrent normalField, AnomalyCurrent anomalyField)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (normalField == null) throw new ArgumentNullException(nameof(normalField));
            if (anomalyField == null) throw new ArgumentNullException(nameof(anomalyField));

            Level = level;
            NormalField = normalField;
            AnomalyField = anomalyField;
            Level = level;
        }

    }
}
