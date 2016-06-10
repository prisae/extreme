using System.Linq;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core.Model;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public class AlphaBeta
    {
        public readonly Complex[] Alpha1;
        public readonly Complex[] Beta1;
        public readonly Complex[] AlphaSigma;
        public readonly Complex[] BetaSigma;

        private AlphaBeta(Complex[] alpha1, Complex[] beta1, Complex[] alphaSigma, Complex[] betaSigma)
        {
            Alpha1 = alpha1;
            Beta1 = beta1;
            AlphaSigma = alphaSigma;
            BetaSigma = betaSigma;
        }

        public static AlphaBeta CreateFrom(ISection1D<IsotropyLayer> section1D)
        {
            int numberOf1DLayers = section1D.NumberOfLayers;
            var zeta = section1D.GetAllSection1DZeta();

            var alpha1 = Enumerable.Repeat(Complex.One, numberOf1DLayers).ToArray();
            var beta1 = Enumerable.Repeat(Complex.One, numberOf1DLayers).ToArray();

            var alphaSigma = new Complex[numberOf1DLayers];
            var betaSigma = new Complex[numberOf1DLayers];

            for (int k = 0; k < numberOf1DLayers - 1; k++)
                alphaSigma[k] = zeta[k + 1] / zeta[k];

            for (int k = 1; k < numberOf1DLayers; k++)
                betaSigma[k] = zeta[k - 1] / zeta[k];

            return new AlphaBeta(alpha1, beta1, alphaSigma, betaSigma);
        }
    }
}
