using System;
using Extreme.Cartesian.IntelMklFft;
using Extreme.Core;

namespace Extreme.Cartesian.FftW
{
    public unsafe class IntelMklMemoryProvider : MemoryProvider
    {
        private readonly int _alignment = 64;
        
        public IntelMklMemoryProvider(int alignment = 64)
        {
            _alignment = alignment;
        }

        protected override void ReleaseMemory(IntPtr ptr)
            => IntelMklUnm.Free(ptr);

        protected override IntPtr AllocateMemory(long sizeInBytes)
            => IntelMklUnm.Malloc(new IntPtr(sizeInBytes), _alignment);
    }
}
