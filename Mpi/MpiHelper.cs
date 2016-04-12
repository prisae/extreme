using System;

namespace Extreme.Parallel
{
    public static class MpiHelper
    {
        public unsafe static void Send(this Mpi mpi, IntPtr data, int count, IntPtr datatype, int dest, int tag)
        {
            mpi.Send(data.ToPointer(), count, datatype, dest, tag);
        }

        public unsafe static void Send(this Mpi mpi, int data, int dest, int tag)
        {
            mpi.Send(&data, 1, Mpi.Int, dest, tag);
        }

        public unsafe static void Send(this Mpi mpi, double data, int dest, int tag)
        {
            mpi.Send(&data, 1, Mpi.Double, dest, tag);
        }

        public unsafe static int RecvInt(this Mpi mpi, int source, int tag)
        {
            int result;
            int actualSource;
            int err = mpi.Recv(&result, 1, Mpi.Int, source, tag,  out actualSource);
            return result;
        }

        public unsafe static int RecvInt(this Mpi mpi, int source, int tag,  out int actualSource)
        {
            int result;
            int err = mpi.Recv(&result, 1, Mpi.Int, source, tag,  out actualSource);
            return result;
        }

        public unsafe static double RecvDouble(this Mpi mpi, int source, int tag)
        {
            float result;
            int actualSource;
            mpi.Recv(&result, 1, Mpi.Double, source, tag,  out actualSource);
            return result;
        }
    }
}
