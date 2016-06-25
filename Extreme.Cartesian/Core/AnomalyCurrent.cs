//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.Forward;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Core
{
    public unsafe class AnomalyCurrent : IDisposable
    {
        private readonly INativeMemoryProvider _memoryProvider;

        public Complex* Ptr { get; private set; }

		public int Nx { get; private set; }
        public int Ny { get; private set; }
        public int Nz { get; private set; }

        public MemoryLayoutOrder LayoutOrder { get; set; }

        private AnomalyCurrent(INativeMemoryProvider memoryProvider)
        {
            _memoryProvider = memoryProvider;
        }

        private AnomalyCurrent()
        {
        }

		public long GetFullLength() => ((long) Nx) * Ny * Nz * 3L;

		public Complex* this[long linearIndex]
            => Ptr + linearIndex;

        public static AnomalyCurrent ReUseMemory(Complex* ptr, int nx, int ny, int nz)
        {
            return new AnomalyCurrent()
            {
                Nx = nx,
                Ny = ny,
                Nz = nz,
                Ptr = ptr,
            };
        }

        public void CopyToAnomalyCurrent(AnomalyCurrent dst)
        {
            if (dst.Nx != Nx) throw new ArgumentOutOfRangeException(nameof(dst));
            if (dst.Ny != Ny) throw new ArgumentOutOfRangeException(nameof(dst));
            if (dst.Nz != Nz) throw new ArgumentOutOfRangeException(nameof(dst));
            
			var length = ((long)Nx) * Ny * Nz * 3L;
            UnsafeNativeMethods.Zcopy(length, Ptr, dst.Ptr);
        }

        public static AnomalyCurrent AllocateNewLocalSize(INativeMemoryProvider memoryProvider, OmegaModel model)
            => AllocateNew(memoryProvider, model.Anomaly.LocalSize.Nx, model.Anomaly.LocalSize.Ny, model.Nz);

        public static AnomalyCurrent AllocateNewDoubleLateralSize(INativeMemoryProvider memoryProvider, OmegaModel model)
            => AllocateNew(memoryProvider, 2 * model.Anomaly.LocalSize.Nx, 2 * model.Anomaly.LocalSize.Ny, model.Nz);

        public static AnomalyCurrent AllocateNewOneLayer(INativeMemoryProvider memoryProvider, CartesianModel model)
            => AllocateNew(memoryProvider, model.Anomaly.LocalSize.Nx, model.Anomaly.LocalSize.Ny, 1);

        public static AnomalyCurrent AllocateNewOneLayer(INativeMemoryProvider memoryProvider, OmegaModel model)
            => AllocateNew(memoryProvider, model.Anomaly.LocalSize.Nx, model.Anomaly.LocalSize.Ny, 1);

        public static AnomalyCurrent AllocateNew(INativeMemoryProvider memoryProvider, int nx, int ny, int nz)
        {
            if (memoryProvider == null) throw new ArgumentNullException(nameof(memoryProvider));

			long componentSize = ((long) nx) * ny * nz;
            var ptr = memoryProvider.AllocateComplex(componentSize * 3L);

            var current = new AnomalyCurrent(memoryProvider)
            {
                Nx = nx,
                Ny = ny,
                Nz = nz,
                Ptr = ptr,
            };

            return current;
        }

        public void Dispose()
        {
            _memoryProvider?.Release(Ptr);
        }
    }
}
