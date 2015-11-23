using System;
using System.Numerics;
using Extreme.Core.Model;

using static System.Math;

namespace Extreme.Cartesian.Model
{
    public static class OmegaModelUtils
    {
        public const double Mu0 = (4.0 * Math.PI * 1.0E-07);
        public const double Epsilon0 = (1E-9 / (36 * Math.PI));

        public static Complex ConvertSigmaToZeta(double omega, Complex sigma)
        {
            return new Complex((sigma.Real + omega * Epsilon0), (sigma.Imaginary - omega * Epsilon0));
        }


        public static Complex[,] ConvertSigmaToZeta(double omega, double[,] sigma)
        {
            var result = new Complex[sigma.GetLength(0), sigma.GetLength(1)];

            for (int i = 0; i < sigma.GetLength(0); i++)
                for (int j = 0; j < sigma.GetLength(1); j++)
                    result[i, j] = ToZeta(sigma[i, j], omega);

            return result;
        }

        public static Complex[][,] ConvertSigmaToZeta(double omega, double[,,] sigma)
        {
            var result = new Complex[sigma.GetLength(2)][,];

            int nx = sigma.GetLength(0);
            int ny = sigma.GetLength(1);

            for (int k = 0; k < result.Length; k++)
            {
                result[k] = new Complex[nx, ny];

                for (int i = 0; i < nx; i++)
                    for (int j = 0; j < ny; j++)
                        result[k][i, j] = ToZeta(sigma[i, j, k], omega);
            }

            return result;
        }

        private static Complex ToZeta(double sigma, double omega)
            => new Complex((sigma + omega * Epsilon0), -omega * Epsilon0);

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

        public static double OmegaToFrequency(double omega)
        {
            return (omega / (2 * Math.PI));
        }

        public static double GetFrequency(this OmegaModel model)
        {
            return OmegaToFrequency(model.Omega);
        }
    }
}
