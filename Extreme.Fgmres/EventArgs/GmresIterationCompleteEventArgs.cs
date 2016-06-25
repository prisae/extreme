//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Fgmres
{
    public class GmresIterationCompleteEventArgs : EventArgs
    {
        public int NumberOfIteration { get; }
        public double ArnoldiBackwardError { get; }

        public GmresIterationCompleteEventArgs(int numberOfIteration, double arnoldiBackwardError)
        {
            NumberOfIteration = numberOfIteration;
            ArnoldiBackwardError = arnoldiBackwardError;
        }
    }
}
