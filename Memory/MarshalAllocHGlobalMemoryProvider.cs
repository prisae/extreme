using System;
using System.Runtime.InteropServices;

namespace Extreme.Core
{
    public unsafe class MarshalAllocHGlobalMemoryProvider : MemoryProvider
    {
        protected override void ReleaseMemory(IntPtr ptr)
            => Marshal.FreeHGlobal(ptr);

        protected override IntPtr AllocateMemory(long size)
            => Marshal.AllocHGlobal(new IntPtr(size));
    }
}
