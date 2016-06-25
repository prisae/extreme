//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;

namespace Extreme.Cartesian.Green.Scalar
{
    public unsafe class SingleScalarSegment
    {
        public Complex* I1A { get; }
        public Complex* I2A { get; }
        public Complex* I3A { get; }
        public Complex* I4A { get; }
        public Complex* I5A { get; }
                      
        public Complex* I1B { get; }
        public Complex* I2B { get; }
        public Complex* I3B { get; }
        public Complex* I4B { get; }
        public Complex* I5B { get; }

        public SingleScalarSegment(ScalarPlan plan, Complex* startPtr, int length, int nComp)
        {
            int counter = 0;
            I1A = plan.CalculateI1 ? startPtr + length * counter++ : null;
            I2A = plan.CalculateI2 ? startPtr + length * counter++ : null;
            I3A = plan.CalculateI3 ? startPtr + length * counter++ : null;
            I4A = plan.CalculateI4 ? startPtr + length * counter++ : null;
            I5A = plan.CalculateI5 ? startPtr + length * counter++ : null;
                  
            I1B = plan.CalculateI1 ? startPtr + length * counter++ : null;
            I2B = plan.CalculateI2 ? startPtr + length * counter++ : null;
            I3B = plan.CalculateI3 ? startPtr + length * counter++ : null;
            I4B = plan.CalculateI4 ? startPtr + length * counter++ : null;
            I5B = plan.CalculateI5 ? startPtr + length * counter++ : null; ;

            if (counter != nComp * 2)
                throw new InvalidOperationException();
        }

        public SingleScalarSegment()
        {
            I1A = null;
            I2A = null;
            I3A = null;
            I4A = null;
            I5A = null;

            I1B = null;
            I2B = null;
            I3B = null;
            I4B = null;
            I5B = null;
        }
    }
}
