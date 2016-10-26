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

		private static readonly IntPtr CommWorld; //= GetCommWorld();

		private static readonly IntPtr CommNull;// =GetCommNull();

		public static readonly IntPtr Int; 
		public static readonly IntPtr Float; 
		public static readonly IntPtr Double;
		public static readonly IntPtr Complex;
		public static readonly int AnySource; 
		public static readonly IntPtr InPlace;
		public static readonly IntPtr MpiOpSumm;
		private static readonly bool _mpiexists=true;

        private readonly int _rank = 0;
        private readonly int _size = 1;
        private Mpi() {
			
			IntPtr comm;
			if (_mpiexists) {
				Communicator = CommWorld;

				int rank;
				int size;

				UnsafeNativeMethods.GetCommRank (Communicator, &rank);
				UnsafeNativeMethods.GetCommSize (Communicator, &size);
				_rank = rank;
				_size = size;
			} else {
				_rank = 0;
				_size = 1;
			}
		}



        private static bool _initialized = false;

		static Mpi(){
			try{
				UnsafeNativeMethods.Init();
				CommWorld = GetCommWorld();
				CommNull =GetCommNull();
				Int = GetMpiInt();
				Float = GetMpiFloat();
				Double = GetMpiDouble();
				Complex = GetMpiDoubleComplex();
				AnySource = GetMpiAnySource();
				InPlace=GetMpiInPlace();
				MpiOpSumm = GetMpiOpSum();
				_initialized = true;
			}catch (DllNotFoundException){
				_mpiexists=false;
			}
		}


        public static Mpi Init()
        {
			
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
			if (_mpiexists) {
				UNM.CommDup (this.Communicator, &comm);
				return new Mpi (comm);
			} else {
				return new Mpi();
			}
		}

        public Complex AllReduce(Complex value)
        {
            Complex result;
			if (_mpiexists) {
				UnsafeNativeMethods.AllReduce(&value, &result, 2, Double, Communicator);
			} else {
				result = value;
			}
           	return result;
        }

		public void AllReduceInPlace(Complex[] values)
		{
			fixed(Complex* ptr=&values[0]){
				UnsafeNativeMethods.AllReduceInPlace(ptr, 2*values.Length, Double, Communicator);
			}


		}



        public double AllReduce(double value)
        {
            double result;
			if (_mpiexists) {
				UnsafeNativeMethods.AllReduce (&value, &result, 1, Double, Communicator);
			} else {
				result = value;
			}
            return result;
        }

		public int AllReduce(int value)
		{
			int result;
			Console.WriteLine("int");
			if (_mpiexists) {
				UnsafeNativeMethods.AllReduce(&value, &result, 1, Int, Communicator);
			} else {
				result = value;
			}
			return result;
		}


        public void Reduce(double* value, double* result, int length)
        { 
			if (_mpiexists) {
				if (result == value && IsMaster) {
					UnsafeNativeMethods.Reduce ((void*)InPlace, result, length, Double, Communicator);
				} else {
					UnsafeNativeMethods.Reduce (value, result, length, Double, Communicator);
				}
			} else {
				if (result == value) {
					return;
				} else {
					for (int i = 0; i < length; i++) {
						result [i] = value [i];
					}
				}
			}
        }
		public void Reduce(double[] value, double[] result)
		{
			if (_mpiexists) {
				fixed(double* pv=&value[0], ptr=&result[0]) {
					if (pv == ptr && IsMaster) {
						UnsafeNativeMethods.Reduce ((void*)InPlace, ptr, value.Length, Double, Communicator);
					} else {
						UnsafeNativeMethods.Reduce (pv, ptr, value.Length, Double, Communicator);
					}
				}
			} else {
				if (result == value) {
					return;
				} else {
					for (int i = 0; i < result.Length; i++) {
						result [i] = value [i];
					}
				}
			}
		}


		public double Reduce(double value)
		{
			double result = value;
			if (_mpiexists)
				UnsafeNativeMethods.Reduce (&value, &result,1, Double, Communicator);
			return result;
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

        public int BroadCast(int root, int value=0)
        {
			var tmp = value;
			Bcast(&tmp, 1, Int, root, Communicator);
           return tmp;
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

		public int MasterBroadCast(int value=0)
		{
			var tmp = value;
			if (_mpiexists)
				Bcast(&tmp, 1, Int, Master, Communicator);
			return tmp;
		}



		public void MasterBroadCast(int[,] data)
		{
			var len = data.Length;
			fixed(int* ptr=&data[0,0]) {
				Bcast (ptr, len, Int, Master, Communicator);
			}

		}

		public void MasterBroadCast(double[,] data)
		{
			var len = data.Length;
			fixed(double* ptr=&data[0,0]) {
				Bcast (ptr, len, Double, Master, Communicator);
			}

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
			if (_mpiexists) {
				UNM.Gather (src, srcSize, dst, dstSize, Master, Communicator);
			} else {
				if (src != dst) {
					for (int i = 0; i < srcSize; i++)
						dst [i] = src [i];
				}
			}
        }

        public void Gather(Complex[] dst, Complex src)
        {
			if (_mpiexists){
            	fixed (Complex* dstPtr = &dst[0])
					UNM.Gather(&src, 1, dstPtr, 1, Master, Communicator);
			} else {
				dst [0] = src;
			}
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
			if (_mpiexists) {
				if (Communicator == CommWorld)
					Finalize ();
					Free ();
			}
        }

		private static void Finalize()
		{
			if (_mpiexists)
				FinalizeMpi ();
		}
    }
}
