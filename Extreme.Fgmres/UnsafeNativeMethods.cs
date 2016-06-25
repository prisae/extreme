//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;

namespace Extreme.Fgmres
{
    [SuppressUnmanagedCodeSecurity]
    internal unsafe class UnsafeNativeMethods
    {
        private const string LibName = @"ntv_math";

        [DllImport(LibName)]
        public static extern void Zaxpy(long n, Complex alpha, Complex* x, int incX, Complex* y, int intY);

        [DllImport(LibName)]
        public static extern void Zrot(int n, Complex* x, int incX, Complex* y, int incY, double c, Complex s);

        [DllImport(LibName)]
        public static extern void Zrotg(Complex a, Complex b, double* c, Complex* s);

        [DllImport(LibName)]
        public static extern void Zcopy(long n, Complex* x, Complex* y);

        [DllImport(LibName)]
        public static extern void SimplifiedZtrsv(int n, Complex* a, int lda, Complex* x);

        [DllImport(LibName)]
        public static extern void ZgemvNotTrans(int n, int jh, Complex alpha, Complex* a, Complex* input, Complex beta, Complex* result);

        [DllImport(LibName)]
        public static extern double Dznrm2(int n, Complex* x);
    }
}
