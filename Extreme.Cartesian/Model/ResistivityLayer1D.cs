using System.Numerics;
using Extreme.Core.Model;

namespace Extreme.Cartesian.Model
{
    public class ResistivityLayer1D : Layer1D, IResistivityLayer1D
    {
        public ResistivityLayer1D(decimal thicknessInMeters, double resistivity)
            : base(thicknessInMeters)
        {
            Resistivity = resistivity;
        }

        public double Resistivity { get; }
    }
}
