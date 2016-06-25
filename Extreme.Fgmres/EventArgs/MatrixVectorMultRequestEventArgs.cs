//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Fgmres
{
    public class MatrixVectorMultRequestEventArgs : EventArgs
    {
        public NativeVector X { get; }
        public NativeVector Result { get; }

        public MatrixVectorMultRequestEventArgs(NativeVector x, NativeVector result)
        {
            X = x;
            Result = result;
        }
    }
}
