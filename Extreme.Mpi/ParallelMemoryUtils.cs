//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Extreme.Core;

namespace Extreme.Parallel
{
    public static class ParallelMemoryUtils
    {
        public static void ExportMemoryUsage(string resultPath, Mpi mpi, INativeMemoryProvider memoryProvider, double frequency)
        {
            Complex localNativeMemory = (memoryProvider as MemoryProvider)?.GetAllocatedMemorySizeInBytes() / (1024 * 1024.0) ?? 0;
            var nativeMemory = new Complex[mpi?.Size ?? 1];

            Complex localManagedMemory = GC.GetTotalMemory(false) / (1024 * 1024.0);
            var managedMemory = new Complex[mpi?.Size ?? 1];

            Complex localLinuxMemory = (double)LinuxMemoryFileReader.GetTotalMemoryInMiB();
            var linuxMemory = new Complex[mpi?.Size ?? 1];

            mpi?.Gather(nativeMemory, localNativeMemory);
            mpi?.Gather(managedMemory, localManagedMemory);
            mpi?.Gather(linuxMemory, localLinuxMemory);

            if (mpi?.IsMaster ?? true)
            {
                string path = Path.Combine(resultPath, $"mem_info_mpi{mpi?.Size:0000}_freq{frequency}.dat");

                Func<Complex, string> pr = v => $"{v.Real / 1024:######0.0000} GiB".PadLeft(16);

                using (var sw = new StreamWriter(path))
                {
                    sw.WriteLine($"MPI_proc\t\tNATIVE\t\tMANAGED\t\tSUMM\t\tLINUX");

                    sw.Write($"Total".PadRight(16));
                    sw.Write(pr(nativeMemory.Sum(c => c.Real)));
                    sw.Write(pr(managedMemory.Sum(c => c.Real)));
                    sw.Write(pr(nativeMemory.Sum(c => c.Real) + managedMemory.Sum(c => c.Real)));
                    sw.Write(pr(linuxMemory.Sum(c => c.Real)));
                    sw.WriteLine();


                    for (int i = 0; i < nativeMemory.Length; i++)
                    {
                        sw.Write($"{i}".PadRight(16));
                        sw.Write(pr(nativeMemory[i]));
                        sw.Write(pr(managedMemory[i]));
                        sw.Write(pr(nativeMemory[i] + managedMemory[i]));
                        sw.Write(pr(linuxMemory[i]));
                        sw.WriteLine();
                    }
                }
            }
        }
    }
}
