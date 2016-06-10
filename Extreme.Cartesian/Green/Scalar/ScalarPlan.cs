using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Green;

namespace Extreme.Cartesian.Green.Scalar
{
    public class ScalarPlan
    {
        private readonly List<ScalarPlanItem> _items;

        public ScalarPlanItem[] Items => _items.ToArray();
        public bool CalculateZeroRho { get; }

        public bool CalculateI1 { get; set; } = true;
        public bool CalculateI2 { get; set; } = true;
        public bool CalculateI3 { get; set; } = true;
        public bool CalculateI4 { get; set; } = true;
        public bool CalculateI5 { get; set; } = true;

        public void SetAll()
        {
            CalculateI1 = true;
            CalculateI2 = true;
            CalculateI3 = true;
            CalculateI4 = true;
            CalculateI5 = true;
        }

        public void SetOnlyAsym()
        {
            CalculateI1 = false;
            CalculateI2 = false;
            CalculateI3 = true;
            CalculateI4 = false;
            CalculateI5 = false;
        }

        public void SetOnlySymm()
        {
            CalculateI1 = true;
            CalculateI2 = true;
            CalculateI3 = false;
            CalculateI4 = false;
            CalculateI5 = true;
        }
        
        public ScalarPlan(bool calculateZeroRho)
        {
            CalculateZeroRho = calculateZeroRho;
            _items = new List<ScalarPlanItem>();
        }

        public double[] GetSortedRho()
        {
            var rho = Items.SelectMany(t => t.Rho).ToList();
            rho.Sort();

            if (CalculateZeroRho)
                rho.Insert(0, 0);

            return rho.ToArray();
        }

        public void AddNewLog10PlanItem(HankelCoefficients hankelCoefficients, double[] rho)
        {
            if (hankelCoefficients == null) throw new ArgumentNullException(nameof(hankelCoefficients));
            if (rho == null) throw new ArgumentNullException(nameof(rho));

            var lambdas = CalculateLambdasForLog10(hankelCoefficients, rho);

            _items.Add(new ScalarPlanItem(this, hankelCoefficients, rho, lambdas));
        }

        private static double[] CalculateLambdasForLog10(HankelCoefficients hankel, double[] rho)
        {
            var rhoLength = rho.Length;

            var k = new double[hankel.GetLengthOfLambdaWithRespectTo(rhoLength)];

            var rhoMin = rho[0];
            var rhoStep = hankel.GetLog10RhoStep();

            int n1 = hankel.GetN1WithRespectTo(rhoLength);
            int n2 = hankel.GetN2WithRespectTo(rhoLength);

            k[-n1] = 1 / rhoMin;

            for (int i = -n1 + 1; i <= n2 - n1; i++)
                k[i] = k[i - 1] / rhoStep;

            for (int i = -n1 - 1; i >= 0; i--)
                k[i] = k[i + 1] * rhoStep;

            return k;
        }
    }
}