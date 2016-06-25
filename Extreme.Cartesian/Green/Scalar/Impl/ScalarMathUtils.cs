//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Cartesian.Green.Scalar;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public static class ScalarMathUtils
    {
        public const double Mu0 = (4.0 * System.Math.PI * 1.0E-07);

        public static int DetermineCorrespondingBackgroundLayer(this OmegaModel model, decimal depth)
        {
            decimal prev = model.Section1D.ZeroAirLevelAlongZ;

            for (int k = 0; k < model.Section1D.NumberOfLayers; k++)
            {
                var d1 = prev;
                var d2 = prev + model.Section1D[k].Thickness;

                if (depth >= d1 && depth < d2)
                    return k;

                prev = d2;
            }

            throw new InvalidOperationException("Anomaly is not within 1D section");
        }

        public static void CalculateZeroRhoForH(InnerResult target, double rhoStep)
        {
            var rho = target.Rho.ToList();
            var i1 = target.I1.ToList();
            var i2 = target.I2.ToList();
            var i3 = target.I3.ToList();
            var i4 = target.I4.ToList();

            var alphaPow2 = rhoStep * rhoStep;

            Func<List<Complex>, Complex> interpolate =
                (f) => (f[1] * alphaPow2 - f[2]) / (alphaPow2 - 1);

            rho.Insert(0, 0);
            i1.Insert(0, 0);
            i2.Insert(0, 0);
            i3.Insert(0, interpolate(i3));
            i4.Insert(0, interpolate(i4));

            target.Rho = rho.ToArray();
            target.I1 = i1.ToArray();
            target.I2 = i2.ToArray();
            target.I3 = i3.ToArray();
            target.I4 = i4.ToArray();
        }

        public static void CalculateZeroRhoForE(InnerResult target, double rhoStep)
        {
            var rho = target.Rho.ToList();
            var i1 = target.I1.ToList();
            var i2 = target.I2.ToList();
            var i3 = target.I3.ToList();
            var i4 = target.I4.ToList();
            var i5 = target.I5.ToList();

            var alphaPow2 = rhoStep * rhoStep;

            Func<List<Complex>, Complex> interpolate =
                (f) => (f[1] * alphaPow2 - f[2]) / (alphaPow2 - 1);

            rho.Insert(0, 0);
            i1.Insert(0, 0);
            i2.Insert(0, 0);
            i3.Insert(0, interpolate(i3));
            i4.Insert(0, interpolate(i4));
            i5.Insert(0, 0);

            target.Rho = rho.ToArray();
            target.I1 = i1.ToArray();
            target.I2 = i2.ToArray();
            target.I3 = i3.ToArray();
            target.I4 = i4.ToArray();
            target.I5 = i5.ToArray();
        }

        #region Hankel

        public static InnerResult NormalizeAndPerformHankelFast(ResultH h, ScalarPlanItem planItem)
        {
            NormalizeBeforeHankelForH(h, planItem);
            var r = CalculateHankelForHFast(planItem, h);
            NormalizeAfterHankelForH(planItem, r);

            return r;
        }

        public static InnerResult NormalizeAndPerformHankelFast(ResultE e, ScalarPlanItem planItem, OmegaModel model, int corrBackgroundRc)
        {
            NormalizeBeforeHankelForE(model, corrBackgroundRc, e, planItem);
            var r = CalculateHankelForEFast(planItem, e);
            NormalizeAfterHankelForE(planItem, r);

            return r;
        }

        public static InnerResult NormalizeAndPerformHankel(ResultH h, ScalarPlanItem planItem)
        {
            NormalizeBeforeHankelForH(h, planItem);
            var r = CalculateHankelForH(planItem, h);
            NormalizeAfterHankelForH(planItem, r);

            return r;
        }

        public static InnerResult NormalizeAndPerformHankel(ResultE e, ScalarPlanItem planItem, OmegaModel model, int corrBackgroundRc)
        {
            NormalizeBeforeHankelForE(model, corrBackgroundRc, e, planItem);
            var r = CalculateHankelForE(planItem, e);
            NormalizeAfterHankelForE(planItem, r);

            return r;
        }

        private static void NormalizeAfterHankelForH(ScalarPlanItem item, InnerResult r)
        {
            const double pi2 = 2 * System.Math.PI;

            for (int i = 0; i < r.I1.Length; i++)
                r.I1[i] = -r.I1[i] / (2 * pi2 * item.Rho[i]);

            for (int i = 0; i < r.I2.Length; i++)
                r.I2[i] = -r.I2[i] / (2 * pi2 * item.Rho[i]);

            for (int i = 0; i < r.I3.Length; i++)
                r.I3[i] = -r.I3[i] / (2 * pi2 * item.Rho[i]);

            for (int i = 0; i < r.I4.Length; i++)
                r.I4[i] = r.I4[i] / (2 * pi2 * item.Rho[i]);
        }

        private static InnerResult CalculateHankelForH(ScalarPlanItem item, ResultH r)
        {
            var hankel = item.HankelCoefficients;
            var length = item.Rho.Length;

            return new InnerResult
            {
                Rho = item.Rho,

                I1 = item.CalculateI1 ? hankel.ConvoluteWithHank1(r.U31, length) : new Complex[0],
                I2 = item.CalculateI2 ? hankel.ConvoluteWithHank1(r.U4Sigma, length) : new Complex[0],
                I3 = item.CalculateI3 ? hankel.ConvoluteWithHank0(r.U1Sigma, length) : new Complex[0],
                I4 = item.CalculateI4 ? hankel.ConvoluteWithHank0(r.U11, length) : new Complex[0],
                I5 = new Complex[0],
            };
        }


        private static InnerResult CalculateHankelForHFast(ScalarPlanItem item, ResultH r)
        {
            var hankel = item.HankelCoefficients;
            var length = item.Rho.Length;

            return new InnerResult
            {
                Rho = item.Rho,

                I1 = item.CalculateI1 ? hankel.ConvoluteWithHank1Fast(r.U31, length) : new Complex[0],
                I2 = item.CalculateI2 ? hankel.ConvoluteWithHank1Fast(r.U4Sigma, length) : new Complex[0],
                I3 = item.CalculateI3 ? hankel.ConvoluteWithHank0Fast(r.U1Sigma, length) : new Complex[0],
                I4 = item.CalculateI4 ? hankel.ConvoluteWithHank0Fast(r.U11, length) : new Complex[0],
            };
        }

        private static void NormalizeBeforeHankelForH(ResultH r, ScalarPlanItem item)
        {
            var lambdas = item.Lambdas;

            for (int i = 0; i < r.U1Sigma.Length; i++)
                r.U1Sigma[i] *= lambdas[i];

            for (int i = 0; i < r.U11.Length; i++)
                r.U11[i] *= lambdas[i];
        }


        private static void NormalizeAfterHankelForE(ScalarPlanItem item, InnerResult r)
        {
            const double pi2 = 2 * System.Math.PI;

            for (int i = 0; i < r.I1.Length; i++)
                r.I1[i] = -r.I1[i] / (pi2 * item.Rho[i]);

            for (int i = 0; i < r.I2.Length; i++)
                r.I2[i] = r.I2[i] / (pi2 * item.Rho[i]);

            for (int i = 0; i < r.I3.Length; i++)
                r.I3[i] = r.I3[i] / (pi2 * item.Rho[i]);

            for (int i = 0; i < r.I4.Length; i++)
                r.I4[i] = r.I4[i] / (pi2 * item.Rho[i]);

            for (int i = 0; i < r.I5.Length; i++)
                r.I5[i] = r.I5[i] / (pi2 * item.Rho[i]);
        }

        private static InnerResult CalculateHankelForE(ScalarPlanItem item, ResultE r)
        {
            var hankel = item.HankelCoefficients;
            var length = item.Rho.Length;

            return new InnerResult
            {
                Rho = item.Rho,

                I1 = item.CalculateI1 ? hankel.ConvoluteWithHank1(r.U1, length) : new Complex[0],
                I2 = item.CalculateI2 ? hankel.ConvoluteWithHank1(r.U2, length) : new Complex[0],
                I3 = item.CalculateI3 ? hankel.ConvoluteWithHank0(r.U3, length) : new Complex[0],
                I4 = item.CalculateI4 ? hankel.ConvoluteWithHank0(r.U4, length) : new Complex[0],
                I5 = item.CalculateI5 ? hankel.ConvoluteWithHank1(r.U5, length) : new Complex[0],
            };
        }

        private static InnerResult CalculateHankelForEFast(ScalarPlanItem item, ResultE r)
        {
            var hankel = item.HankelCoefficients;
            var length = item.Rho.Length;

            return new InnerResult
            {
                Rho = item.Rho,

                I1 = item.CalculateI1 ? hankel.ConvoluteWithHank1Fast(r.U1, length) : new Complex[0],
                I2 = item.CalculateI2 ? hankel.ConvoluteWithHank1Fast(r.U2, length) : new Complex[0],
                I3 = item.CalculateI3 ? hankel.ConvoluteWithHank0Fast(r.U3, length) : new Complex[0],
                I4 = item.CalculateI4 ? hankel.ConvoluteWithHank0Fast(r.U4, length) : new Complex[0],
                I5 = item.CalculateI5 ? hankel.ConvoluteWithHank1Fast(r.U5, length) : new Complex[0],
            };
        }

        private static void NormalizeBeforeHankelForE(OmegaModel model, int corrBackgroundRc, ResultE r, ScalarPlanItem item)
        {
            var lambdas = item.Lambdas;
            var zetaRc2 = 2 * model.Section1D[corrBackgroundRc].Zeta;
            var iOmegaMu0 = Complex.ImaginaryOne * model.Omega * Mu0;

            for (int i = 0; i < r.U1.Length; i++)
                r.U1[i] *= (iOmegaMu0 / 2);

            for (int i = 0; i < r.U2.Length; i++)
                r.U2[i] /= zetaRc2;

            for (int i = 0; i < r.U3.Length; i++)
                r.U3[i] *= lambdas[i] / zetaRc2;

            for (int i = 0; i < r.U4.Length; i++)
                r.U4[i] *= -lambdas[i] / zetaRc2;

            for (int i = 0; i < r.U5.Length; i++)
                r.U5[i] *= (lambdas[i] * lambdas[i]) / zetaRc2;
        }

        #endregion
    }
}
