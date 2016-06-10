using System;
using System.Numerics;
using Extreme.Core.Model;

using static System.Math;

namespace Extreme.Cartesian.Model
{
    public static class OmegaModelUtils
    {
        public const double Mu0 = (4.0 * Math.PI * 1.0E-07);
		public const double Epsilon0 = 1E0/(Mu0*299792458L*299792458L);	//(1E-9 / (36 * Math.PI));



        public static Complex ConvertSigmaToZeta(double omega, Complex sigma)
        {
          //  return new Complex((sigma.Real + omega * Epsilon0), (sigma.Imaginary - omega * Epsilon0));
			return new Complex((sigma.Real), (sigma.Imaginary - omega * Epsilon0));
        }


        public static Complex[,,] ConvertSigmaToZeta(double omega, double[,,] sigma)
        {
            int nx = sigma.GetLength(0);
            int ny = sigma.GetLength(1);
            int nz = sigma.GetLength(2);

            var result = new Complex[nx,ny,nz];

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                    for (int k = 0; k < nz; k++)
                        result[i, j, k] = ToZeta(sigma[i, j, k], omega);


            return result;
        }

        private static Complex ToZeta(double sigma, double omega)
		=> new Complex(sigma, -omega * Epsilon0);
            //=> new Complex((sigma + omega * Epsilon0), -omega * Epsilon0);

        public static Complex[] GetAllSection1DZeta(this ISection1D<IsotropyLayer> section1D)
        {
            var result = new Complex[section1D.NumberOfLayers];

            for (int i = 0; i < result.Length; i++)
                result[i] = section1D[i].Zeta;

            return result;
        }

        public static double FrequencyToOmega(double frequency)
        {
            return (2 * Math.PI * frequency);
        }
    }
}
