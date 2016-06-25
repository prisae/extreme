//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public struct InnerResult
    {
        public Complex[] I1;
        public Complex[] I2;
        public Complex[] I3;
        public Complex[] I4;
        public Complex[] I5;

        public double[] Rho;
    }

    public struct ResultE
    {
        public Complex[] U1;
        public Complex[] U2;
        public Complex[] U3;
        public Complex[] U4;
        public Complex[] U5;
    }

    public struct ResultH
    {
        public Complex[] U11;
        public Complex[] U1Sigma;
        public Complex[] U31;
        public Complex[] U4Sigma;
    }
}
