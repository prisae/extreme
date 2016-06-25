//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;

namespace Extreme.Cartesian.Forward
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class UnsafeNativeMethods
    {
        private const string LibName = @"ntv_math";

        [DllImport(LibName, EntryPoint = "AddElementwise")]
        public static extern void AddElementwise(long size, Complex* m1, Complex* m2, Complex* result);

        [DllImport(LibName, EntryPoint = "SubtractElementwise")]
        public static extern void SubtractElementwise(long size, Complex* m1, Complex* m2, Complex* result);

        [DllImport(LibName, EntryPoint = "MultiplyElementwiseAndAddToResult")]
        public static extern void MultiplyAndAddToResult(long size, Complex* m1, IntPtr m2, Complex* result);
        [DllImport(LibName, EntryPoint = "MultiplyElementwiseAndSubtractFromResult")]
        public static extern void MultiplyAndSubtractFromResult(long size, Complex* m1, IntPtr m2, Complex* result);

        [DllImport(LibName, EntryPoint = "MultiplyElementwise")]
        public static extern void MultiplyElementwise(long fullSize, Complex* src, Complex* r, Complex* dst);

        [DllImport(LibName, EntryPoint = "CalculateDotProductNotConjugated")]
        public static extern void CalculateProductNotConjugatedDouble(long size, Complex* m1, Complex* m2, ref Complex result);

        public static void Zaxpy(long n, Complex alpha, Complex* x, Complex* y) => Zaxpy(n, alpha, x, 1, y, 1);
        public static void Zaxpy(long n, Complex alpha, Complex* x, Complex* y, int incY) => Zaxpy(n, alpha, x, 1, y, incY);
        [DllImport(LibName, EntryPoint = "Zaxpy")]
        public static extern void Zaxpy(long n, Complex alpha, Complex* x, long incX, Complex* y, long intY);


        [DllImport(LibName, EntryPoint = "Zcopy")]
        public static extern void Zcopy(long n, Complex* x, Complex* y);


        [DllImport(LibName)]
        public static extern void ZgemvAtoO(int n, int m, Complex* alpha, Complex* beta, Complex* green, Complex* input, Complex* result);

        [DllImport(LibName)]
        public static extern void ZgemvOtoA(int n, int m, Complex* alpha, Complex* beta, Complex* green, Complex* input, Complex* result);




        [DllImport(LibName)]
        public static extern void Zscal(long n, Complex alpha, Complex* x);
        [DllImport(LibName)]
        public static extern void ClearBuffer(Complex* buffer, long length);


        [DllImport(LibName)]
        public static extern void FullZgemv(int nz, int length,
            Complex* xx,
            Complex* xy,
            Complex* xz,
            Complex* yy,
            Complex* yz,
            Complex* zz,
            Complex* src,
            Complex* dst);


        [DllImport(LibName)]
        public static extern void FullZgemv2(int nz, int length);

        [DllImport(LibName)]
        public static extern void ZgemvAsymTrans(int nz, Complex* alpha, Complex* beta, Complex* green, Complex* input, Complex* result);
        [DllImport(LibName)]
        public static extern void ZgemvAsymNoTrans(int nz, Complex* alpha, Complex* beta, Complex* green, Complex* input, Complex* result);
        [DllImport(LibName)]
        public static extern void ZgemvSym(int nz, Complex* alpha, Complex* beta, Complex* green, Complex* input, Complex* result);
    }
}
