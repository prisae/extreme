using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;

namespace Extreme.Parallel
{
    [SuppressUnmanagedCodeSecurity]
    internal static partial class UnsafeNativeMethods
    {
        private const string MpiWrapper = @"ntv_mpi";

        [DllImport(MpiWrapper)]
        public static extern unsafe int AllToAllDoubleComplex(Complex* sendbuf, int sendcount, 
            Complex* recvbuf, int recvcount, IntPtr comm);

        [DllImport(MpiWrapper)]
        public static extern unsafe int AllToAllDoubleComplexInPlace(Complex* recvbuf, int recvcount, IntPtr comm);

        [DllImport(MpiWrapper)]
		unsafe public static extern int GatherV(Complex* sendbuf, int size, Complex* rbuf, int* recvcounts, int* displs, IntPtr comm);

        [DllImport(MpiWrapper)]
		unsafe public static extern int AllGatherV(Complex* sendbuf, int size, Complex* rbuf, int* recvcounts, int* displs, IntPtr comm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Gather")]
        public static extern unsafe int Gather(Complex* sendbuf, int sendcount, Complex* recvbuf, int recvcount, int root, IntPtr comm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AllReduce")]
        public static extern unsafe int AllReduce(void* sendbuf, void* recvbuf, int count, IntPtr datatype, IntPtr comm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Reduce")]
        public static extern unsafe int Reduce(void* sendbuf, void* recvbuf, int count, IntPtr datatype, IntPtr comm);

		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LogicalReduce")]
		public static extern unsafe int LogicalReduce(void *sendbuf, void *recvbuf, int count, IntPtr comm);


		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CommDup")]
		public static extern unsafe int CommDup(IntPtr comm,  IntPtr* newcomm);

		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CommSplit")]
		public static extern unsafe int CommSplit(IntPtr comm, int color , int key,IntPtr* newcomm);


        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CommCreate")]
        public static extern unsafe int CommCreate(IntPtr comm, IntPtr group, IntPtr* newcomm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GroupIncl")]
        public static extern unsafe int GroupIncl(IntPtr group, int n, [In] int[] ranks, IntPtr* newgroup);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CommGroup")]
        public unsafe static extern int CommGroup(IntPtr comm, IntPtr* group);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Init")]
		private unsafe static extern int InitNative(int* threads_on);

		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetThreadSupportLevels")]
		private unsafe static extern int GetThreadSupportLevels(int* sup_levels);


		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CommFree")]
		public unsafe static extern int CommFree(IntPtr* comm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Finalize")]
        private static extern int FinalizeNative();

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetProcessorName")]
        unsafe public static extern int GetProcessorName(byte* name, ref int resultlen);


		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCommWorldRank")]
		unsafe private static extern int GetCommWorldRank(int* rank);

		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCommWorldSize")]
		unsafe private static extern int GetCommWorldSize(int* size);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCommRank")]
        unsafe public static extern int GetCommRank(IntPtr comm, int* rank);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCommSize")]
		unsafe public static extern int GetCommSize(IntPtr comm, int* size);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Bcast")]
        public static extern unsafe int Bcast(void* buffer, int count, IntPtr datatype, int root, IntPtr comm);

		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Scatter")]
		public static extern unsafe int Scatter(void* sendbuf,void* recvbuf, int count, IntPtr datatype, int root, IntPtr comm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Barrier")]
        public static extern int Barrier(IntPtr comm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Send")]
        unsafe public static extern int Send(void* data, int count, IntPtr datatype, int dest, int tag, IntPtr comm);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Recv")]
        unsafe public static extern int Recv(void* data, int count, IntPtr datatype, int source, int tag, IntPtr comm, out int actualSource);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetErrorString")]
        private static extern unsafe int GetErrorString(int errorcode, byte* str, int* resultlen);

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCommWorld")]
        public static extern IntPtr GetCommWorld();

		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCommNull")]
		public static extern IntPtr GetCommNull();


        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMpiInt")]
        public static extern IntPtr GetMpiInt();

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMpiFloat")]
        public static extern IntPtr GetMpiFloat();

        [DllImport(MpiWrapper)]
        public static extern IntPtr GetMpiDouble();

       
        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMpiDoubleComplex")]
        public static extern IntPtr GetMpiDoubleComplex();

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMpiOpSum")]
        public static extern IntPtr GetMpiOpSum();
          
        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMaxProcessorName")]
        public static extern int GetMaxProcessorName();

        [DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMpiAnySource")]
        public static extern int GetMpiAnySource();

		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMpiInPlace")]
		public static extern IntPtr GetMpiInPlace();


		[DllImport(MpiWrapper, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CommunicatorC2Fortran")]
		public static extern unsafe long  CommunicatorC2Fortran(IntPtr comm);

    }
}
