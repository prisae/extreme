//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Runtime.InteropServices;

namespace FftWrap.Numerics
{
    public class NativeArray<T> where T : struct
    {
        private readonly int _length;
        private readonly IntPtr _ptr;

        public static readonly int ElementSize = Marshal.SizeOf(typeof(T));

        public NativeArray(IntPtr ptr, int length)
        {
            _ptr = ptr;
            _length = length;
        }

        public IntPtr Ptr
        {
            get { return _ptr; }
        }

        public int Length
        {
            get { return _length; }
        }

        public T this[int i]
        {
            get { return GetValue(i); }
            set { SetValue(i, value); }
        }

        private void SetValue(int i, T value)
        {
            if (i < 0 || i >= _length)
                throw new ArgumentOutOfRangeException();

            int shift = i * ElementSize;

            Marshal.StructureToPtr(value, (IntPtr.Add(_ptr, shift)), false);
        }

        private T GetValue(int i)
        {
            if (i < 0 || i >= _length )
                throw new ArgumentOutOfRangeException();

            int shift = i * ElementSize;

            return (T)Marshal.PtrToStructure(IntPtr.Add(_ptr, shift), typeof(T));
        }
    }
}
