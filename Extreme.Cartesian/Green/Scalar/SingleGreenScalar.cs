//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar
{
    public class SingleGreenScalar
    {
        public Transceiver Transceiver { get; } = Transceiver.Zero;

        public Complex[] I1 { get; }
        public Complex[] I2 { get; }
        public Complex[] I3 { get; }
        public Complex[] I4 { get; }
        public Complex[] I5 { get; }

        public SingleGreenScalar(ScalarPlan plan, Transceiver transceiver, int length)
        {
            Transceiver = transceiver;
            I1 = plan.CalculateI1 ? new Complex[length] : new Complex[0];
            I2 = plan.CalculateI2 ? new Complex[length] : new Complex[0];
            I3 = plan.CalculateI3 ? new Complex[length] : new Complex[0];
            I4 = plan.CalculateI4 ? new Complex[length] : new Complex[0];
            I5 = plan.CalculateI5 ? new Complex[length] : new Complex[0];
        }
    }

}
