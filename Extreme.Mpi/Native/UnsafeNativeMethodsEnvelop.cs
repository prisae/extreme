//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
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
			int thread_level;
            int error = InitNative(&thread_level);
			var sup_levels = new int[4];
			fixed(int* p =&sup_levels[0])
					GetThreadSupportLevels(p);
            if (error != 0)
                throw new InvalidOperationException("Can't init MPI subsytem");
			int rank = GetWorldRank ();
			if (rank == 0) {
				if (thread_level==sup_levels[3]) 				
					Console.WriteLine ("Absolute  multithreading");
				if (thread_level==sup_levels[2]) 				
					Console.WriteLine (" Only one thread will make MPI library calls at one time ");
				if (thread_level==sup_levels[1]) 				
					Console.WriteLine (" Only the thread that called MPI_Init_thread will make MPI calls");
				if (thread_level==sup_levels[0]) 				
					Console.WriteLine ("Multithreading is unsupported");
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
