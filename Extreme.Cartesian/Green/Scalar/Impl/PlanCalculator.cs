using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Cartesian.Green.Scalar;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public class VolumeToVolumePlanCalculator : PlanCalculator
    {
        public VolumeToVolumePlanCalculator(OmegaModel model, AlphaBeta alphaBeta, ScalarPlanItem planItem)
            : base(model, alphaBeta, planItem)
        {
        }

        protected override ICalculatorU GetCalculatorU(OmegaModel model, AuxContainer c)
        {
            return new VolumeToVolumeCalculatorU(model, c);
        }
    }

    public class VolumeToPointPlanCalculator : PlanCalculator
    {
        public VolumeToPointPlanCalculator(OmegaModel model, AlphaBeta alphaBeta, ScalarPlanItem planItem)
            : base(model, alphaBeta, planItem)
        {
        }

        protected override ICalculatorU GetCalculatorU(OmegaModel model, AuxContainer c)
        {
            return new VolumeToPointCalculatorU(model, c);
        }
    }

    public abstract class PlanCalculator : IPlanCalculator
    {
        public const double Mu0 = (4.0 * System.Math.PI * 1.0E-07);

        public interface ICalculatorU
        {
            Complex CalculateU1();
            Complex CalculateU2();
            Complex CalculateU3();
            Complex CalculateU4();
            Complex CalculateU5(double lambda);
        }

        private readonly OmegaModel _model;
        private readonly AlphaBeta _alphaBeta;
        private readonly ScalarPlanItem _planItem;

        protected PlanCalculator(OmegaModel model, AlphaBeta alphaBeta, ScalarPlanItem planItem)
        {
            _model = model;
            _alphaBeta = alphaBeta;
            _planItem = planItem;
        }

        protected abstract ICalculatorU GetCalculatorU(OmegaModel model, AuxContainer c);

        public InnerResult CalculateForE(Transmitter transmitter, Receiver receiver)
        {
            var c = AuxContainer.CreateContainer(_model, transmitter, receiver);
            var uCalculator = GetCalculatorU(_model, c);
            var result = CalculateForE(c, uCalculator);

            return result;
        }

        public InnerResult CalculateForH(Transmitter transmitter, Receiver receiver)
        {
            var c = AuxContainer.CreateContainer(_model, transmitter, receiver);
            var uCalculator = GetCalculatorU(_model, c);
            var result = CalculateForH(c, uCalculator);

            return result;
        }

        private InnerResult CalculateForE(AuxContainer c, ICalculatorU u)
        {
            var e = InitResultE(_planItem);
            var zeta = _model.Section1D.GetAllSection1DZeta();
            var length = _planItem.Lambdas.Length;

            for (int i = 0; i < length; i++)
            {
                var lambda = _planItem.Lambdas[i];

                c.Eta = CalculateEta(_model, lambda);
                c.Exp = CalculateExp(_model, c.Eta);

                if (_planItem.CalculateI1)
                {
                    c.P = CalculateP1(_alphaBeta, c.Eta, c.Exp);
                    c.Q = CalculateQ1(_alphaBeta, c.Eta, c.Exp);
                    c.A = CalculateA1(_model, c);
                    e.U1[i] = u.CalculateU1();
                }

                c.P = CalculatePSigma(zeta, c.Eta, c.Exp);
                c.Q = CalculateQSigma(c.Eta, c.Exp);
                c.A = CalculateASigma(_model, c);

                if (_planItem.CalculateI2) e.U2[i] = u.CalculateU2();
                if (_planItem.CalculateI3) e.U3[i] = u.CalculateU3();
                if (_planItem.CalculateI4) e.U4[i] = u.CalculateU4();
                if (_planItem.CalculateI5) e.U5[i] = u.CalculateU5(lambda);
            }

            var result = ScalarMathUtils.NormalizeAndPerformHankel(e, _planItem, _model, c.CorrBackgroundRc);
            return result;
        }

        private InnerResult CalculateForH(AuxContainer c, ICalculatorU u)
        {
            var h = InitResultH(_planItem);
            var zeta = _model.Section1D.GetAllSection1DZeta();
            var length = _planItem.Lambdas.Length;

            for (int i = 0; i < length; i++)
            {
                c.Eta = CalculateEta(_model, _planItem.Lambdas[i]);
                c.Exp = CalculateExp(_model, c.Eta);

                c.P = CalculateP1(_alphaBeta, c.Eta, c.Exp);
                c.Q = CalculateQ1(_alphaBeta, c.Eta, c.Exp);
                c.A = CalculateA1(_model, c);
                if (_planItem.CalculateI4) h.U11[i] = u.CalculateU1();
                if (_planItem.CalculateI1) h.U31[i] = u.CalculateU3();

                c.P = CalculatePSigma(zeta, c.Eta, c.Exp);
                c.Q = CalculateQSigma(c.Eta, c.Exp);
                c.A = CalculateASigma(_model, c);
                if (_planItem.CalculateI3) h.U1Sigma[i] = u.CalculateU1();
                if (_planItem.CalculateI2) h.U4Sigma[i] = u.CalculateU4();
            }

            var result = ScalarMathUtils.NormalizeAndPerformHankel(h, _planItem);
            return result;
        }

        private static ResultE InitResultE(ScalarPlanItem planItem)
        {
            var length = planItem.Lambdas.Length;

            var r = new ResultE
            {
                U1 = planItem.CalculateI1 ? new Complex[length] : new Complex[0],
                U2 = planItem.CalculateI2 ? new Complex[length] : new Complex[0],
                U3 = planItem.CalculateI3 ? new Complex[length] : new Complex[0],
                U4 = planItem.CalculateI4 ? new Complex[length] : new Complex[0],
                U5 = planItem.CalculateI5 ? new Complex[length] : new Complex[0],
            };

            return r;
        }

        private static ResultH InitResultH(ScalarPlanItem plan)
        {
            var length = plan.Lambdas.Length;

            var r = new ResultH
            {
                U11 = new Complex[length],
                U1Sigma = new Complex[length],
                U31 = new Complex[length],
                U4Sigma = new Complex[length],
            };
            return r;
        }

        public static Complex[,] CalculateA1(OmegaModel model, AuxContainer c) 
            => CalculateA(model, c);

        private static Complex[,] CalculateASigma(OmegaModel model, AuxContainer c)
        {
            var a = CalculateA(model, c);

            int s = System.Math.Max(c.CorrBackgroundRc, c.CorrBackgroundTr);
            int r = System.Math.Min(c.CorrBackgroundRc, c.CorrBackgroundTr);

            for (int n = s; n >= r; n--)
                for (int k = n - 1; k >= r; k--)
                {
                    var zetaRc = model.Section1D[k].Zeta;
                    var zetaTr = model.Section1D[n].Zeta;

                    a[n, k] = (zetaTr / zetaRc) * a[k, n];
                }

            return a;
        }

        private static Complex[,] CalculateA(OmegaModel model, AuxContainer c)
        {
            var result = new Complex[c.P.Length, c.P.Length];

            int s = System.Math.Max(c.CorrBackgroundRc, c.CorrBackgroundTr);
            int r = System.Math.Min(c.CorrBackgroundRc, c.CorrBackgroundTr);

            for (int n = s; n >= r; n--)
            {
                Complex prevA = 1.0 / (c.Eta[n] * (1 - c.P[n] * c.Q[n] * c.Exp[n]));

                result[n, n] = prevA;

                for (int k = n - 1; k >= r; k--)
                {
                    var dk1 = (double)model.GetLayerDepth(k + 1);

                    var q = ((1 + c.P[k + 1]) / (1 + c.P[k] * c.Exp[k])) *
                        Complex.Exp((c.Eta[k + 1] - c.Eta[k]) * dk1);

                    Complex nextA = q * prevA;

                    result[k, n] = nextA;
                    result[n, k] = nextA;

                    prevA = nextA;
                }
            }

            return result;
        }

        public static Complex[] CalculateEta(OmegaModel model, double lambda)
        {
            var iOmega = Complex.ImaginaryOne * model.Omega;
            var eta = new Complex[model.Section1D.NumberOfLayers];

            for (int k = 0; k < eta.Length; k++)
            {
                var zeta = model.Section1D[k].Zeta;
                eta[k] = Complex.Sqrt(lambda * lambda - iOmega * zeta * Mu0);
            }

            return eta;
        }

        public static Complex[] CalculateExp(OmegaModel model, Complex[] eta)
        {
            var exp = new Complex[eta.Length];

            for (int k = 0; k < exp.Length; k++)
            {
                var thick = model.Section1D[k].Thickness;
                exp[k] = Complex.Exp(-2 * eta[k] * (double)thick);
            }

            return exp;
        }

        public static Complex[] CalculateP1(AlphaBeta alphaBeta, Complex[] eta, Complex[] exp)
        {
            var alpha = alphaBeta.Alpha1;
            var p1 = (eta[1] - eta[0]) / (eta[1] + eta[0]);

            return CalculatePBelow1(p1, eta, exp, alpha);
        }

        private Complex[] CalculatePSigma(Complex[] zeta, Complex[] eta, Complex[] exp)
        {
            var alpha = _alphaBeta.AlphaSigma;
            var p1 = (zeta[0] * eta[1] - zeta[1] * eta[0]) / (zeta[0] * eta[1] + zeta[1] * eta[0]);

            return CalculatePBelow1(p1, eta, exp, alpha);
        }

        private static Complex[] CalculatePBelow1(Complex p1, Complex[] eta, Complex[] exp, Complex[] alpha)
        {
            var p = new Complex[eta.Length];

            p[1] = p1;

            for (int k = 1; k < eta.Length - 1; k++)
            {
                var pExp = p[k] * exp[k];
                var alphaEta = alpha[k] * (eta[k] / eta[k + 1]);
                var part = alphaEta * ((pExp - 1) / (pExp + 1));

                p[k + 1] = (1 + part) / (1 - part);
            }

            return p;
        }

        public static Complex[] CalculateQ1(AlphaBeta alphaBeta, Complex[] eta, Complex[] exp) 
            => CalculateQ(eta, exp, alphaBeta.Beta1);

        private Complex[] CalculateQSigma(Complex[] eta, Complex[] exp) 
            => CalculateQ(eta, exp, _alphaBeta.BetaSigma);

        private static Complex[] CalculateQ(Complex[] eta, Complex[] exp, Complex[] betta)
        {
            var q = new Complex[eta.Length];
            int n = eta.Length - 1;

            q[n] = 0;

            var part = betta[n] * (eta[n] / eta[n - 1]);
            q[n - 1] = (1 - part) / (1 + part);

            for (int k = n - 1; k > 1; k--)
            {
                var qExp = q[k] * exp[k];
                var betaEta = betta[k] * (eta[k] / eta[k - 1]);
                part = betaEta * ((qExp - 1) / (qExp + 1));

                q[k - 1] = (1 + part) / (1 - part);
            }

            return q;
        }
    }
}
