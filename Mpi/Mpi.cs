using System;
using System.Numerics;
using System.Text;

using static Extreme.Parallel.UnsafeNativeMethods;
using UNM = Extreme.Parallel.UnsafeNativeMethods;
using System.ComponentModel.Design;
using System.Runtime.Remoting.Messaging;
using System.Runtime.ConstrainedExecution;

namespace Extreme.Parallel
{
    public unsafe class Mpi : IDisposable
    {
        public readonly int Master = 0;

        private static readonly IntPtr CommWorld = GetCommWorld();

		private static readonly IntPtr CommNull =GetCommNull();

        public static readonly IntPtr Int = GetMpiInt();
        public static readonly IntPtr Float = GetMpiFloat();
        public static readonly IntPtr Double = GetMpiDouble();
        public static readonly IntPtr Complex = GetMpiDoubleComplex();
        public static readonly int AnySource = GetMpiAnySource();
		public static readonly IntPtr InPlace=GetMpiInPlace();
        public static readonly IntPtr MpiOpSumm = GetMpiOpSum();


        private readonly int _rank = 0;
        private readonly int _size = 1;
        private Mpi()
        {
            UnsafeNativeMethods.Init();


			IntPtr comm;
			Communicator = CommWorld;

			int rank;
			int size;

			UnsafeNativeMethods.GetCommRank (Communicator,&rank);
			UnsafeNativeMethods.GetCommSize (Communicator,&size);
			_rank = rank;
			_size = size;
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
		private Mpi(IntPtr comm)
		{
			if (_initialized) {
				Communicator = comm;

				int rank;
				int size;
				int err;
				err = UnsafeNativeMethods.GetCommRank (comm, &rank);
					
				if (err != 0)
					throw new InvalidOperationException (GetErrorString (err));
				_rank = rank;

				err = UnsafeNativeMethods.GetCommSize (comm, &size);
				if (err != 0)
					throw new InvalidOperationException (GetErrorString (err));

				_size = size;
			} else {
				
			}
		}
		public Mpi Merge (params int[] ranks)
		{
			var comm=CreateNewCommunicator(ranks);
			if (comm != CommNull)
				return new Mpi (comm);
			else
				return null;
		}

		public Mpi Separate(int color){
			IntPtr comm;
			UNM.CommSplit (Communicator, color, Rank, &comm);
			return new Mpi (comm);
		}


        public bool IsParallel => Size > 1;
        public int Size => _size;
        public int Rank => _rank;
        public bool IsMaster => _rank == Master;
		public IntPtr Communicator { get; private set; }=CommNull;

        private static void WithErrorHandling(int err)
        {
            if (err != 0)
                throw new InvalidOperationException(GetErrorString(err));
        }

        private IntPtr CreateNewCommunicator(params int[] ranks)
        {
            IntPtr group;
            CommGroup(Communicator, &@group);

            IntPtr newGroup;
            GroupIncl(@group, ranks.Length, ranks, &newGroup);

            IntPtr newComm;
			CommCreate(Communicator, newGroup, &newComm);

            return newComm;
        }


		public Mpi Dup(){
			IntPtr comm;

			UNM.CommDup (this.Communicator, &comm);
			return new Mpi (comm);
		}

        public Complex AllReduce(Complex value)
        {
            Complex result;
			UnsafeNativeMethods.AllReduce(&value, &result, 1, Complex, Communicator);

            return result;
        }

        public double AllReduce(double value)
        {
            double result;
			UnsafeNativeMethods.AllReduce(&value, &result, 1, Double, Communicator);
            return result;
        }

		public int AllReduce(int value)
		{
			int result;
			UnsafeNativeMethods.AllReduce(&value, &result, 1, Int, Communicator);
			return result;
		}


        public void Reduce(double* value, double* result, int length)
        {
			if (result == value && IsMaster) {
				UnsafeNativeMethods.Reduce ((void*)InPlace, result, length, Double, Communicator);
			} else {
				UnsafeNativeMethods.Reduce (value, result, length, Double, Communicator);
			}
        }

		public void LogicalReduce(double* value, double* result, int length)
		{

			if (result == value && IsMaster) {
				UnsafeNativeMethods.LogicalReduce ((void*)InPlace, result, length,  Communicator);
			} else {
				UnsafeNativeMethods.LogicalReduce (value, result, length,  Communicator);
			}
		}

		public static long CommunicatorC2Fortran(IntPtr comm)
		{
			return UnsafeNativeMethods.CommunicatorC2Fortran(comm);
		}

		public long CommunicatorC2Fortran()
		{
			return CommunicatorC2Fortran(this.Communicator);
		}

		public static void Barrier(IntPtr comm)
            => WithErrorHandling(UnsafeNativeMethods.Barrier(comm));

		public void Barrier()
		=> WithErrorHandling(UnsafeNativeMethods.Barrier(Communicator));

        public int BroadCast(int root, int value)
        {
			Bcast(&value, 1, Int, root, Communicator);
           return value;
        }

        public void AllGatherV(Complex* src, int sendSize, Complex* dst, int[] rCounts, int[] rDispl)
        {
            fixed (int* cntPtr = &rCounts[0], dsplPtr = &rDispl[0])
			UnsafeNativeMethods.AllGatherV(src, sendSize, dst, cntPtr, dsplPtr,Communicator);
        }

        public void GatherV(Complex* src, int sendSize, Complex* dst, int[] rCounts, int[] rDispl)
        {
            fixed (int* cntPtr = &rCounts[0], dsplPtr = &rDispl[0])
			UnsafeNativeMethods.GatherV(src, sendSize, dst, cntPtr, dsplPtr,Communicator);
        }

        public void BroadCast( int root, double[] values)
        {
            fixed (double* ptr = &values[0])
			Bcast(ptr, values.Length, Double, root, Communicator);
        }

		public void BroadCast( int root, int[,,] values)
		{
			fixed (int* ptr = &values[0,0,0])
				BroadCast ( root, ptr, values.Length);
		}


        public void BroadCast(int root, Complex[] values)
        {
            fixed (Complex* ptr = &values[0])
			Bcast(ptr, values.Length, Complex, root, Communicator);
        }

        public void BroadCast(int root, double* values, int length)
        {
            Bcast(values, length, Double, root, Communicator);
        }


        public void BroadCast(int root, Complex* values, int length)
        {
			Bcast(values, length, Complex, root, Communicator);
        }

		public void BroadCast( int root, int* values, int length)
		{
			Bcast(values, length, Int, root, Communicator);
		}



        public int Send(void* data, int count, IntPtr datatype, int dest, int tag)
		=> UNM.Send(data, count, datatype, dest, tag, Communicator);

        public int Recv(void* data, int count, IntPtr datatype, int source, int tag, out int actualSource)
		=> UNM.Recv(data, count, datatype, source, tag, Communicator, out actualSource);

        public static string GetErrorString(int error)
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

	
        public void Gather(Complex* dst, Complex* src, int dstSize, int srcSize)
        {
			UNM.Gather(src, srcSize, dst, dstSize, Master, Communicator);
        }

        public void Gather(Complex[] dst, Complex src)
        {
            fixed (Complex* dstPtr = &dst[0])
				UNM.Gather(&src, 1, dstPtr, 1, Master, Communicator);
        }

        public void AllToAll(Complex* buffer, int size)
		=> AllToAllDoubleComplexInPlace(buffer, size, Communicator);

        public void AllToAll(Complex* src, int srcSize, Complex* dst, int dstSize)
		=> AllToAllDoubleComplex(src, srcSize, dst, dstSize, Communicator);


		public void Free(){
			IntPtr comm = Communicator;
			if (Communicator!=CommNull&&Communicator!=CommWorld)
				UnsafeNativeMethods.CommFree(&comm);
			Communicator = CommNull;
		}

        public void Dispose()
        {
           // FinalizeMpi();
			if (Communicator == CommWorld)
				Finalize ();
			Free ();
        }

		private static void Finalize()
		{
			FinalizeMpi ();
		}
    }
}
