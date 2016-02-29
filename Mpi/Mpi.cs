using System;
using System.Numerics;
using System.Text;

using static Extreme.Parallel.UnsafeNativeMethods;
using UNM = Extreme.Parallel.UnsafeNativeMethods;
using System.ComponentModel.Design;

namespace Extreme.Parallel
{
    public unsafe class Mpi : IDisposable
    {
        public const int Master = 0;

        public static readonly IntPtr CommWorld = GetCommWorld();
        public static readonly IntPtr Int = GetMpiInt();
        public static readonly IntPtr Float = GetMpiFloat();
        public static readonly IntPtr Double = GetMpiDouble();
        public static readonly IntPtr Complex = GetMpiDoubleComplex();
        public static readonly int AnySource = GetMpiAnySource();

        public static readonly IntPtr MpiOpSumm = GetMpiOpSum();


        private readonly int _rank = -1;
        private readonly int _size = -1;
        private Mpi()
        {
            UnsafeNativeMethods.Init();

            _rank = GetWorldRank();
            _size = GetWorldSize();
        }

        private static bool _initialized = false;

        public static Mpi Init()
        {
            if (_initialized)
                throw new InvalidOperationException("Mpi service is already initialized");

            try
            {
                return new Mpi();
            }
            finally
            {
                _initialized = true;
            }
        }

        public bool IsParallel => Size > 1;
        public int Size => _size;
        public int Rank => _rank;
        public bool IsMaster => _rank == 0;
		public IntPtr Communicator { get; private set; }=CommWorld;

        private void WithErrorHandling(int err)
        {
            if (err != 0)
                throw new InvalidOperationException(GetErrorString(err));
        }

        public IntPtr CreateNewCommunicator(params int[] ranks)
        {
            IntPtr group;
            CommGroup(CommWorld, &@group);

            IntPtr newGroup;
            GroupIncl(@group, ranks.Length, ranks, &newGroup);

            IntPtr newComm;
            CommCreate(CommWorld, newGroup, &newComm);

            return newComm;
        }

        public Complex AllReduce(IntPtr comm, Complex value)
        {
            Complex result;
            UnsafeNativeMethods.AllReduce(&value, &result, 1, Complex, comm);

            return result;
        }

        public double AllReduce(IntPtr comm, double value)
        {
            double result;
            UnsafeNativeMethods.AllReduce(&value, &result, 1, Double, comm);
            return result;
        }

		public int AllReduce(IntPtr comm, int value)
		{
			int result;
			UnsafeNativeMethods.AllReduce(&value, &result, 1, Int, comm);
			return result;
		}


        public void Reduce(IntPtr comm, double* value, double* result, int length)
        {
            UnsafeNativeMethods.Reduce(value, result, length, Double, comm);
        }

		public void LogicalReduce(IntPtr comm, double* value, double* result, int length)
		{
			UnsafeNativeMethods.LogicalReduce(value, result, length, comm);
		}

		public long CommunicatorC2Fortran(IntPtr comm)
		{
			return UnsafeNativeMethods.CommunicatorC2Fortran(comm);
		}

		public long CommunicatorC2Fortran()
		{
			return CommunicatorC2Fortran(this.Communicator);
		}

        public void Barrier(IntPtr comm)
            => WithErrorHandling(UnsafeNativeMethods.Barrier(comm));

        public int BroadCast(IntPtr comm, int root, int value)
        {
            Bcast(&value, 1, Int, root, comm);
            return value;
        }

        public void AllGatherV(Complex* src, int sendSize, Complex* dst, int[] rCounts, int[] rDispl)
        {
            fixed (int* cntPtr = &rCounts[0], dsplPtr = &rDispl[0])
                UnsafeNativeMethods.AllGatherV(src, sendSize, dst, cntPtr, dsplPtr);
        }

        public void GatherV(Complex* src, int sendSize, Complex* dst, int[] rCounts, int[] rDispl)
        {
            fixed (int* cntPtr = &rCounts[0], dsplPtr = &rDispl[0])
                UnsafeNativeMethods.GatherV(src, sendSize, dst, cntPtr, dsplPtr);
        }

        public void BroadCast(IntPtr comm, int root, double[] values)
        {
            fixed (double* ptr = &values[0])
                Bcast(ptr, values.Length, Double, root, comm);
        }

        public void BroadCast(IntPtr comm, int root, Complex[] values)
        {
            fixed (Complex* ptr = &values[0])
                Bcast(ptr, values.Length, Complex, root, comm);
        }

        public void BroadCast(IntPtr comm, int root, double* values, int length)
        {
            Bcast(values, length, Double, root, comm);
        }


        public void BroadCast(IntPtr comm, int root, Complex* values, int length)
        {
            Bcast(values, length, Complex, root, comm);
        }


        public void Barrier()
            => Barrier(CommWorld);

        public int Send(void* data, int count, IntPtr datatype, int dest, int tag, IntPtr comm)
            => UNM.Send(data, count, datatype, dest, tag, comm);

        public int Recv(void* data, int count, IntPtr datatype, int source, int tag, IntPtr comm, out int actualSource)
            => UNM.Recv(data, count, datatype, source, tag, comm, out actualSource);

        public string GetErrorString(int error)
            => UNM.GetErrorString(error);

        public string GetProcessorName()
        {
            var name = new byte[GetMaxProcessorName()];
            int length = 0;

            fixed (byte* namePtr = &name[0])
            {
                int err = UNM.GetProcessorName(namePtr, ref length);
            }

            return Encoding.ASCII.GetString(name, 0, length);
        }

        public void Gather(IntPtr comm, Complex* dst, Complex* src, int dstSize, int srcSize)
        {
            UNM.Gather(src, srcSize, dst, dstSize, Master, comm);
        }

        public void Gather(IntPtr comm, Complex[] dst, Complex src)
        {
            fixed (Complex* dstPtr = &dst[0])
                UNM.Gather(&src, 1, dstPtr, 1, Master, comm);
        }

        public void AllToAll(Complex* buffer, int size)
            => AllToAllDoubleComplexInPlace(buffer, size, CommWorld);

        public void AllToAll(Complex* src, int srcSize, Complex* dst, int dstSize)
            => AllToAllDoubleComplex(src, srcSize, dst, dstSize, CommWorld);

        public void Dispose()
        {
            FinalizeMpi();
        }
    }
}
