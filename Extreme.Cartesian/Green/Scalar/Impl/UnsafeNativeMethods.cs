//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    [SuppressUnmanagedCodeSecurity]
    internal unsafe class UnsafeNativeMethods
    {
        private const string LibName = @"ntv_math";

        public static void CalcEta(Complex[,] eta, int i, double[] lambdas, Complex value)
        {
            fixed (Complex* etaPtr = &eta[i, 0])
            fixed (double* lambdasPtr = &lambdas[0])
                   CalcEta(lambdas.Length, lambdasPtr, etaPtr, value);
        }

        public static void CalcExp(Complex[,] eta, int i, double factor, Complex[,] exp)
        {
            int length = eta.GetLength(1);

            fixed (Complex* etaPtr = &eta[i, 0], resultPtr = &exp[i, 0])
                CalcExp(length, etaPtr, factor, resultPtr);
        }

        private static Complex[] Calculate(NativeEnvelop ne, Complex[,] eta, Action<IntPtr, IntPtr, IntPtr> calc)
        {
            var result = new Complex[ne.length];

            fixed (Complex* etaPtr = &eta[0, 0], resultPtr = &result[0])
                calc(new IntPtr(&ne), new IntPtr(etaPtr), new IntPtr(resultPtr));

            return result;
        }

        public static Complex[] CalculateT(NativeEnvelop ne, Complex[,] eta)
            => Calculate(ne, eta, CalculateT);

        public static Complex[] CalculateR(NativeEnvelop ne, Complex[,] eta)
            => Calculate(ne, eta, CalculateR);

        public static Complex[] CalculateCTop(NativeEnvelop ne, Complex[,] eta)
            => Calculate(ne, eta, CalculateCTop);

        public static Complex[] CalculateCBot(NativeEnvelop ne, Complex[,] eta)
            => Calculate(ne, eta, CalculateCBot);

        public static Complex[] CalculateDTop(NativeEnvelop ne, Complex[,] eta)
            => Calculate(ne, eta, CalculateDTop);

        public static Complex[] CalculateDBot(NativeEnvelop ne, Complex[,] eta)
            => Calculate(ne, eta, CalculateDBot);

        [DllImport(LibName, EntryPoint = "CalculateT")]
        private static extern void CalculateT(IntPtr ne, IntPtr eta, IntPtr result);
        [DllImport(LibName, EntryPoint = "CalculateR")]
        private static extern void CalculateR(IntPtr ne, IntPtr eta, IntPtr result);
        [DllImport(LibName, EntryPoint = "CalculateCTop")]
        private static extern void CalculateCTop(IntPtr ne, IntPtr eta, IntPtr result);
        [DllImport(LibName, EntryPoint = "CalculateCBot")]
        private static extern void CalculateCBot(IntPtr ne, IntPtr eta, IntPtr result);
        [DllImport(LibName, EntryPoint = "CalculateDTop")]
        private static extern void CalculateDTop(IntPtr ne, IntPtr eta, IntPtr result);
        [DllImport(LibName, EntryPoint = "CalculateDBot")]
        private static extern void CalculateDBot(IntPtr ne, IntPtr eta, IntPtr result);
        [DllImport(LibName, EntryPoint = "CalcExp")]
        private static extern void CalcExp(int length, Complex* eta, double factor, Complex* result);
        [DllImport(LibName, EntryPoint = "CalcEta")]
        private static extern void CalcEta(int length, double* lambdasPtr, Complex* etaPtr, Complex value);
    }
}
