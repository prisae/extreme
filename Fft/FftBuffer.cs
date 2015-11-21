using System;
using System.Linq;
using System.Numerics;
using FftWrap;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.FftW;
using Extreme.Core;
using Extreme.Parallel;


namespace Extreme.Cartesian.Fft
{
    public unsafe class FftBuffer //: IDisposable
    {
        private readonly INativeMemoryProvider _memoryProvider;
        private readonly bool _planForSigma;
        private readonly IProfiler _profiler;

        private Complex* _inputBuffer;
        private Complex* _outputBuffer;

        public IFftBufferPlan Plan3Nz { get; private set; }
        public IFftBufferPlan Plan1Nz { get; private set; }
        public IFftBufferPlan Plan3 { get; private set; }
        public IFftBufferPlan Plan1 { get; private set; }

        public IFftBufferPlan PlanSigma { get; private set; }

        private CustomDistributedFft CustomFft { get; set; }
        private FftWTransform LocalFft { get; set; }

        public IntPtr RealModelPartCommunicator { get; private set; }

        private bool IsParallel { get; set; }

        public FftBuffer(INativeMemoryProvider memoryProvider, bool planForSigma, IProfiler profiler = null)
        {
            if (memoryProvider == null) throw new ArgumentNullException(nameof(memoryProvider));

            _memoryProvider = memoryProvider;
            _planForSigma = planForSigma;
            _profiler = profiler;
        }

        public void ExecuteForward(IFftBufferPlan plan)
        {
            if (IsParallel)
                CustomFft.ComputeForward((plan as CustomBuffer)?.CustomFftPlan);
            else
                LocalFft.ExecutePlan((plan as LocalFftwBuffer)?.ForwardPlan, new IntPtr(plan.Buffer1Ptr), new IntPtr(plan.Buffer1Ptr));
        }

        public void ExecuteBackward(IFftBufferPlan plan)
        {
            if (IsParallel)
                CustomFft.ComputeBackward((plan as CustomBuffer)?.CustomFftPlan);
            else
                LocalFft.ExecutePlan((plan as LocalFftwBuffer)?.BackwardPlan, new IntPtr(plan.Buffer2Ptr), new IntPtr(plan.Buffer2Ptr));
        }

        public void AllocateBuffersAndCreatePlansParallel(int nx, int ny, int nz, Mpi mpi)
        {
            CustomFft = new CustomDistributedFft(mpi, _profiler);
            IsParallel = true;

            var localSize3Nz = CustomDistributedFft.GetLocalSize(mpi, nx * 2, ny * 2, 3 * nz);

            var ranks = Enumerable.Range(0, mpi.Size / 2).ToArray();
            RealModelPartCommunicator = mpi.CreateNewCommunicator(ranks);

            _inputBuffer = _memoryProvider.AllocateComplex(localSize3Nz);
            _outputBuffer = _memoryProvider.AllocateComplex(localSize3Nz);

            Plan3Nz = CreatePlan2D(CustomFft, mpi, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 3 * nz);
            Plan1Nz = CreatePlan2D(CustomFft, mpi, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 1 * nz);
            Plan3 = CreatePlan2D(CustomFft, mpi, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 3);
            Plan1 = CreatePlan2D(CustomFft, mpi, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 1);

#warning CheckPlan sizes!!!
            if (_planForSigma)
                PlanSigma = CreatePlan3D(CustomFft, mpi, _inputBuffer, _outputBuffer, nx * 2, ny * 2, nz);
        }

        public void AllocateBuffersAndCreatePlansLocal(int nx, int ny, int nz)
        {
            if (_inputBuffer != null || _outputBuffer != null)
                throw new InvalidOperationException("This is allowed only once");

            LocalFft = new FftWTransform(MultiThreadUtils.MaxDegreeOfParallelism);
            IsParallel = false;

            var localSize = nx * 2 * ny * 2 * 3 * nz;

            _inputBuffer = _memoryProvider.AllocateComplex(localSize);
            _outputBuffer = _memoryProvider.AllocateComplex(localSize);

            Plan3Nz = CreatePlan2D(LocalFft, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 3 * nz);
            Plan1Nz = CreatePlan2D(LocalFft, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 1 * nz);
            Plan3 = CreatePlan2D(LocalFft, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 3);
            Plan1 = CreatePlan2D(LocalFft, _inputBuffer, _outputBuffer, nx * 2, ny * 2, 1);

#warning CheckPlan sizes!!!
            if (_planForSigma)
                PlanSigma = CreatePlan3D(LocalFft, _inputBuffer, _outputBuffer, nx * 2, ny * 2, nz);

        }

