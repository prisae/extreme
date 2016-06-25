//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    internal class VolumeToVolumeCalculatorU : PlanCalculator.ICalculatorU
    {
        private const double Mu0 = (4.0 * System.Math.PI * 1.0E-07);
        private readonly OmegaModel _model;
        private readonly AuxContainer _container;

        public VolumeToVolumeCalculatorU(OmegaModel model, AuxContainer container)
        {
            _model = model;
            _container = container;
        }

        private class Z
        {
            public double r1;
            public double r2;
            public double s1;
            public double s2;
        }

        public Complex CalculateU1()
        {
            return CalcUGeneral(
            rcAndTrMatch: () => CalculateU1And5Special(CalcPsi1RcAndTrMatch),
            rcBelowTr: () => CalculateU1And5Special(CalcPsi1And5RcBelowTr),
            rcAboveTr: () => CalculateU1And5Special(CalcPsi1And5RcAboveTr));
        }

        public Complex CalculateU2()
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU2Special(CalcPsi1RcAndTrMatch),
           rcBelowTr: () => CalculateU2Special(CalcPsi1And5RcBelowTr),
           rcAboveTr: () => CalculateU2Special(CalcPsi1And5RcAboveTr));
        }

        public Complex CalculateU3()
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU3Special(CalcPsi3And4RcAndTrMatch),
           rcBelowTr: () => CalculateU3Special(CalcPsi3And4RcBelowTr),
           rcAboveTr: () => CalculateU3Special(CalcPsi3And4RcAboveTr));
        }

        public Complex CalculateU4()
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU4Special(CalcPsi3And4RcAndTrMatch),
           rcBelowTr: () => CalculateU4Special(CalcPsi3And4RcBelowTr),
           rcAboveTr: () => CalculateU4Special(CalcPsi3And4RcAboveTr));
        }

        public Complex CalculateU5(double lambda)
        {
            return CalcUGeneral(
           rcAndTrMatch: () => CalculateU5SpecialForTrAndRcMatch(lambda),
           rcBelowTr: () => CalculateU1And5Special(CalcPsi1And5RcBelowTr),
           rcAboveTr: () => CalculateU1And5Special(CalcPsi1And5RcAboveTr));
        }

        private delegate Complex CalcGSpecial();

        private Complex CalcUGeneral(CalcGSpecial rcAndTrMatch, CalcGSpecial rcBelowTr, CalcGSpecial rcAboveTr)
        {
            var c = _container;

            var zWork = c.Rc.GetWorkingDepth();
            var z1 = c.Tr.StartingDepth;
            var z2 = c.Tr.GetEndDepth();

            if (zWork > z1 && zWork < z2) return rcAndTrMatch();
            if (zWork >= z2) return rcBelowTr();
            if (zWork <= z1) return rcAboveTr();

            throw new NotImplementedException();
        }

        private Z GetZ(AuxContainer c)
        {
            return new Z
            {
                s1 = (double)c.Tr.StartingDepth,
                s2 = (double)c.Tr.GetEndDepth(),
                r1 = (double)c.Rc.StartingDepth,
                r2 = (double)c.Rc.GetEndDepth()
            };
        }

        private Complex CalculateSpecial(double pSign, double psiSign, CalcPsi calcPsi)
        {
            var c = _container;

            int r = c.CorrBackgroundRc;
            int s = c.CorrBackgroundTr;
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var z = GetZ(c);

            var pAddendum = CalculateT(z, c.Eta, r, s);
            var qAddendum = CalculateR(z, c.Eta, r, s);
            var psi = calcPsi(c.Eta, c.P, c.Q, z, r, s);

            return (pSign * c.P[t] * pAddendum + c.Q[b] * qAddendum + psiSign * psi);
        }

        private Complex CalculateU1And5Special(CalcPsi calcPsi)
        {
            var c = _container;
            var annFactor = 1 / (c.Eta[c.CorrBackgroundRc] * c.Eta[c.CorrBackgroundTr]);

            return GetAnn()*annFactor* CalculateSpecial(1, 1, calcPsi);
        }


        private Complex CalculateU5SpecialForTrAndRcMatch(double lambda)
        {
            var c = _container;

            int r = c.CorrBackgroundRc;
            int s = c.CorrBackgroundTr;
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var ann = GetAnn();
            var z = GetZ(c);

            var pAddendum = CalculateT(z, c.Eta, r, s);
            var qAddendum = CalculateR(z, c.Eta, r, s);
            var psi = CalcPsi5RcAndTrMatch(c.Eta, c.P, c.Q, z, r, lambda * lambda);

            var annFactor = 1 / (c.Eta[c.CorrBackgroundRc] * c.Eta[c.CorrBackgroundTr]);

            return (ann * annFactor) * (c.P[t] * pAddendum + c.Q[b] * qAddendum + psi);
        }

        private Complex CalculateU2Special(CalcPsi calcPsi)
        {
            var annFactor = 1;

            return GetAnn() * annFactor * CalculateSpecial(1, -1, calcPsi);
        }

        private Complex CalculateU3Special(CalcPsi calcPsi)
        {
            var annFactor = 1 / (_container.Eta[_container.CorrBackgroundTr]);

            return GetAnn() * annFactor * CalculateSpecial(-1, 1, calcPsi);
        }

        private Complex CalculateU4Special(CalcPsi calcPsi)
        {
            var annFactor = 1 / (_container.Eta[_container.CorrBackgroundRc]);

            return GetAnn() * annFactor * CalculateSpecial(-1, -1, calcPsi);
        }

        private Complex GetAnn()
        {
            return _container.A[_container.CorrBackgroundRc, _container.CorrBackgroundTr];
        }
        
        private static Func<Complex, Complex> exp = Complex.Exp;

        private Complex CalculateT(Z z, Complex[] eta, int r, int s)
        {
            int t = System.Math.Min(r, s);
            var dt = (double)_model.GetLayerDepth(t);

            return exp(-eta[r] * z.r1 - eta[s] * z.s1 + 2 * eta[t] * dt) -
                   exp(-eta[r] * z.r1 - eta[s] * z.s2 + 2 * eta[t] * dt) -
                   exp(-eta[r] * z.r2 - eta[s] * z.s1 + 2 * eta[t] * dt) +
                   exp(-eta[r] * z.r2 - eta[s] * z.s2 + 2 * eta[t] * dt);
        }
       
     
        private Complex CalculateR(Z z, Complex[] eta, int r, int s)
        {
            int b = System.Math.Max(r, s);
            var db1 = (double)_model.GetLayerDepth(b + 1);

            return exp(-2 * eta[b] * db1 + eta[s] * z.s2 + eta[r] * z.r2) -
                   exp(-2 * eta[b] * db1 + eta[s] * z.s1 + eta[r] * z.r2) -
                   exp(-2 * eta[b] * db1 + eta[s] * z.s2 + eta[r] * z.r1) +
                   exp(-2 * eta[b] * db1 + eta[s] * z.s1 + eta[r] * z.r1);
        }

        private Complex CalculateCTop(Z z, Complex[] eta, int r, int s)
        {
            return exp(-eta[s] * z.s1 + eta[r] * z.r2) -
                   exp(-eta[s] * z.s2 + eta[r] * z.r2) -
                   exp(-eta[s] * z.s1 + eta[r] * z.r1) +
                   exp(-eta[s] * z.s2 + eta[r] * z.r1);
        }

        private Complex CalculateCBot(Z z, Complex[] eta, int r, int s)
        {
            return exp(-eta[r] * z.r1 + eta[s] * z.s2) -
                   exp(-eta[r] * z.r1 + eta[s] * z.s1) -
                   exp(-eta[r] * z.r2 + eta[s] * z.s2) +
                   exp(-eta[r] * z.r2 + eta[s] * z.s1);
        }

        private Complex CalculateDTop(Z z, Complex[] eta, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);
            var dt = (double)_model.GetLayerDepth(t);
            var db1 = (double)_model.GetLayerDepth(b + 1);

            var firstAddendum = -2 * (eta[b] * db1 - eta[t] * dt);

            return exp(firstAddendum + eta[s] * z.s2 - eta[r] * z.r1) -
                   exp(firstAddendum + eta[s] * z.s1 - eta[r] * z.r1) -
                   exp(firstAddendum + eta[s] * z.s2 - eta[r] * z.r2) +
                   exp(firstAddendum + eta[s] * z.s1 - eta[r] * z.r2);
        }

        private Complex CalculateDBot(Z z, Complex[] eta, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);
            var dt = (double)_model.GetLayerDepth(t);
            var db1 = (double)_model.GetLayerDepth(b + 1);

            var firstAddendum = -2 * (eta[b] * db1 - eta[t] * dt);

            return exp(firstAddendum - eta[s] * z.s1 + eta[r] * z.r2) -
                   exp(firstAddendum - eta[s] * z.s2 + eta[r] * z.r2) -
                   exp(firstAddendum - eta[s] * z.s1 + eta[r] * z.r1) +
                   exp(firstAddendum - eta[s] * z.s2 + eta[r] * z.r1);
        }

        private delegate Complex CalcPsi(Complex[] eta, Complex[] p, Complex[] q, Z z, int r, int s);

        private Complex CalcPsi1RcAndTrMatch(Complex[] eta, Complex[] p, Complex[] q, Z z, int r, int s)
        {
            var tau = z.r2 - z.r1;
            var n = r;
            var dn1 = (double)_model.GetLayerDepth(n + 1);

            var part1 = tau * eta[n] - (1 - exp(-eta[n] * tau));
            var part2 = p[n] * q[n] *
                 ((tau * eta[n] + 1) * exp(-2 * eta[n] * dn1) -
                                       exp(-2 * eta[n] * dn1 + eta[n] * tau));

            var psi1 = 2 * (part1 - part2);

            return psi1;
        }

        private Complex CalcPsi1And5RcAboveTr(Complex[] eta, Complex[] p, Complex[] q, Z z, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi1 = CalculateCTop(z, eta, r, s) + p[t] * q[b] *
                       CalculateDTop(z, eta, r, s);

            return psi1;
        }
        private Complex CalcPsi1And5RcBelowTr(Complex[] eta, Complex[] p, Complex[] q, Z z, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi1 = CalculateCBot(z, eta, r, s) + p[t] * q[b] *
                       CalculateDBot(z, eta, r, s);

            return psi1;
        }

        private Complex CalcPsi3And4RcAndTrMatch(Complex[] eta, Complex[] p, Complex[] q, Z z, int r, int s)
        {
            return 0;
        }

        private Complex CalcPsi3And4RcAboveTr(Complex[] eta, Complex[] p, Complex[] q, Z z, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi3 = CalculateCTop(z, eta, r, s) - p[t] * q[b] *
                       CalculateDTop(z, eta, r, s);

            return psi3;
        }

        private Complex CalcPsi3And4RcBelowTr(Complex[] eta, Complex[] p, Complex[] q, Z z, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi3 = -CalculateCBot(z, eta, r, s) + p[t] * q[b] *
                        CalculateDBot(z, eta, r, s);

            return psi3;
        }

        private Complex CalcPsi5RcAndTrMatch(Complex[] eta, Complex[] p, Complex[] q, Z z, int n, double ll)
        {
            var tau = z.r2 - z.r1;
            var ln = (double)_model.Section1D[n].Thickness;

            var layer = _model.Section1D[n];
            var kk = new Complex(0, _model.Omega) * layer.Zeta * Mu0;

            var part1 = -(1 - exp(-eta[n] * tau));
            var part2 = p[n] * q[n] * (exp(-eta[n] * (2 * ln - tau)) - exp(-2 * eta[n] * ln));
            var part3 = (kk / ll) * (1 - p[n] * q[n] * exp(-2 * eta[n] * ln)) * tau * eta[n];

            var psi5 = 2 * (part1 + part2 + part3);

            return psi5;
        }
    }
}
