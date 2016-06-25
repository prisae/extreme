//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Collections.Generic;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Parallel;
using System.Runtime.CompilerServices;

namespace Extreme.Cartesian.Fft
{
    public static class FftBuffersPool
    {
        #region ModelSize struct
        private struct ModelSize
        {
            public bool Equals(ModelSize other)
            {
                return Nx == other.Nx && Ny == other.Ny && Nz == other.Nz;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ModelSize && Equals((ModelSize)obj);
            }

            public readonly int Nx;
            public readonly int Ny;
            public readonly int Nz;

            public ModelSize(int nx, int ny, int nz)
            {
                Nx = nx;
                Ny = ny;
                Nz = nz;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Nx;
                    hashCode = (hashCode * 397) ^ Ny;
                    hashCode = (hashCode * 397) ^ Nz;
                    return hashCode;
                }
            }

            public static bool operator ==(ModelSize a, ModelSize b)
                => a.Nx == b.Nx && a.Ny == b.Ny && a.Nz == b.Nz;

            public static bool operator !=(ModelSize a, ModelSize b)
                => !(a == b);
        }

        #endregion

        private static readonly Dictionary<ModelSize, FftBuffer> Buffers;

        static FftBuffersPool()
        {
            Buffers = new Dictionary<ModelSize, FftBuffer>();
        }
        public static FftBuffer GetBuffer(OmegaModel model)
        {
            var modelSize = new ModelSize(model.Nx, model.Ny, model.Nz);

            if (!Buffers.ContainsKey(modelSize))
                throw new InvalidOperationException("Fft buffer does not created for this model size");

            return Buffers[modelSize];
        }

        public static void PrepareBuffersForModel(CartesianModel model, INativeMemoryProvider memoryProvider, Mpi mpi = null, IProfiler profiler = null)
        {
            var modelSize = new ModelSize(model.Nx, model.Ny, model.Nz);
            PrepareBuffersForModel(modelSize, memoryProvider, mpi, profiler);
        }

        private static void PrepareBuffersForModel(ModelSize ms, INativeMemoryProvider memoryProvider, Mpi mpi, IProfiler profiler)
        {
            if (Buffers.ContainsKey(ms))
                throw new InvalidOperationException("Buffer for such model size is already created");

            using (profiler?.StartAuto(ProfilerEvent.FftwPlanCalculation))
            {
                var buffer = new FftBuffer(memoryProvider, profiler);

                if (IsParallel(mpi))
                    buffer.AllocateBuffersAndCreatePlansParallel(ms.Nx, ms.Ny, ms.Nz, mpi);
                else
                    buffer.AllocateBuffersAndCreatePlansLocal(ms.Nx, ms.Ny, ms.Nz);

                Buffers.Add(ms, buffer);
            }
        }

        private static bool IsParallel(Mpi mpi)
            => mpi != null && mpi.Size > 1;

		public static void Free(Mpi mpi){
			if (IsParallel(mpi)) {
				foreach (var buf in Buffers.Values)
					buf.Dispose ();
			}
		}
    }
}
