//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Core;

namespace Extreme.Fgmres
{
    public unsafe class NativeVector : IDisposable
    {
        private readonly INativeMemoryProvider _memoryProvider;
        public int Length { get; }

        private readonly Complex* _ptr;

        public NativeVector(INativeMemoryProvider memoryProvider, int length)
        {
            _memoryProvider = memoryProvider;
            _ptr = memoryProvider.AllocateComplex(length);
            Length = length;
        }

        public NativeVector(Complex* ptr, int length)
        {
            _ptr = ptr;
            Length = length;
        }

        public Complex this[long i]
        {
            get { return _ptr[i]; }
            set { _ptr[i] = value; }
        }

        public Complex* Ptr => _ptr;

        public void Dispose()
            => _memoryProvider?.Release(_ptr);

        public void SetAllValuesToZero()
        {
            for (int i = 0; i < Length; i++)
                _ptr[i] = Complex.Zero;
        }

        public static void Add(NativeVector v1, NativeVector v2, NativeVector result)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = v1[i] + v2[i];
        }

        public static void Sub(NativeVector v1, NativeVector v2, NativeVector result)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = v1[i] - v2[i];
        }

        public static void Mult(double alpha, NativeVector v, NativeVector result)
        {
            for (int i = 0; i < result.Length; i++)
                result[i] = alpha * v[i]; ;
        }

        public string DumpToString()
        {
            var str = "";

            for (int i = 0; i < Length; i++)
            {
                str += $"{_ptr[i]} ";
            }
            return str;
        }

        public Complex[] CopyToArray()
        {
            var result = new Complex[Length];

            for (int i = 0; i < Length; i++)
                result[i] = _ptr[i];
            return result;
        }

        public void CopyFromArray(Complex[] data)
        {
            if (Length != data.Length)
                throw new ArgumentOutOfRangeException();

            for (int i = 0; i < Length; i++)
                _ptr[i] = data[i];
        }

    }
}
