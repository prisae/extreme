using System;
using System.Numerics;

namespace Extreme.Core
{
    public interface INativeMemoryProvider : IDisposable
    {
        IntPtr AllocateBytes(long numberOfBytes);
        void ReleaseMemory(IntPtr intPtr);
    }

    public static class NativeMemoryProviderHelper
    {
        private static unsafe IntPtr AllocateComplexIntPtr(this INativeMemoryProvider memoryProvider, long numberOfElements)
        {
            var ptr = memoryProvider.AllocateBytes(numberOfElements * sizeof(Complex));
            return ptr;
        }

        public static unsafe Complex* AllocateComplex(this INativeMemoryProvider memoryProvider, long numberOfElements)
        {
            var ptr = (Complex*)memoryProvider.AllocateComplexIntPtr(numberOfElements).ToPointer();
            return ptr;
        }

        public static unsafe double* AllocateDouble(this INativeMemoryProvider memoryProvider, long numberOfElements)
        {
            var ptr = (double*)memoryProvider.AllocateBytes(numberOfElements * sizeof(double)).ToPointer();
            return ptr;
        }

        public static unsafe void Release(this INativeMemoryProvider memoryProvider, void* data)
        {
            memoryProvider.ReleaseMemory(new IntPtr(data));
        }
    }

}
