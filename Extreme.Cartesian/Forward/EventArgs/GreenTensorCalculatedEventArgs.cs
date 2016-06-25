//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian.Green.Tensor;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public class GreenTensorCalculatedEventArgs : System.EventArgs
    {
        public ObservationLevel Level { get; }
        public GreenTensor GreenTensor { get; }
        public bool SupressGreenTensorDisposal { get; set; }

        public GreenTensorCalculatedEventArgs(ObservationLevel level, GreenTensor greenTensor)
        {
            Level = level;
            GreenTensor = greenTensor;
        }
    }
}
