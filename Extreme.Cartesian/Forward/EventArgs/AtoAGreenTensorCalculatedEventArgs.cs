//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian.Green.Tensor;

namespace Extreme.Cartesian.Forward
{
    public class AtoAGreenTensorCalculatedEventArgs : System.EventArgs
    {
        public GreenTensor GreenTensor { get; }

        public AtoAGreenTensorCalculatedEventArgs(GreenTensor greenTensor)
        {
            GreenTensor = greenTensor;
        }
    }
}
