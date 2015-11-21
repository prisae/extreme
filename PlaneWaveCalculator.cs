using System.Numerics;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Green.Scalar.Impl;
using Extreme.Cartesian.Model;

namespace Porvem.Cartesian.Magnetotellurics
{
    public static class PlaneWaveCalculator
    {
        private const double Mu0 = (4.0 * System.Math.PI * 1.0E-07);
        public static Complex CalculateFieldE(OmegaModel model, decimal recieverDepth)
        {
            // c.CorrBackgroundTr is always layer with air boundary
            var c = PrepareContainer(model, recieverDepth);

            var kkr = Complex.ImaginaryOne * model.Omega * Mu0 * model.Section1D[c.CorrBackgroundRc].Zeta;
            var ikr = Complex.ImaginaryOne * Complex.Sqrt(kkr);

            var kk1 = Complex.ImaginaryOne * model.Omega * Mu0 * model.Section1D[c.CorrBackgroundTr].Zeta;
            var ik1 = Complex.ImaginaryOne * Complex.Sqrt(kk1);

            var dr1 = (double)model.GetLayerDepth(c.CorrBackgroundRc + 1);
            var d2 = (double)model.GetLayerDepth(c.CorrBackgroundTr + 1);

            var zr = (double)recieverDepth;
            var a = c.A[c.CorrBackgroundRc, c.CorrBackgroundTr] / c.A[c.CorrBackgroundTr, c.CorrBackgroundTr];

            var upper = Complex.Exp(ikr * zr) + c.Q[c.CorrBackgroundRc] * Complex.Exp(ikr * (2 * dr1 - zr));
            var lower = 1 + c.Q[c.CorrBackgroundTr] * Complex.Exp(ik1 * (2 * d2));

            return a * (upper / lower);
        }

        public static Complex CalculateFieldH(OmegaModel model, decimal recieverDepth)
        {
            // c.CorrBackgroundTr is always layer with air boundary
            var c = PrepareContainer(model, recieverDepth);

            var kkr = Complex.ImaginaryOne * model.Omega * Mu0 * model.Section1D[c.CorrBackgroundRc].Zeta;
            var ikr = Complex.ImaginaryOne * Complex.Sqrt(kkr);

            var kk1 = Complex.ImaginaryOne * model.Omega * Mu0 * model.Section1D[c.CorrBackgroundTr].Zeta;
            var ik1 = Complex.ImaginaryOne * Complex.Sqrt(kk1);


            var dr1 = (double)model.GetLayerDepth(c.CorrBackgroundRc + 1);
            var d2 = (double)model.GetLayerDepth(c.CorrBackgroundTr + 1);

            var zr = (double)recieverDepth;
            var a = c.A[c.CorrBackgroundRc, c.CorrBackgroundTr] / c.A[c.CorrBackgroundTr, c.CorrBackgroundTr];

            var upper = Complex.Exp(ikr * zr) - c.Q[c.CorrBackgroundRc] * Complex.Exp(ikr * (2 * dr1 - zr));
            var lower = 1 + c.Q[c.CorrBackgroundTr] * Complex.Exp(ik1 * (2 * d2));

            return (Complex.Sqrt(kkr) / (model.Omega * Mu0)) * a * (upper / lower);
        }

        private static AuxContainer PrepareContainer(OmegaModel model, decimal recieverDepth)
        {
            var alphaBeta = AlphaBeta.CreateFrom(model.Section1D);

            var c = AuxContainer.CreateContainer(model, 0, recieverDepth);
          
            const double lambda = 0;

            c.Eta = PlanCalculator.CalculateEta(model, lambda);
            c.Exp = PlanCalculator.CalculateExp(model, c.Eta);

            c.Q = PlanCalculator.CalculateQ1(alphaBeta, c.Eta, c.Exp);
            c.P = PlanCalculator.CalculateP1(alphaBeta, c.Eta, c.Exp);

            c.A = PlanCalculator.CalculateA1(model, c);

            return c;
        }
    }
}
