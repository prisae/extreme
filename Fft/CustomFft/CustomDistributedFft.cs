using System;
using System.Numerics;
using FftWrap;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.FftW;
using Extreme.Cartesian.Forward;
using Extreme.Core;
using Extreme.Parallel;

using static System.Math;

namespace Extreme.Cartesian.Fft
{
    public unsafe class CustomDistributedFft
    {
        private readonly Mpi _mpi;
        private readonly uint _flags1d = (uint)(Flags.Measure | Flags.DestroyInput);
        private readonly uint _flagsTranspose = (uint)(Flags.Patient | Flags.DestroyInput);

        private readonly IProfiler _profiler;

        private enum Direction
        {
            Forward,
            Backward
        }

        public CustomDistributedFft(Mpi mpi, IProfiler profiler = null)
        {
            _mpi = mpi;
            _profiler = profiler;

            Fftw.InitThreads();
            FftwMpi.Init();
            Fftw.PlanWithNthreads(MultiThreadUtils.MaxDegreeOfParallelism);
        }

        public static int GetLocalSize(Mpi mpi, int fullNx, int fullNy, int nc)
        {
            var localNx = mpi.CalcLocalNxLength(fullNx);
            var numberOfLocalFftsAlongSecondDimension = GetNumberOfLocalFftsAlongSecondDimension(mpi, fullNy, nc);
            var localSize = numberOfLocalFftsAlongSecondDimension * mpi.Size * localNx;

            return localSize;
        }

        private static int GetNumberOfLocalFftsAlongSecondDimension(Mpi mpi, int fullNy, int nc)
        {
            var mx = fullNy * nc;
            var m = mx % mpi.Size;

            var numberOfLocalFftsAlongSecondDimension =
                m == 0 ? mx / mpi.Size : (mx - m) / mpi.Size + 1;

            return numberOfLocalFftsAlongSecondDimension;
        }

        public CustomFftPlan CreatePlan2D(Complex* input, Complex* output, Mpi mpi, int fullNx, int fullNy, int nc)
            => CreatePlan(input, output, mpi, fullNx, fullNy, nc, 2);
        public CustomFftPlan CreatePlan3D(Complex* input, Complex* output, Mpi mpi, int fullNx, int fullNy, int nc)
            => CreatePlan(input, output, mpi, fullNx, fullNy, nc, 3);

        public CustomFftPlan CreatePlan(Complex* input, Complex* output, Mpi mpi, int fullNx, int fullNy, int nc, int dimension)
        {
            var localNx = mpi.CalcLocalNxLength(fullNx);
            var numberOfLocalFftsAlongSecondDimension = GetNumberOfLocalFftsAlongSecondDimension(mpi, fullNy, nc);

            var plan = new CustomFftPlan(input, output, dimension)
            {
                Ny = fullNy,
                Nc = nc,
                NumberOfMpiProcesses = mpi.Size,
                LocalNx = localNx,
                NumberOfLocalFftsAlongSecondDimension = numberOfLocalFftsAlongSecondDimension
            };

            CreateInternalPlans(plan);

            return plan;
        }


        private void CreateInternalPlans(CustomFftPlan data)
        {
            CreateTransposePlan(data, _flagsTranspose);
            Create1DFftPlansY(data, _flags1d);
            Create1DFftPlansX(data, _flags1d);

            if (data.Dimension == 3)
                Create1DFftPlansZ(data, _flags1d);
        }

        private void Create1DFftPlansZ(CustomFftPlan data, uint flags)
        {
            var pNx = new int[] { data.Nc };
            var howmany = data.LocalNx * data.Ny;

            var input = new IntPtr(data.Input.Ptr);
            var output = new IntPtr(data.Output.Ptr);

            int stride = 1;
            int dist = data.Nc;

            var handlerPlanZForward
              = Fftw.PlanManyDft(1, pNx, howmany, input, pNx, stride, dist, output, pNx, stride, dist, (int)FftWrap.Direction.Forward, flags);

            var handlerPlanZBackward
              = Fftw.PlanManyDft(1, pNx, howmany, input, pNx, stride, dist, output, pNx, stride, dist, (int)FftWrap.Direction.Backward, flags);


            data.ForwardZ = new FftwPlan(0, 0, 0, handlerPlanZForward);
            data.BackwardZ = new FftwPlan(0, 0, 0, handlerPlanZBackward);
        }

