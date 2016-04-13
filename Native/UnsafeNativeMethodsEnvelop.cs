using System;
using System.Linq;
using System.Security;
using Extreme.Core;

namespace Extreme.Parallel
{
    [SuppressUnmanagedCodeSecurity]
    internal static partial class UnsafeNativeMethods
    {
        public unsafe static void Init()
        {
			int thread_on;
            int error = InitNative(&thread_on);

            if (error != 0)
                throw new InvalidOperationException("Can't init MPI subsytem");
			if (thread_on==0) {
				int rank = GetWorldRank ();
				if (rank == 0) {
					Console.WriteLine ("Multithreading is not supported");
				}
			}
        }

        public static void FinalizeMpi()
        {
            FinalizeNative();
        }

        public static int GetWorldRank()
        {
            int result;

            unsafe
            {
                int error = GetCommWorldRank(&result);

                if (error != 0)
                    throw new InvalidOperationException("Can't get rank");
            }

            return result;
        }

        public static int GetWorldSize()
        {
            int result;

            unsafe
            {
                int error = GetCommWorldSize(&result);

                if (error != 0)
                    throw new InvalidOperationException("Can't get size");
            }

            return result;
        }
        

        public static string GetErrorString(int errorcode)
        {
            unsafe
            {
                byte[] buffer = new byte[1024];

                fixed (byte* b = &buffer[0])
                {
                    int length;

                    GetErrorString(errorcode, b, &length);

                    return System.Text.Encoding.ASCII.GetString(buffer, 0, length);
                }
            }
        }
    }
}
