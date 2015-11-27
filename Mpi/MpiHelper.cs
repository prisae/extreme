using System;

namespace Extreme.Parallel
{
    public static class MpiHelper
    {
        public unsafe static void Send(this Mpi mpi, IntPtr data, int count, IntPtr datatype, int dest, int tag, IntPtr comm)
        {
            mpi.Send(data.ToPointer(), count, datatype, dest, tag, comm);
        }

        public unsafe static void Send(this Mpi mpi, int data, int dest, int tag, IntPtr comm)
        {
            mpi.Send(&data, 1, Mpi.Int, dest, tag, comm);
        }

        public unsafe static void Send(this Mpi mpi, double data, int dest, int tag, IntPtr comm)
        {
            mpi.Send(&data, 1, Mpi.Double, dest, tag, comm);
        }

        public unsafe static int RecvInt(this Mpi mpi, int source, int tag, IntPtr comm)
        {
            int result;
            int actualSource;
            int err = mpi.Recv(&result, 1, Mpi.Int, source, tag, comm, out actualSource);
            return result;
        }

        public unsafe static int RecvInt(this Mpi mpi, int source, int tag, IntPtr comm, out int actualSource)
        {
            int result;
            int err = mpi.Recv(&result, 1, Mpi.Int, source, tag, comm, out actualSource);
            return result;
        }

        public unsafe static double RecvDouble(this Mpi mpi, int source, int tag, IntPtr comm)
        {
            float result;
            int actualSource;
            mpi.Recv(&result, 1, Mpi.Double, source, tag, comm, out actualSource);
            return result;
        }
    }
}
