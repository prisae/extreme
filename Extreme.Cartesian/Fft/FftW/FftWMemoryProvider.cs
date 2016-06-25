//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using FftWrap;
using Extreme.Core;

namespace Extreme.Cartesian.FftW
{
    public unsafe class FftWMemoryProvider : MemoryProvider
    {
        protected override void ReleaseMemory(IntPtr ptr)
            => Fftw.Free(ptr);

        protected override IntPtr AllocateMemory(long sizeInBytes)
            => Fftw.Malloc(new IntPtr(sizeInBytes));
    }
}
