//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Extreme.Cartesian.Model;

namespace Extreme.Cartesian.Core
{

    public interface IAnomalyCurrentAccessor
    {
        Complex this[int i, int j, int k] { get; set; }
    }

    public unsafe class AnomalyCurrentAccessor : IAnomalyCurrentAccessor
    {
        private readonly Complex* _ptr;
        private readonly int _comp;
        private readonly long _nx;
        private readonly long _ny;
        private readonly long _nz;

        public AnomalyCurrentAccessor(AnomalyCurrent ac, int comp)
        {
            _ptr = ac.Ptr;
            _comp = comp;

            _nx = ac.Nx;
            _ny = ac.Ny;
            _nz = ac.Nz;
        }

        public static IAnomalyCurrentAccessor NewX(AnomalyCurrent ac) => new AnomalyCurrentAccessor(ac, 0);
        public static IAnomalyCurrentAccessor NewY(AnomalyCurrent ac) => new AnomalyCurrentAccessor(ac, 1);
        public static IAnomalyCurrentAccessor NewZ(AnomalyCurrent ac) => new AnomalyCurrentAccessor(ac, 2);


        public Complex this[int i, int j, int k]
        {
            get { return _ptr[GetIndex(i, j, k)]; }
            set { _ptr[GetIndex(i, j, k)] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetIndex(long i, long j, long k)
                    => (i * _ny + j) * 3 * _nz + _nz * _comp + k;
    }


    public interface ILayerAccessor
    {
        Complex this[long i, long j] { get; set; }
        int Nx { get; }
        int Ny { get; }
    }

    public unsafe class VerticalLayerAccessor : ILayerAccessor
    {
        private readonly int _nz;
        private readonly Complex* _ptr;
        public int Nx { get; }
        public int Ny { get; }

        private VerticalLayerAccessor(Complex* basePtr, int nx, int ny, int nz, int k)
        {
            _nz = nz;
			_ptr = basePtr + (long)k;
            Nx = nx;
            Ny = ny;
        }

        public static ILayerAccessor NewX(AnomalyCurrent ac, int k) =>
            new VerticalLayerAccessor(ac.Ptr, ac.Nx, ac.Ny, ac.Nz, k);

        public static ILayerAccessor NewY(AnomalyCurrent ac, int k) =>
            new VerticalLayerAccessor(ac.Ptr + ac.Nz, ac.Nx, ac.Ny, ac.Nz, k);

        public static ILayerAccessor NewZ(AnomalyCurrent ac, int k) =>
            new VerticalLayerAccessor(ac.Ptr + ac.Nz * 2, ac.Nx, ac.Ny, ac.Nz, k);

        public Complex this[long i, long j]
        {
            get { return _ptr[(i * Ny + j) * 3 * _nz]; }
            set { _ptr[(i * Ny + j) * 3 * _nz] = value; }
        }
    }

    public static class AccessorUtils
    {
        public static IAnomalyCurrentAccessor GetX(this AnomalyCurrent ac) 
            => AnomalyCurrentAccessor.NewX(ac);
        public static IAnomalyCurrentAccessor GetY(this AnomalyCurrent ac) 
            => AnomalyCurrentAccessor.NewY(ac);
        public static IAnomalyCurrentAccessor GetZ(this AnomalyCurrent ac)
            => AnomalyCurrentAccessor.NewZ(ac);
    }

}
