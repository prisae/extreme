//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using FftWrap;

namespace Extreme.Cartesian.FftW
{
    public class FftWTransform
    {
        public FftWTransform(int nThreads)
        {
            Fftw.InitThreads();
            Fftw.PlanWithNthreads(nThreads);
        }

        public void ExecutePlan(FftwPlan plan, IntPtr src, IntPtr dst)
            => Fftw.ExecuteDft(plan.Handler, src, dst);
        public void DestroyPlan(FftwPlan plan)
            => Fftw.DestroyPlan(plan.Handler);
        
        public FftwPlan CreatePlan2D(IntPtr src, IntPtr dst, int fullNx, int fullNy, int nz, Direction direction, Flags flags)
        {
           var n = new[] { fullNx, fullNy, };
            int dist = 1;
            int stride = nz;
            var plan = Fftw.PlanManyDft(2, n, nz, src, null, stride, dist, dst, null, stride, dist, (int)direction, (uint)flags);
            return new FftwPlan(fullNx, fullNy, nz, plan);
        }

        public FftwPlan CreatePlan3D(IntPtr src, IntPtr dst, int fullNx, int fullNy, int nz, Direction direction, Flags flags)
        {
            var n = new[] { fullNx, fullNy, nz, };
            int dist = 1;
            int stride = 1;
            var plan = Fftw.PlanManyDft(3, n, 1, src, null, stride, dist, dst, null, stride, dist, (int)direction, (uint)flags);
            return new FftwPlan(fullNx, fullNy, nz, plan);
        }
    }
}
