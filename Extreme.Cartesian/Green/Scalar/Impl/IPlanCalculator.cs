//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public interface IPlanCalculator
    {
        InnerResult CalculateForE(Transmitter transmitter, Receiver receiver);
        InnerResult CalculateForH(Transmitter transmitter, Receiver receiver);
    }
}
