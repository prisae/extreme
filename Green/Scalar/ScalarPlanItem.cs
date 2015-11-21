using System;
using Extreme.Cartesian.Green;

namespace Extreme.Cartesian.Green.Scalar
{
    public class ScalarPlanItem
    {
        private readonly ScalarPlan _parent;
        public double[] Lambdas { get; }
        public double[] Rho { get; }
        public HankelCoefficients HankelCoefficients { get;}

        public bool CalculateI1 => _parent.CalculateI1;
        public bool CalculateI2 => _parent.CalculateI2;
        public bool CalculateI3 => _parent.CalculateI3;
        public bool CalculateI4 => _parent.CalculateI4;
        public bool CalculateI5 => _parent.CalculateI5;

        public ScalarPlanItem(ScalarPlan parent, HankelCoefficients hankelCoefficients, double[] rho, double[] lambdas)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (hankelCoefficients == null) throw new ArgumentNullException(nameof(hankelCoefficients));
            if (rho == null) throw new ArgumentNullException(nameof(rho));
            if (lambdas == null) throw new ArgumentNullException(nameof(lambdas));

            _parent = parent;
            Lambdas = lambdas;
            HankelCoefficients = hankelCoefficients;
            Rho = rho;
        }
    }
}
