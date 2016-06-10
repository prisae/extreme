using Extreme.Cartesian.Model;

namespace Extreme.Model
{
    public abstract class AnalyticModel
    {
        public CartesianSection1D Section1D { get; }

        public decimal MaxZ { get; }
        public decimal MinZ { get; }

        protected AnalyticModel(CartesianSection1D section1D, decimal maxZ, decimal minZ)
        {
            Section1D = section1D;
            MaxZ = maxZ;
            MinZ = minZ;
        }

        public abstract double GetValue(decimal x, decimal y, decimal z);
    }
}
