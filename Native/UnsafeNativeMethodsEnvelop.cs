using System;
using System.Linq;
using System.Security;
using Extreme.Core;

namespace Extreme.Parallel
{
    [SuppressUnmanagedCodeSecurity]
    internal static partial class UnsafeNativeMethods
    {
        public static void Init()
        {
            int error = InitNative();

            if (error != 0)
                throw new InvalidOperationException("Can't init MPI subsytem");
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
                int error = GetCommWorldRankNative(&result);

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
