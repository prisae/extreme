using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Extreme.Core.Model
{
    public class NonMeshedModel
    {
        private readonly List<NonMeshedAnomaly> _anomalies;

        public NonMeshedModel(ISection1D<Layer1D> section1D)
        {
            Section1D = section1D;
            _anomalies = new List<NonMeshedAnomaly>();
        }

        public ISection1D<Layer1D> Section1D { get; }

        public IReadOnlyCollection<NonMeshedAnomaly> Anomalies 
            => new ReadOnlyCollection<NonMeshedAnomaly>(_anomalies);

        public void AddAnomaly(NonMeshedAnomaly anomaly)
        {
            if (anomaly == null) 
                throw new ArgumentNullException(nameof(anomaly));
        
            _anomalies.Add(anomaly);
        }

        public decimal GetMinZ() => _anomalies.Min(a => a.Z.Start);
        public decimal GetMaxZ() => _anomalies.Max(a => a.Z.Start + a.Z.Size);
    }
}
