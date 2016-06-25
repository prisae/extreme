//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Collections.Generic;
using FftWrap.Numerics;

namespace FftWrap
{
    public class Memory
    {
        private static List<IntPtr> _allPointers = new List<IntPtr>();

        public static NativeArray<T> AllocateArray<T>(int length) where T : struct
        {
            var size = NativeArray<T>.ElementSize;

            IntPtr ptr = Fftw.Malloc((IntPtr)(length * size));

            _allPointers.Add(ptr);

            return new NativeArray<T>(ptr, length);
        }

        public static NativeMatrix<T> AllocateMatrix<T>(int nx, int ny) where T : struct
        {
            var size = NativeMatrix<T>.ElementSize;

            IntPtr ptr = Fftw.Malloc((IntPtr)(nx * ny * size));

            _allPointers.Add(ptr);

            return new NativeMatrix<T>(ptr, nx, ny);
        }

        public static void FreeAllPointers()
        {
            _allPointers.ForEach(Fftw.Free);
        }
    }
}
