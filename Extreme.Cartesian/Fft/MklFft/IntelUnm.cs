//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Extreme.Cartesian.IntelMklFft
{
    [SuppressUnmanagedCodeSecurity]
    internal static partial class IntelMklUnm
    {
        private const string LibName = @"ntv_mkl_fft";

        [DllImport(LibName, EntryPoint = "CreateDftiDescriptorDouble")]
        public static extern IntPtr CreateDftiDescriptor(int nx, int ny);

        [DllImport(LibName, EntryPoint = "FreeDftiDescriptor")]
        public static extern void FreeDftiDescriptor(ref IntPtr descripor);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PerformForwardFft")]
        public static extern void PerformForwardFft(IntPtr descriptor, IntPtr srcAndDst);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PerformBackwardFft")]
        public static extern void PerformBackwardFft(IntPtr descriptor, IntPtr srcAndDst);

        [DllImport(LibName, EntryPoint = "Malloc")]
        public static extern IntPtr Malloc(IntPtr size, int alignment);

        [DllImport(LibName, EntryPoint = "Free")]
        public static extern void Free(IntPtr ptr);
    }
}