        #region  Buffer Plans

        private class Buffer : IFftBufferPlan
        {
            protected Buffer(Complex* buffer1Ptr, Complex* buffer2Ptr, int bufferLength)
            {
                Buffer1Ptr = buffer1Ptr;
                Buffer2Ptr = buffer2Ptr;
                BufferLength = bufferLength;
            }

            public Complex* Buffer1Ptr { get; }
            public Complex* Buffer2Ptr { get; }
            public int BufferLength { get; }
        }

        private class LocalFftwBuffer : Buffer
        {
            public LocalFftwBuffer(Complex* buffer1Ptr, Complex* buffer2Ptr, int bufferLength, FftwPlan forwardPlan, FftwPlan backwardPlan) : base(buffer1Ptr, buffer2Ptr, bufferLength)
            {
                ForwardPlan = forwardPlan;
                BackwardPlan = backwardPlan;
            }

            public FftwPlan ForwardPlan { get; }
            public FftwPlan BackwardPlan { get; }
        }

        private class CustomBuffer : Buffer
        {
            public CustomBuffer(Complex* buffer1Ptr, Complex* buffer2Ptr, int bufferLength, CustomFftPlan customFftPlan) : base(buffer1Ptr, buffer2Ptr, bufferLength)
            {
                CustomFftPlan = customFftPlan;
            }

            public CustomFftPlan CustomFftPlan { get; }
        }

        #endregion

        private static IFftBufferPlan CreatePlan2D(FftWTransform fft, Complex* input, Complex* output, int nx, int ny, int nz)
        {
            var forwardPlan
                = fft.CreatePlan2D(new IntPtr(input), new IntPtr(input), nx, ny, nz, Direction.Forward, Flags.Measure);

            var backwardPlan
                = fft.CreatePlan2D(new IntPtr(input), new IntPtr(input), nx, ny, nz, Direction.Backward, Flags.Measure);

            return new LocalFftwBuffer(input, output, nx * ny * nz, forwardPlan, backwardPlan);
        }


        private static IFftBufferPlan CreatePlan3D(FftWTransform fft, Complex* input, Complex* output, int nx, int ny, int nz)
        {
            var forwardPlan
                = fft.CreatePlan3D(new IntPtr(input), new IntPtr(input), nx, ny, nz, Direction.Forward, Flags.Measure);

            var backwardPlan
                = fft.CreatePlan3D(new IntPtr(input), new IntPtr(input), nx, ny, nz, Direction.Backward, Flags.Measure);

            return new LocalFftwBuffer(input, output, nx * ny * nz, forwardPlan, backwardPlan);
        }

        private static IFftBufferPlan CreatePlan2D(CustomDistributedFft fft, Mpi mpi, Complex* input, Complex* output, int nx, int ny, int nz)
        {
            var localSize = CustomDistributedFft.GetLocalSize(mpi, nx, ny, nz);
            var plan = fft.CreatePlan2D(input, output, mpi, nx, ny, nz);
            return new CustomBuffer(input, output, localSize, plan);
        }

        private static IFftBufferPlan CreatePlan3D(CustomDistributedFft fft, Mpi mpi, Complex* input, Complex* output, int nx, int ny, int nz)
        {
            var localSize = CustomDistributedFft.GetLocalSize(mpi, nx, ny, nz);
            var plan = fft.CreatePlan3D(input, output, mpi, nx, ny, nz);
            return new CustomBuffer(input, output, localSize, plan);
        }

        public void Dispose()
        {
            //if (IsParallel)
            //{
            //    Fft.DestroyPlan(ForwardPlan3Nz);
            //    Fft.DestroyPlan(BackwardPlan3Nz);
            //}
            //else
            //{
            //    LocalFft.DestroyPlan(ForwardPlan3Nz);
            //    LocalFft.DestroyPlan(BackwardPlan3Nz);
            //}

            _memoryProvider.Release(_inputBuffer);
            _memoryProvider.Release(_outputBuffer);
        }
    }
}
