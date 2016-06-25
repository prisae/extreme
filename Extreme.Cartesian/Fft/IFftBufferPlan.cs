//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;

namespace Extreme.Cartesian.Fft
{
    public unsafe  interface IFftBufferPlan
    {
        Complex* Buffer1Ptr { get; }
        Complex* Buffer2Ptr { get; }
        int BufferLength { get; }
    }
}
