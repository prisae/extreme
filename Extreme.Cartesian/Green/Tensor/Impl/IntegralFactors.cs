//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Cartesian.Green.Tensor
{
    public class IntegralFactors
    {
        public double D1A { get; private set; }
        public double D1B { get; private set; }
        public double D2A { get; private set; }
        public double D2B { get; private set; }
        public double D3A { get; private set; }
        public double D3B { get; private set; }

        private IntegralFactors()
        {
        }

        public static IntegralFactors Prepare(double s, double u, double rs, double ru, double v)
        {
            return new IntegralFactors()
            {
                D1A = v == 0 ? (u * ru - s * rs) / 2 : (u * ru - s * rs + v * v * (System.Math.Log((u + ru) / (s + rs)))) / 2,
                D1B = u - s,

                D2A = v == 0 ? 0 : -(u - s) * v,
                D2B = v == 0 ? 0 : -(v * System.Math.Log((u + ru) / (s + rs))),

                D3A = -(u * u - s * s) / 2,
                D3B = -(ru - rs),
            };
        }

    }
}