        private void Create1DFftPlansX(CustomFftPlan data, uint flags)
        {
            var pNx = new int[] { data.LocalNx * data.NumberOfMpiProcesses };
            var howmany = data.NumberOfLocalFftsAlongSecondDimension;

            var input = new IntPtr(data.Input.Ptr);
            var output = new IntPtr(data.Output.Ptr);

            var handlerPlanXForward
              = Fftw.PlanManyDft(1, pNx, howmany, input, pNx, howmany, 1, output, pNx, howmany, 1, (int)FftWrap.Direction.Forward, flags);

            var handlerPlanXBackward
              = Fftw.PlanManyDft(1, pNx, howmany, input, pNx, howmany, 1, output, pNx, howmany, 1, (int)FftWrap.Direction.Backward, flags);


            data.ForwardX = new FftwPlan(0, 0, 0, handlerPlanXForward);
            data.BackwardX = new FftwPlan(0, 0, 0, handlerPlanXBackward);
        }

        private static void Create1DFftPlansY(CustomFftPlan data, uint flags)
        {
            var pNy = new int[] { data.Ny, };
            var dy = data.Ny;
            var howmany = data.LocalNx * data.Nc;

            var input = new IntPtr(data.Output.Ptr);
            var output = new IntPtr(data.Input.Ptr);

            var handlerPlanYForward
                = Fftw.PlanManyDft(1, pNy, howmany, input, pNy, 1, dy, output, pNy, 1, dy, (int)FftWrap.Direction.Forward,
                    flags);

            var handlerPlanYBackward
                = Fftw.PlanManyDft(1, pNy, howmany, input, pNy, 1, dy, output, pNy, 1, dy, (int)FftWrap.Direction.Backward,
                    flags);

            data.ForwardY = new FftwPlan(0, 0, 0, handlerPlanYForward);
            data.BackwardY = new FftwPlan(0, 0, 0, handlerPlanYBackward);
        }

        private static void CreateTransposePlan(CustomFftPlan data, uint flags)
        {
            var n = new IntPtr(2 * data.NumberOfLocalFftsAlongSecondDimension * data.LocalNx);
            var np = new IntPtr(data.NumberOfMpiProcesses);
            var one = new IntPtr(1);

            var input = new IntPtr(data.Input.Ptr);
            var output = new IntPtr(data.Output.Ptr);

            var handler = FftwMpi.PlanManyTranspose(np, np, n, one, one, input, output, Mpi.CommWorld, flags);
            data.TransposePlan = new FftwPlan(np.ToInt32(), np.ToInt32(), n.ToInt32(), handler);
        }

        public void ComputeForward(CustomFftPlan data)
            => Compute(data, Direction.Forward);

        public void ComputeBackward(CustomFftPlan data)
            => Compute(data, Direction.Backward);

        private void Compute(CustomFftPlan data, Direction direction)
        {
            var buff1 = direction == Direction.Forward ? data.Input : data.Output;
            var buff2 = direction == Direction.Forward ? data.Output : data.Input;

            using (_profiler?.StartAuto(ProfilerEvent.CustomFft))
            {
                if (data.Dimension == 3)
                    WithProfiler(ProfilerEvent.CustomFftFourierZ, () => FourierZ(data, direction, buff2, buff1));

                WithProfiler(ProfilerEvent.CustomFftInitialTranspose, () => InitialTranspose(data, buff1, buff2));
                WithProfiler(ProfilerEvent.CustomFftFourierY, () => FourierY(data, direction, buff2, buff1));
                WithProfiler(ProfilerEvent.CustomFftBlockTransposeYtoX, () => BlockTransposeYtoX(data, buff1, buff2));
                WithProfiler(ProfilerEvent.CustomFftDistributedTranspose, () => DistributedTranspose(data, buff2, buff1));
                WithProfiler(ProfilerEvent.CustomFftFourierX, () => FourierX(data, direction, buff1, buff2));
                WithProfiler(ProfilerEvent.CustomFftDistributedTranspose, () => DistributedTranspose(data, buff2, buff1));
                WithProfiler(ProfilerEvent.CustomFftBlockTransposeXtoY, () => BlockTransposeXtoY(data, buff1, buff2));
                WithProfiler(ProfilerEvent.CustomFftFinalTranspose, () => FinalTranspose(data, buff2, buff1));
            }
        }

        protected virtual void InitialTranspose(CustomFftPlan data, UnsafeArray src, UnsafeArray dst)
        {
            var nxy = data.LocalNx * data.Ny;
            var nz = data.Nc;

            LocalTranspose(src, dst, nz, nxy);
        }


        private void WithProfiler(ProfilerEvent profEvent, Action action)
        {
            using (_profiler?.StartAuto(profEvent))
            {
                action();
            }
        }

