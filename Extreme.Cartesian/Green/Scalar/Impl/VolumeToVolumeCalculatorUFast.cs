using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    internal class VolumeToVolumeCalculatorUFast : PlanCalculatorFast.ICalculatorU
    {
        private const double Mu0 = (4.0 * System.Math.PI * 1.0E-07);
        private readonly OmegaModel _model;
        private readonly AuxContainerFast _container;

        private int _currentLength;

        private Complex[] _calcT;
        private Complex[] _calcR;

        private Complex[] _calcCtop;
        private Complex[] _calcCbot;
        private Complex[] _calcDtop;
        private Complex[] _calcDbot;

        public VolumeToVolumeCalculatorUFast(OmegaModel model, AuxContainerFast container)
        {
            _model = model;
            _container = container;
        }

        private delegate Complex[] CalcGSpecial();
        private delegate Complex[] CalcPsi(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne);

        public void PreCalculate()
        {
            var ne = GetNativeEnvelop(_container);

            _calcT = UnsafeNativeMethods.CalculateT(ne, _container.Eta);
            _calcR = UnsafeNativeMethods.CalculateR(ne, _container.Eta);

            _calcCtop = UnsafeNativeMethods.CalculateCTop(ne, _container.Eta);
            _calcCbot = UnsafeNativeMethods.CalculateCBot(ne, _container.Eta);

            _calcDtop = UnsafeNativeMethods.CalculateDTop(ne, _container.Eta);
            _calcDbot = UnsafeNativeMethods.CalculateDBot(ne, _container.Eta);
        }

        public Complex[] CalculateU1()
        {
            return CalcUGeneral(
            rcAndTrMatch: () => CalculateU1And5Special(CalcPsi1RcAndTrMatch),
            rcBelowTr: () => CalculateU1And5Special(CalcPsi1And5RcBelowTr),
            rcAboveTr: () => CalculateU1And5Special(CalcPsi1And5RcAboveTr)
            );
        }

        public Complex[] CalculateU2()
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU2Special(CalcPsi1RcAndTrMatch),
           rcBelowTr: () => CalculateU2Special(CalcPsi1And5RcBelowTr),
           rcAboveTr: () => CalculateU2Special(CalcPsi1And5RcAboveTr));
        }

        public Complex[] CalculateU3()
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU3Special(CalcPsi3And4RcAndTrMatch),
           rcBelowTr: () => CalculateU3Special(CalcPsi3And4RcBelowTr),
           rcAboveTr: () => CalculateU3Special(CalcPsi3And4RcAboveTr));
        }

        public Complex[] CalculateU4()
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU4Special(CalcPsi3And4RcAndTrMatch),
           rcBelowTr: () => CalculateU4Special(CalcPsi3And4RcBelowTr),
           rcAboveTr: () => CalculateU4Special(CalcPsi3And4RcAboveTr));
        }

        public Complex[] CalculateU5(double[] lambdas)
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU5SpecialForTrAndRcMatch(lambdas),
           rcBelowTr: () => CalculateU1And5Special(CalcPsi1And5RcBelowTr),
           rcAboveTr: () => CalculateU1And5Special(CalcPsi1And5RcAboveTr));
        }


        private Complex[] CalcUGeneral(CalcGSpecial rcAndTrMatch, CalcGSpecial rcBelowTr, CalcGSpecial rcAboveTr)
        {
            _currentLength = _container.Eta.GetLength(1);

            var zWork = _container.Rc.GetWorkingDepth();
            var z1 = _container.Tr.StartingDepth;
            var z2 = _container.Tr.GetEndDepth();

            if (zWork > z1 && zWork < z2) return rcAndTrMatch();
            if (zWork >= z2) return rcBelowTr();
            if (zWork <= z1) return rcAboveTr();

            throw new NotImplementedException();
        }

        private NativeEnvelop GetNativeEnvelop(AuxContainerFast c)
        {
            var ne = new NativeEnvelop()
            {
                length = c.Eta.GetLength(1),
                r = c.CorrBackgroundRc,
                s = c.CorrBackgroundTr,

                r1 = (double)c.Rc.StartingDepth,
                r2 = (double)c.Rc.GetEndDepth(),
                s1 = (double)c.Tr.StartingDepth,
                s2 = (double)c.Tr.GetEndDepth(),
            };

            ne.t = System.Math.Min(ne.r, ne.s);
            ne.b = System.Math.Max(ne.r, ne.s);
            ne.dt = (double)_model.GetLayerDepth(ne.t);
            ne.db1 = (double)_model.GetLayerDepth(ne.b + 1);

            return ne;
        }

        private Complex[] CalculateSpecial(double pSign, double psiSign, CalcPsi calcPsi)
        {
            var c = _container;
            var ne = GetNativeEnvelop(c);

            var pAddendum = _calcT;//UnsafeNativeMethods.CalculateT(ne, c.Eta);
            var qAddendum = _calcR;// UnsafeNativeMethods.CalculateR(ne, c.Eta);
            var psi = calcPsi(c.Eta, c.P, c.Q, ne);

            var result = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
                result[i] = pSign * c.P[ne.t, i] * pAddendum[i] + c.Q[ne.b, i] * qAddendum[i] + psiSign * psi[i];

            return result;
        }

        private Complex[] CalculateU1And5Special(CalcPsi calcPsi)
        {
            var c = _container;

            var result = new Complex[_currentLength];
            var spec = CalculateSpecial(1, 1, calcPsi);
            var ann = GetAnn();

            for (int i = 0; i < _currentLength; i++)
            {
                var annFactor = 1 / (c.Eta[c.CorrBackgroundRc, i] * c.Eta[c.CorrBackgroundTr, i]);
                result[i] = ann[i] * annFactor * spec[i];
            }

            return result;
        }

        private Complex[] CalculateU5SpecialForTrAndRcMatch(double[] lambdas)
        {
            var c = _container;
            var ne = GetNativeEnvelop(c);

            var ann = GetAnn();

            var pAddendum = _calcT;// UnsafeNativeMethods.CalculateT(ne, c.Eta);
            var qAddendum = _calcR;//UnsafeNativeMethods.CalculateR(ne, c.Eta);
            var psi = CalcPsi5RcAndTrMatch(c.Eta, c.P, c.Q, ne, lambdas);

            var result = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
            {
                var annFactor = 1 / (c.Eta[c.CorrBackgroundRc, i] * c.Eta[c.CorrBackgroundTr, i]);
                result[i] = (ann[i] * annFactor) * (c.P[ne.t, i] * pAddendum[i] + c.Q[ne.b, i] * qAddendum[i] + psi[i]);
            }

            return result;
        }

        private Complex[] CalculateU2Special(CalcPsi calcPsi)
        {
            var result = new Complex[_currentLength];
            var ann = GetAnn();
            var spec = CalculateSpecial(1, -1, calcPsi);

            for (int i = 0; i < _currentLength; i++)
                result[i] = ann[i] * spec[i];

            return result;
        }

        private Complex[] CalculateU3Special(CalcPsi calcPsi)
        {
            var c = _container;

            var result = new Complex[_currentLength];
            var ann = GetAnn();
            var spec = CalculateSpecial(-1, 1, calcPsi);

            for (int i = 0; i < _currentLength; i++)
            {
                var annFactor = 1 / (c.Eta[c.CorrBackgroundTr, i]);
                result[i] = ann[i] * annFactor * spec[i]; ;
            }

            return result;
        }

        private Complex[] CalculateU4Special(CalcPsi calcPsi)
        {
            var c = _container;

            var result = new Complex[_currentLength];
            var ann = GetAnn();
            var spec = CalculateSpecial(-1, -1, calcPsi);

            for (int i = 0; i < _currentLength; i++)
            {
                var annFactor = 1 / (c.Eta[c.CorrBackgroundRc, i]);
                result[i] = ann[i] * annFactor * spec[i]; ;
            }

            return result;
        }

        private Complex[] CalcPsi1And5RcAboveTr(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne)
        {
            var ctop = _calcCtop;//UnsafeNativeMethods.CalculateCTop(ne, eta);
            var dtop = _calcDtop; //UnsafeNativeMethods.CalculateDTop(ne, eta);

            var psi1 = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
                psi1[i] = ctop[i] + p[ne.t, i] * q[ne.b, i] * dtop[i];

            return psi1;
        }

        private Complex[] CalcPsi1And5RcBelowTr(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne)
        {
            var cbot = _calcCbot;// UnsafeNativeMethods.CalculateCBot(ne, eta);
            var dbot = _calcDbot;//UnsafeNativeMethods.CalculateDBot(ne, eta);

            var psi1 = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
                psi1[i] = cbot[i] + p[ne.t, i] * q[ne.b, i] * dbot[i];

            return psi1;
        }

        private Complex[] CalcPsi3And4RcAndTrMatch(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne)
        {
            var psi = new Complex[ne.length];
            Array.Clear(psi, 0, psi.Length);
            return psi;
        }

        private Complex[] CalcPsi3And4RcAboveTr(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne)
        {
            var ctop = _calcCtop;// UnsafeNativeMethods.CalculateCTop(ne, eta);
            var dtop = _calcDtop;// UnsafeNativeMethods.CalculateDTop(ne, eta);

            var psi3 = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
                psi3[i] = ctop[i] - p[ne.t, i] * q[ne.b, i] * dtop[i];

            return psi3;
        }

        private Complex[] CalcPsi3And4RcBelowTr(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne)
        {
            var cbot = _calcCbot;//UnsafeNativeMethods.CalculateCBot(ne, eta);
            var dbot = _calcDbot;//UnsafeNativeMethods.CalculateDBot(ne, eta);

            var psi3 = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
                psi3[i] = -cbot[i] + p[ne.t, i] * q[ne.b, i] * dbot[i];

            return psi3;
        }

        private static Func<Complex, Complex> exp = Complex.Exp;

        private Complex[] CalcPsi5RcAndTrMatch(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne, double[] lambdas)
        {
            var n = ne.r;
            var tau = ne.r2 - ne.r1;
            var ln = (double)_model.Section1D[n].Thickness;

            var layer = _model.Section1D[n];
            var kk = new Complex(0, _model.Omega) * layer.Zeta * Mu0;

            var psi5 = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
            {
                var part1 = -(1 - exp(-eta[n, i] * tau));
                var part2 = p[n, i] * q[n, i] * (exp(-eta[n, i] * (2 * ln - tau)) - exp(-2 * eta[n, i] * ln));
                var part3 = (kk / (lambdas[i] * lambdas[i])) * (1 - p[n, i] * q[n, i] * exp(-2 * eta[n, i] * ln)) * tau * eta[n, i];

                psi5[i] = 2 * (part1 + part2 + part3);
            }

            return psi5;
        }

        private Complex[] CalcPsi1RcAndTrMatch(Complex[,] eta, Complex[,] p, Complex[,] q, NativeEnvelop ne)
        {
            var tau = ne.r2 - ne.r1;
            var n = ne.r;
            var dn1 = (double)_model.GetLayerDepth(n + 1);

            var psi1 = new Complex[_currentLength];

            for (int i = 0; i < _currentLength; i++)
            {
                var part1 = tau * eta[n, i] - (1 - exp(-eta[n, i] * tau));
                var part2 = p[n, i] * q[n, i] *
                     ((tau * eta[n, i] + 1) * exp(-2 * eta[n, i] * dn1) -
                                           exp(-2 * eta[n, i] * dn1 + eta[n, i] * tau));

                psi1[i] = 2 * (part1 - part2);
            }

            return psi1;
        }

        private Complex[] GetAnn()
        {
            return _container.A[_container.CorrBackgroundRc, _container.CorrBackgroundTr];
        }
    }
}
