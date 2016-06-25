//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    internal class VolumeToPointCalculatorU : PlanCalculator.ICalculatorU
    {
        private const double Mu0 = (4.0 * System.Math.PI * 1.0E-07);
        private readonly OmegaModel _model;
        private readonly AuxContainer _container;

        public VolumeToPointCalculatorU(OmegaModel model, AuxContainer container)
        {
            _model = model;
            _container = container;
        }

        public Complex CalculateU1()
        {
            return CalcUGeneral(
            calcRcAndTrMatch: () => CalculateU1And5Special(CalcPsi1RcAndTrMatch),
            calcRcBelowTr: () => CalculateU1And5Special(CalcPsi1And5RcBelowTr),
            calcRcAboveTr: () => CalculateU1And5Special(CalcPsi1And5RcAboveTr));
        }

        public Complex CalculateU2()
        {
            return CalcUGeneral(
           calcRcAndTrMatch: () => CalculateU2Special(CalcPsi1RcAndTrMatch),
           calcRcBelowTr: () => CalculateU2Special(CalcPsi1And5RcBelowTr),
           calcRcAboveTr: () => CalculateU2Special(CalcPsi1And5RcAboveTr));
        }

        public Complex CalculateU3()
        {
            return CalcUGeneral(
           calcRcAndTrMatch: () => CalculateU3Special(CalcPsi3And4RcAndTrMatch),
           calcRcBelowTr: () => CalculateU3Special(CalcPsi3And4RcBelowTr),
           calcRcAboveTr: () => CalculateU3Special(CalcPsi3And4RcAboveTr));
        }

        public Complex CalculateU4()
        {
            return CalcUGeneral(
           calcRcAndTrMatch: () => CalculateU4Special(CalcPsi3And4RcAndTrMatch),
           calcRcBelowTr: () => CalculateU4Special(CalcPsi3And4RcBelowTr),
           calcRcAboveTr: () => CalculateU4Special(CalcPsi3And4RcAboveTr));
        }

        public Complex CalculateU5(double lambda)
        {
            return CalcUGeneral(
           calcRcAndTrMatch: () => CalculateU5SpecialForTrAndRcMatch(lambda),
           calcRcBelowTr: () => CalculateU1And5Special(CalcPsi1And5RcBelowTr),
           calcRcAboveTr: () => CalculateU1And5Special(CalcPsi1And5RcAboveTr));
        }

        private delegate Complex CalcGSpecial();

        private Complex CalcUGeneral(CalcGSpecial calcRcAndTrMatch, CalcGSpecial calcRcBelowTr, CalcGSpecial calcRcAboveTr)
        {
            var c = _container;
            var zWork = c.Rc.GetWorkingDepth();
            var z1 = c.Tr.StartingDepth;
            var z2 = c.Tr.GetEndDepth();

            if (zWork > z1 && zWork < z2) return calcRcAndTrMatch();
            if (zWork >= z2) return calcRcBelowTr();
            if (zWork <= z1) return calcRcAboveTr();

            throw new NotImplementedException();
        }

        private Complex CalculateU1And5Special(CalcPsi calcPsi)
        {
            var c = _container;
            int r = c.CorrBackgroundRc;
            int s = c.CorrBackgroundTr;
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var ann = GetAnn();

            var z1 = (double)c.Tr.StartingDepth;
            var z2 = (double)c.Tr.GetEndDepth();
            var zWork = (double)c.Rc.GetWorkingDepth();
            var eta = c.Eta;

            var pAddendum = CalculateT(zWork, z1, z2, eta, r, s);
            var qAddendum = CalculateR(zWork, z1, z2, eta, r, s);
            var psi = calcPsi(eta, c.P, c.Q, zWork, z1, z2, r, s);

            return (ann / eta[s]) * (c.P[t] * pAddendum + c.Q[b] * qAddendum + psi);
        }

        private Complex CalculateU5SpecialForTrAndRcMatch(double lambda)
        {
            var c = _container;

            int r = c.CorrBackgroundRc;
            int s = c.CorrBackgroundTr;
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var ann = GetAnn();

            var z1 = (double)c.Tr.StartingDepth;
            var z2 = (double)c.Tr.GetEndDepth();
            var zWork = (double)c.Rc.GetWorkingDepth();
            var eta = c.Eta;

            var pAddendum = CalculateT(zWork, z1, z2, eta, r, s);
            var qAddendum = CalculateR(zWork, z1, z2, eta, r, s);
            var psi = CalcPsi5RcAndTrMatch(eta, c.P, c.Q, zWork, z1, z2, r, lambda * lambda);

            return (ann / eta[s]) * (c.P[t] * pAddendum + c.Q[b] * qAddendum + psi);
        }

        private Complex CalculateU2Special(CalcPsi calcPsi)
        {
            var c = _container;

            int r = c.CorrBackgroundRc;
            int s = c.CorrBackgroundTr;
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var ann = GetAnn();

            var z1 = (double)c.Tr.StartingDepth;
            var z2 = (double)c.Tr.GetEndDepth();
            var zWork = (double)c.Rc.GetWorkingDepth();
            var eta = c.Eta;

            var pAddendum = CalculateT(zWork, z1, z2, eta, r, s);
            var qAddendum = CalculateR(zWork, z1, z2, eta, r, s);
            var psi = -calcPsi(eta, c.P, c.Q, zWork, z1, z2, r, s);

            return (ann * eta[r]) * (c.P[t] * pAddendum + c.Q[b] * qAddendum + psi);
        }

        private Complex CalculateU3Special(CalcPsi calcPsi)
        {
            var c = _container;

            int r = c.CorrBackgroundRc;
            int s = c.CorrBackgroundTr;
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var ann = GetAnn();

            var z1 = (double)c.Tr.StartingDepth;
            var z2 = (double)c.Tr.GetEndDepth();
            var zWork = (double)c.Rc.GetWorkingDepth();
            var eta = c.Eta;


            var pAddendum = -CalculateT(zWork, z1, z2, eta, r, s);
            var qAddendum = CalculateR(zWork, z1, z2, eta, r, s);
            var psi = calcPsi(eta, c.P, c.Q, zWork, z1, z2, r, s);

            return (ann * (eta[r] / eta[s])) * (c.P[t] * pAddendum + c.Q[b] * qAddendum + psi);
        }

        private Complex CalculateU4Special(CalcPsi calcPsi)
        {
            var c = _container;
          
            int r = c.CorrBackgroundRc;
            int s = c.CorrBackgroundTr;
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var ann = GetAnn();

            var z1 = (double)c.Tr.StartingDepth;
            var z2 = (double)c.Tr.GetEndDepth();
            var zWork = (double)c.Rc.GetWorkingDepth();
            var eta = c.Eta;

            var pAddendum = -CalculateT(zWork, z1, z2, eta, r, s);
            var qAddendum = CalculateR(zWork, z1, z2, eta, r, s);

            var psi = -calcPsi(eta, c.P, c.Q, zWork, z1, z2, r, s);

            return (ann) * (c.P[t] * pAddendum + c.Q[b] * qAddendum + psi);
        }


        private Complex CalculateT(double zWork, double z1, double z2, Complex[] eta, int r, int s)
        {
            int t = System.Math.Min(r, s);
            var dt = (double)_model.GetLayerDepth(t);

            var result = Complex.Exp(-eta[r] * zWork - eta[s] * z1 + 2 * eta[t] * dt) -
                            Complex.Exp(-eta[r] * zWork - eta[s] * z2 + 2 * eta[t] * dt);

            return result;
        }

        private Complex CalculateR(double zWork, double z1, double z2, Complex[] eta, int r, int s)
        {
            int b = System.Math.Max(r, s);
            var db1 = (double)_model.GetLayerDepth(b + 1);

            var result = Complex.Exp(-2 * eta[b] * db1 + eta[s] * z2 + eta[r] * zWork) -
                         Complex.Exp(-2 * eta[b] * db1 + eta[s] * z1 + eta[r] * zWork);

            return result;
        }

        private Complex CalculateCTop(double zWork, double z1, double z2, Complex[] eta, int r, int s)
        {
            var result = Complex.Exp(-eta[s] * z1 + eta[r] * zWork) -
                         Complex.Exp(-eta[s] * z2 + eta[r] * zWork);

            return result;
        }

        private Complex CalculateCBot(double zWork, double z1, double z2, Complex[] eta, int r, int s)
        {
            var result = Complex.Exp(-eta[r] * zWork + eta[s] * z2) -
                         Complex.Exp(-eta[r] * zWork + eta[s] * z1);

            return result;
        }

        private Complex CalculateDTop(double zWork, double z1, double z2, Complex[] eta, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);
            var dt = (double)_model.GetLayerDepth(t);
            var db1 = (double)_model.GetLayerDepth(b + 1);

            var result = Complex.Exp(-2 * (eta[b] * db1 - eta[t] * dt) + eta[s] * z2 - eta[r] * zWork) -
                         Complex.Exp(-2 * (eta[b] * db1 - eta[t] * dt) + eta[s] * z1 - eta[r] * zWork);

            return result;
        }

        private Complex CalculateDBot(double zWork, double z1, double z2, Complex[] eta, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);
            var dt = (double)_model.GetLayerDepth(t);
            var db1 = (double)_model.GetLayerDepth(b + 1);

            var result = Complex.Exp(-2 * (eta[b] * db1 - eta[t] * dt) - eta[s] * z1 + eta[r] * zWork) -
                         Complex.Exp(-2 * (eta[b] * db1 - eta[t] * dt) - eta[s] * z2 + eta[r] * zWork);

            return result;
        }

        private delegate Complex CalcPsi(
        Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int r, int s);

        private Complex CalcPsi1RcAndTrMatch(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1,
            double z2, int r, int s)
        {
            return CalcPsi1RcAndTrMatch(eta, p, q, zWork, z1, z2, r);
        }

        private Complex CalcPsi1RcAndTrMatch(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int n)
        {
            var psi1 = CalculateCTop(zWork, zWork, z2, eta, n, n) +
                       CalculateCBot(zWork, z1, zWork, eta, n, n) +
                       p[n] * q[n] * (CalculateDTop(zWork, zWork, z2, eta, n, n) +
                                      CalculateDBot(zWork, z1, zWork, eta, n, n));

            return psi1;
        }

        private Complex CalcPsi1And5RcAboveTr(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi1 = CalculateCTop(zWork, z1, z2, eta, r, s) + p[t] * q[b] *
                       CalculateDTop(zWork, z1, z2, eta, r, s);

            return psi1;
        }
        private Complex CalcPsi1And5RcBelowTr(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi1 = CalculateCBot(zWork, z1, z2, eta, r, s) + p[t] * q[b] *
                       CalculateDBot(zWork, z1, z2, eta, r, s);

            return psi1;
        }
        
        private Complex CalcPsi3And4RcAndTrMatch(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int r, int s)
        {
            int n = r;

            return CalculateCTop(zWork, zWork, z2, eta, n, n) -
                   CalculateCBot(zWork, z1, zWork, eta, n, n) +
                   p[n] * q[n] * (-CalculateDTop(zWork, zWork, z2, eta, n, n) +
                                   CalculateDTop(zWork, z1, zWork, eta, n, n));
        }

        private Complex CalcPsi3And4RcAboveTr(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi3 = CalculateCTop(zWork, z1, z2, eta, r, s) - p[t] * q[b] *
                       CalculateDTop(zWork, z1, z2, eta, r, s);

            return psi3;
        }

        private Complex CalcPsi3And4RcBelowTr(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int r, int s)
        {
            int t = System.Math.Min(r, s);
            int b = System.Math.Max(r, s);

            var psi3 = -CalculateCBot(zWork, z1, z2, eta, r, s) + p[t] * q[b] *
                        CalculateDBot(zWork, z1, z2, eta, r, s);

            return psi3;
        }

        private Complex CalcPsi5RcAndTrMatch(Complex[] eta, Complex[] p, Complex[] q, double zWork, double z1, double z2, int n, double ll)
        {
            var ln = (double)_model.Section1D[n].Thickness;

            var layer = _model.Section1D[n];
            var kk = new Complex(0, _model.Omega) * layer.Zeta * Mu0;


            var psi5 = p[n] * q[n] * (Complex.Exp(-eta[n] * (2 * ln - (z2 - zWork))) +
                                      Complex.Exp(-eta[n] * (2 * ln - (zWork - z1))))

                       - (Complex.Exp(-eta[n] * (z2 - zWork)) +
                          Complex.Exp(-eta[n] * (zWork - z1)))

                       + 2 * kk * ((1 - p[n] * q[n] * Complex.Exp(-2 * eta[n] * ln)) / ll);

            return psi5;
        }

        private Complex GetAnn()
        {
            return _container.A[_container.CorrBackgroundRc, _container.CorrBackgroundTr];
        }
    }
}
