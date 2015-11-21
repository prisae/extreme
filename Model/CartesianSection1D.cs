using Extreme.Core.Model;

namespace Extreme.Cartesian.Model
{
    public class CartesianSection1D : Section1D<Sigma1DLayer>
    {
        public CartesianSection1D(Sigma1DLayer[] layers)
            : base(layers)
        {
        }

        public CartesianSection1D(decimal zeroAirLevel, Sigma1DLayer[] layers)
            : base(zeroAirLevel, layers)
        {
        }
    }
}
