using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FftWrap.Numerics;
using Porvem.Parallel;

namespace FftWrap
{
    public class DistributedPlan : IDisposable
    {
        private static readonly IntPtr DefaultBlock = new IntPtr(0);

        private readonly IntPtr _forwardPlan;
        private readonly IntPtr _backwardPlan;

        private readonly int _localN0Size;
        private readonly int _localN0Start;

        private readonly int _fullSize1;
        private readonly int _fullSize2;

        private readonly int _interleaved;

        private NativeMatrix<SingleComplex> _data;

        private bool _isDisposed = false;

        public DistributedPlan(IntPtr forwardPlan, IntPtr backwardPlan, int localN0Size, int localN0Start, int fullSize1, int fullSize2, int interleaved, NativeMatrix<SingleComplex> data)
        {
            _forwardPlan = forwardPlan;
            _backwardPlan = backwardPlan;
            _localN0Size = localN0Size;
            _localN0Start = localN0Start;
            _fullSize1 = fullSize1;
            _fullSize2 = fullSize2;
            _interleaved = interleaved;
            _data = data;
        }

        public int LocalSize1Start
        {
            get { return _localN0Start; }
        }

        public int LocalSize1
        {
            get { return _localN0Size; }
        }

        public int FullSize1
        {
            get { return _fullSize1; }
        }

        public int FullSize2
        {
            get { return _fullSize2; }
        }

        public int Interleaved
        {
            get { return _interleaved; }
        }

        public static DistributedPlan CreateNewPlan2D(IntPtr mpiCommunicator, int size1, int size2, int numberOfInterleaved)
        {
            IntPtr localN0;
            IntPtr localN0Start;

            var n = new[] { new IntPtr(size1), new IntPtr(size2), };


            IntPtr localSize = FftwMpi.LocalSizeMany(2, n, new IntPtr(numberOfInterleaved), DefaultBlock, mpiCommunicator, out localN0, out localN0Start);

            IntPtr srcPtr = Fftw.AllocComplex(localSize);

            var matrix = new NativeMatrix<SingleComplex>(srcPtr, (int)localN0, size2, numberOfInterleaved);

            //IntPtr tblock = new IntPtr(localN0.ToInt32() * size2);

            var planF = FftwMpi.PlanManyDft(2, n, new IntPtr(numberOfInterleaved), DefaultBlock, DefaultBlock, srcPtr, srcPtr, Mpi.CommWorld, (int)Direction.Forward, (uint)Flags.Estimate);
            var planB = FftwMpi.PlanManyDft(2, n, new IntPtr(numberOfInterleaved), DefaultBlock, DefaultBlock, srcPtr, srcPtr, Mpi.CommWorld, (int)Direction.Backward, (uint)Flags.Estimate);


            return new DistributedPlan(planF, planB, (int)localN0, (int)localN0Start, size1, size2, numberOfInterleaved, matrix);
        }

        public static DistributedPlan CreateNewPlan2D(IntPtr mpiCommunicator, int size1, int size2)
        {
            IntPtr localN0;
            IntPtr localN0Start;

            var n = new[] { new IntPtr(size1), new IntPtr(size2), };

            IntPtr localSize = FftwMpi.LocalSize2D(n[0], n[1], mpiCommunicator, out localN0, out localN0Start);

            IntPtr srcPtr = Fftw.AllocComplex(localSize);

            var matrix = new NativeMatrix<SingleComplex>(srcPtr, (int)localN0, size2);

            var planF = FftwMpi.PlanDft2D(n[0], n[1], srcPtr, srcPtr, Mpi.CommWorld, (int)Direction.Forward, (uint)Flags.Estimate);
            var planB = FftwMpi.PlanDft2D(n[0], n[1], srcPtr, srcPtr, Mpi.CommWorld, (int)Direction.Backward, (uint)Flags.Estimate);


            return new DistributedPlan(planF, planB, (int)localN0, (int)localN0Start, size1, size2, 1, matrix);
        }


        public void SetAllValuesTo(SingleComplex value)
        {
            if (_interleaved == 1)
            {
                for (int i = 0; i < _localN0Size; i++)
                    for (int j = 0; j < _fullSize2; j++)
                        _data[i, j] = value;
            }
            else
            {
                for (int i = 0; i < _localN0Size; i++)
                    for (int j = 0; j < _fullSize2; j++)
                        for (int k = 0; k < _interleaved; k++)
                            _data[i, j, k] = value;
            }
        }

        public void SetAllValuesTo(int k, SingleComplex value)
        {
            for (int i = 0; i < _localN0Size; i++)
                for (int j = 0; j < _fullSize2; j++)
                    _data[i, j, k] = value;
        }

        public void SetValue(int i, int j, SingleComplex value)
        {
            if (i >= _localN0Start && i < _localN0Start + _localN0Size)
                _data[i - _localN0Start, j] = value;
        }

        public SingleComplex? GetValue(int i, int j)
        {
            if (i >= _localN0Start && i < _localN0Start + _localN0Size)
                return _data[i - _localN0Start, j];

            return null;
        }

        public void SetValue(int i, int j, int k, SingleComplex value)
        {
            if (i >= _localN0Start && i < _localN0Start + _localN0Size)
                _data[i - _localN0Start, j, k] = value;
        }

        public SingleComplex? GetValue(int i, int j, int k)
        {
            if (i >= _localN0Start && i < _localN0Start + _localN0Size)
                return _data[i - _localN0Start, j, k];

            return null;
        }

        public void RunForward()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().ToString());

            Fftw.Execute(_forwardPlan);
        }

        public void RunBackward()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().ToString());

            Fftw.Execute(_backwardPlan);
        }

        ~DistributedPlan()
        {
            Dispose();
        }

        public void Dispose()
        {
            _isDisposed = true;

            Fftw.Free(_data.Ptr);
            Fftw.DestroyPlan(_forwardPlan);
            Fftw.DestroyPlan(_backwardPlan);

            GC.SuppressFinalize(this);
        }
    }
}

