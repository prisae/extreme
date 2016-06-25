//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Parallel;
using FftWrap;

namespace Extreme.Cartesian.FftW
{
    public class DistributedFftWTransform : IDisposable
    {
        public DistributedFftWTransform(int nThreads)
        {
            //Fftw.InitThreads();
            //FftwMpi.Init();
            //Fftw.PlanWithNthreads(nThreads);
        }

        private readonly IntPtr FFTW_MPI_DEFAULT_BLOCK = IntPtr.Zero;

        public long GetFullLocalSize(int fullNx, int fullNy, int nz)
        {
            var n = new[] { new IntPtr(fullNx), new IntPtr(fullNy), };
            IntPtr localN0;
            IntPtr localN0Start;

            long localSize = FftwMpi.LocalSizeMany(2, n, new IntPtr(nz),
                FFTW_MPI_DEFAULT_BLOCK, Mpi.CommWorld, out localN0, out localN0Start).ToInt64();

            return localSize;
        }

        public int GetLocalN0(int fullNx, int fullNy, int nz)
        {
            var n = new[] { new IntPtr(fullNx), new IntPtr(fullNy), };
            IntPtr localN0;
            IntPtr localN0Start;

            long localSize = FftwMpi.LocalSizeMany(2, n, new IntPtr(nz),
                FFTW_MPI_DEFAULT_BLOCK, Mpi.CommWorld, out localN0, out localN0Start).ToInt64();

            return localN0.ToInt32();
        }

        public FftwPlan CreatePlan(IntPtr memory, int fullNx, int fullNy, int nz, Direction direction, Flags flags)
        {
            var n = new[] { new IntPtr(fullNx), new IntPtr(fullNy), };

            var plan = FftwMpi.PlanManyDft(2, n, new IntPtr(nz), FFTW_MPI_DEFAULT_BLOCK, FFTW_MPI_DEFAULT_BLOCK,
                memory, memory, Mpi.CommWorld, (int)direction, (uint)flags);

            return new FftwPlan(fullNx, fullNy, nz, plan);
        }

        public void ExecutePlan(FftwPlan plan, IntPtr src, IntPtr dst)
            => FftwMpi.ExecuteDft(plan.Handler, src, dst);

        public void DestroyPlan(FftwPlan plan)
            => Fftw.DestroyPlan(plan.Handler);

        public void Dispose()
        {
            FftwMpi.Cleanup();
        }
    }
}
