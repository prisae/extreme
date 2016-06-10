using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Cartesian.Green.Scalar;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public class VolumeToVolumePlanCalculatorFast : PlanCalculatorFast
    {
        public VolumeToVolumePlanCalculatorFast(OmegaModel model, AlphaBeta alphaBeta, ScalarPlanItem planItem)
            : base(model, alphaBeta, planItem)
        {
        }

        protected override ICalculatorU GetCalculatorU(OmegaModel model, AuxContainerFast c)
            => new VolumeToVolumeCalculatorUFast(model, c);
    }

    public class VolumeToPointPlanCalculatorFast : PlanCalculatorFast
    {
        public VolumeToPointPlanCalculatorFast(OmegaModel model, AlphaBeta alphaBeta, ScalarPlanItem planItem)
            : base(model, alphaBeta, planItem)
        {
        }

        protected override ICalculatorU GetCalculatorU(OmegaModel model, AuxContainerFast c)
        {
            throw new NotImplementedException();
        }
    }


    public abstract class PlanCalculatorFast : IPlanCalculator
    {
        private const double Mu0 = (4.0 * System.Math.PI * 1.0E-07);

        public interface ICalculatorU
        {
            void PreCalculate();
            Complex[] CalculateU1();
            Complex[] CalculateU2();
            Complex[] CalculateU3();
            Complex[] CalculateU4();
            Complex[] CalculateU5(double[] lambda);
        }

        private readonly OmegaModel _model;
        private readonly AlphaBeta _alphaBeta;
        private readonly ScalarPlanItem _planItem;

        protected PlanCalculatorFast(OmegaModel model, AlphaBeta alphaBeta, ScalarPlanItem planItem)
        {
            _model = model;
            _alphaBeta = alphaBeta;
            _planItem = planItem;
        }

        protected abstract ICalculatorU GetCalculatorU(OmegaModel model, AuxContainerFast c);

        public InnerResult CalculateForE(Transmitter transmitter, Receiver receiver)
        {
            var c = AuxContainerFast.CreateContainer(_model, transmitter, receiver);
            var uCalculator = GetCalculatorU(_model, c);
            var result = CalculateForE(c, uCalculator);

            return result;
        }

        public InnerResult CalculateForH(Transmitter transmitter, Receiver receiver)
        {
            var c = AuxContainerFast.CreateContainer(_model, transmitter, receiver);
            var uCalculator = GetCalculatorU(_model, c);
            var result = CalculateForH(c, uCalculator);

            return result;
        }

        private InnerResult CalculateForE(AuxContainerFast c, ICalculatorU u)
        {
            var e = new ResultE();
            var zeta = _model.Section1D.GetAllSection1DZeta();

            c.Eta = CalculateEta(_planItem.Lambdas);
            c.Exp = CalculateExp(c.Eta);

            c.P = CalculateP1(c.Eta, c.Exp);
            c.Q = CalculateQ1(c.Eta, c.Exp);
            c.A = CalculateA1(c);

            u.PreCalculate();

            e.U1 = _planItem.CalculateI1 ? u.CalculateU1() : new Complex[0];

            c.P = CalculatePSigma(zeta, c.Eta, c.Exp);
            c.Q = CalculateQSigma(c.Eta, c.Exp);

            c.A = CalculateASigma(c);

            e.U2 = _planItem.CalculateI2 ? u.CalculateU2() : new Complex[0];
            e.U3 = _planItem.CalculateI3 ? u.CalculateU3() : new Complex[0];
            e.U4 = _planItem.CalculateI4 ? u.CalculateU4() : new Complex[0];
            e.U5 = _planItem.CalculateI5 ? u.CalculateU5(_planItem.Lambdas) : new Complex[0];

            var result = ScalarMathUtils.NormalizeAndPerformHankelFast(e, _planItem, _model, c.CorrBackgroundRc);
            return result;
        }

        private InnerResult CalculateForH(AuxContainerFast c, ICalculatorU u)
        {
            var h = new ResultH();

            var zeta = _model.Section1D.GetAllSection1DZeta();

            c.Eta = CalculateEta(_planItem.Lambdas);
            c.Exp = CalculateExp(c.Eta);

            c.P = CalculateP1(c.Eta, c.Exp);
            c.Q = CalculateQ1(c.Eta, c.Exp);
            c.A = CalculateA1(c);
            h.U11 = _planItem.CalculateI4 ? u.CalculateU1() : new Complex[0];
            h.U31 = _planItem.CalculateI1 ? u.CalculateU3() : new Complex[0];

            c.P = CalculatePSigma(zeta, c.Eta, c.Exp);
            c.Q = CalculateQSigma(c.Eta, c.Exp);
            c.A = CalculateASigma(c);
            h.U1Sigma = _planItem.CalculateI3 ? u.CalculateU1() : new Complex[0];
            h.U4Sigma = _planItem.CalculateI2 ? u.CalculateU4() : new Complex[0];

            var result = ScalarMathUtils.NormalizeAndPerformHankelFast(h, _planItem);
            return result;
        }

        private Complex[,][] CalculateA1(AuxContainerFast c)
            => CalculateA(c);

        private Complex[,][] CalculateASigma(AuxContainerFast c)
        {
            var a = CalculateA(c);

            int s = System.Math.Max(c.CorrBackgroundRc, c.CorrBackgroundTr);
            int r = System.Math.Min(c.CorrBackgroundRc, c.CorrBackgroundTr);

            var length1 = c.P.GetLength(1);

            for (int n = s; n >= r; n--)
                for (int k = n - 1; k >= r; k--)
                {
                    var zetaRc = _model.Section1D[k].Zeta;
                    var zetaTr = _model.Section1D[n].Zeta;

                    for (int m = 0; m < length1; m++)
                        a[n, k][m] = (zetaTr / zetaRc) * a[k, n][m];
                }

            return a;
        }

        private Complex[,][] CalculateA(AuxContainerFast c)
        {
            var length0 = c.P.GetLength(0);
            var length1 = c.P.GetLength(1);

            var result = new Complex[length0, length0][];

            for (int i = 0; i < length0; i++)
                for (int j = 0; j < length0; j++)
                    result[i, j] = new Complex[length1];

            int s = System.Math.Max(c.CorrBackgroundRc, c.CorrBackgroundTr);
            int r = System.Math.Min(c.CorrBackgroundRc, c.CorrBackgroundTr);

            for (int n = s; n >= r; n--)
            {
                for (int m = 0; m < length1; m++)
                {
                    Complex prevA = 1.0 / (c.Eta[n, m] * (1 - c.P[n, m] * c.Q[n, m] * c.Exp[n, m]));

                    result[n, n][m] = prevA;

                    for (int k = n - 1; k >= r; k--)
                    {
                        var dk1 = (double)_model.GetLayerDepth(k + 1);

                        var q = ((1 + c.P[k + 1, m]) / (1 + c.P[k, m] * c.Exp[k, m])) *
                            Complex.Exp((c.Eta[k + 1, m] - c.Eta[k, m]) * dk1);

                        Complex nextA = q * prevA;

                        result[k, n][m] = nextA;
                        result[n, k][m] = nextA;

                        prevA = nextA;
                    }
                }
            }

            return result;
        }


        private Complex[,] CalculateExp(Complex[,] eta)
        {
            var length0 = eta.GetLength(0);
            var length1 = eta.GetLength(1);

            var exp = new Complex[length0, length1];

            for (int k = 0; k < length0; k++)
            {
                var thick = (double)_model.Section1D[k].Thickness;

                UnsafeNativeMethods.CalcExp(eta, k, -2 * thick, exp);
            }

            return exp;
        }

        private Complex[,] CalculateEta(double[] lambdas)
        {
            var iOmega = Complex.ImaginaryOne * _model.Omega;
            var eta = new Complex[_model.Section1D.NumberOfLayers, lambdas.Length];

            for (int k = 0; k < eta.GetLength(0); k++)
            {
                var zeta = _model.Section1D[k].Zeta;

                UnsafeNativeMethods.CalcEta(eta, k, lambdas, iOmega * zeta * Mu0);
            }

            return eta;
        }


        private Complex[,] CalculateP1(Complex[,] eta, Complex[,] exp)
        {
            var length0 = eta.GetLength(0);
            var length1 = eta.GetLength(1);

            var p = new Complex[length0, length1];

            for (int m = 0; m < length1; m++)
                p[1, m] = (eta[1, m] - eta[0, m]) / (eta[1, m] + eta[0, m]);

            CalculatePBelow1(p, eta, exp, _alphaBeta.Alpha1);

            return p;
        }

        private Complex[,] CalculatePSigma(Complex[] zeta, Complex[,] eta, Complex[,] exp)
        {
            var length0 = eta.GetLength(0);
            var length1 = eta.GetLength(1);

            var p = new Complex[length0, length1];

            for (int m = 0; m < length1; m++)
                p[1, m] = (zeta[0] * eta[1, m] - zeta[1] * eta[0, m]) / (zeta[0] * eta[1, m] + zeta[1] * eta[0, m]);

            CalculatePBelow1(p, eta, exp, _alphaBeta.AlphaSigma);

            return p;
        }

        private void CalculatePBelow1(Complex[,] p, Complex[,] eta, Complex[,] exp, Complex[] alpha)
        {
            var length0 = eta.GetLength(0);
            var length1 = eta.GetLength(1);

            for (int k = 1; k < length0 - 1; k++)
                for (int m = 0; m < length1; m++)
                {
                    var pExp = p[k, m] * exp[k, m];
                    var alphaEta = alpha[k] * (eta[k, m] / eta[k + 1, m]);
                    var part = alphaEta * ((pExp - 1) / (pExp + 1));

                    p[k + 1, m] = (1 + part) / (1 - part);
                }
        }


        private Complex[,] CalculateQ1(Complex[,] eta, Complex[,] exp)
        {
            return CalculateQ(eta, exp, _alphaBeta.Beta1);
        }

        private Complex[,] CalculateQSigma(Complex[,] eta, Complex[,] exp)
        {
            return CalculateQ(eta, exp, _alphaBeta.BetaSigma);
        }
        private Complex[,] CalculateQ(Complex[,] eta, Complex[,] exp, Complex[] betta)
        {
            var length0 = eta.GetLength(0);
            var length1 = eta.GetLength(1);

            var q = new Complex[length0, length1];
            int n = length0 - 1;

            for (int m = 0; m < length1; m++)
            {
                q[n, m] = 0;

                var part = betta[n] * (eta[n, m] / eta[n - 1, m]);
                q[n - 1, m] = (1 - part) / (1 + part);

                for (int k = n - 1; k > 1; k--)
                {
                    var qExp = q[k, m] * exp[k, m];
                    var betaEta = betta[k] * (eta[k, m] / eta[k - 1, m]);
                    part = betaEta * ((qExp - 1) / (qExp + 1));

                    q[k - 1, m] = (1 + part) / (1 - part);
                }
            }

            return q;
        }
    }
}