        private unsafe void PrintAllMuHaHa(UnsafeArray data, int nx, int ny)
        {
            var src = data.ReShape(nx, ny);

            for (int r = 0; r < _mpi.Size; r++)
            {
                if (_mpi.Rank == r)
                {
                    for (int i = 0; i < nx; i++)
                    {
                        Console.Write($"{_mpi.Rank} i:{i}");
                        for (int j = 0; j < ny; j++)
                            Console.Write($"{src[i, j]} ");

                        Console.WriteLine();
                    }
                }

                _mpi.Barrier();
            }
        }

        private void FinalTranspose(CustomFftPlan data, UnsafeArray src, UnsafeArray dst)
        {
            var nxy = data.LocalNx * data.Ny;
            var nz = data.Nc;

            LocalTranspose(src, dst, nxy, nz);
        }

        protected virtual void LocalTranspose(UnsafeArray input, UnsafeArray output, int first, int second)
        {
            output.ReShape(first, second);
            input.ReShape(second, first);

            Iterate(first, i =>
            {
                //for (var i = 0; i < first; i++)
                for (int j = 0; j < second; j++)
                    output[i, j] = input[j, i];
            });
        }

        private void BlockTransposeYtoX(CustomFftPlan data, UnsafeArray src, UnsafeArray dst)
        {
            var locNx = data.LocalNx;
            var ny = data.Ny;
            var np = data.NumberOfMpiProcesses;
            var nk = data.NumberOfLocalFftsAlongSecondDimension;

            var bufferSize = nk * np * locNx - 1;

            dst.ReShape(np, locNx, nk);

            //Linear writting, nonlinear reading
            Iterate(np, ip =>
            {
                //for (int ip = 0; ip < np; ip++)
                for (int ix = 0; ix < locNx; ix++)
                    for (int ik = 0; ik < nk; ik++)
                    {
                        var l0 = ik + (ip) * nk; // place in XZ
                        var m0 = l0 - l0 % ny;
                        var l = l0 + ix * ny + m0 * (locNx - 1); //Ix + (Iy - 1)Nx + (Ic - 1)NxNyLoc
                        l = (l + bufferSize - Abs(bufferSize - l)) / 2; // if l > M we are in "addition" zone

                        dst[ip, ix, ik] = src[l];
                    }
            });
        }

        private void FourierY(CustomFftPlan data, Direction direction, UnsafeArray src, UnsafeArray dst)
        {
            var plan = direction == Direction.Forward ? data.ForwardY : data.BackwardY;
            Fftw.ExecuteDft(plan.Handler, new IntPtr(src.Ptr), new IntPtr(dst.Ptr));
        }

        private void FourierX(CustomFftPlan data, Direction direction, UnsafeArray src, UnsafeArray dst)
        {
            var plan = direction == Direction.Forward ? data.ForwardX : data.BackwardX;
            Fftw.ExecuteDft(plan.Handler, new IntPtr(src.Ptr), new IntPtr(dst.Ptr));
        }

        private void FourierZ(CustomFftPlan data, Direction direction, UnsafeArray src, UnsafeArray dst)
        {
            var plan = direction == Direction.Forward ? data.ForwardZ : data.BackwardZ;
            Fftw.ExecuteDft(plan.Handler, new IntPtr(src.Ptr), new IntPtr(dst.Ptr));
        }

        private void BlockTransposeXtoY(CustomFftPlan data, UnsafeArray src, UnsafeArray dst)
        {
            var ny = data.Ny;
            var locNx = data.LocalNx;
            var nm = data.Nc;
            var nk = data.NumberOfLocalFftsAlongSecondDimension;

            dst.ReShape(nm, locNx, ny);

            //Linear writting, nonlinear reading
            Iterate(nm, im =>
            {
                //for (int im = 0; im < nm; im++)
                for (int ix = 0; ix < locNx; ix++)
                    for (int iy = 0; iy < ny; iy++)
                    {
                        var l0 = iy + im * ny; // place in XZ
                        var m0 = l0 - l0 % nk; //Ik = m0
                        var l = l0 + ix * nk + m0 * (locNx - 1); //Ix + (Iy - 1)Nx + (Ic - 1)NxNyLoc

                        dst[im, ix, iy] = src[l];
                    }
            });
        }

        protected virtual void DistributedTranspose(CustomFftPlan data, UnsafeArray src, UnsafeArray dst)
        {
            FftwMpi.ExecuteR2r(data.TransposePlan.Handler, new IntPtr(src.Ptr), new IntPtr(dst.Ptr));
            //int size = data.NumberOfLocalFftsAlongSecondDimension * data.LocalNx;
            //_mpi.AllToAll(data.Output.Ptr, size, data.Input.Ptr, size);
            //All2All
        }

        private static void Iterate(int length, Action<int> action)
        {
            var options = MultiThreadUtils.CreateParallelOptions();
            System.Threading.Tasks.Parallel.For(0, length, options, action);
        }

    }
}
