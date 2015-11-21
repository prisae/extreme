using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Extreme.Core
{
    public static class MemoryUtils
    {
        public static string[] ParseStackTrace()
        {
            var full = Environment.StackTrace;

            var trace = full.Split(new[] { " at " }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(str => str.Trim())
                                .Where(str => !string.IsNullOrEmpty(str)).ToArray();
            return trace;
        }

        public static void PrintMemoryReport(string message, ILogger logger, INativeMemoryProvider memoryProvider)
        {
            var linux = LinuxMemoryFileReader.GetTotalMemoryInMiB();
            var native = (memoryProvider as MemoryProvider).GetAllocatedMemorySizeInBytes() / (1024 * 1024M);
            var managed = GC.GetTotalMemory(false) / (1024 * 1024M);

            //   logger.WriteStatus($"[{message.PadRight(15)}]: linux:{linux:0.000}, native:{native:0.000}, managed:{managed:0.000}, n+m:{native + managed:0.000}");
        }
    }
}
