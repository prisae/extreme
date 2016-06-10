using System;
using System.Linq;
using System.Collections.Generic;
using Extreme.Core.Logger;

namespace Extreme.Core
{
    public abstract class MemoryProvider : INativeMemoryProvider
    {
        #region MemoryDescriptor
        private class MemoryDescriptor
        {
            public static readonly MemoryDescriptor Empty = default(MemoryDescriptor);

            public MemoryDescriptor(IntPtr ptr, long numberOfBytes)
            {
                NumberOfBytes = numberOfBytes;
                Ptr = ptr;
            }

            public IntPtr Ptr { get; }

            public long NumberOfBytes { get; }
        }

        #endregion

        private readonly List<MemoryDescriptor> _allocated = new List<MemoryDescriptor>();
        private readonly List<MemoryDescriptor> _free = new List<MemoryDescriptor>();
        private ILogger _logger;

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract void ReleaseMemory(IntPtr ptr);
        protected abstract IntPtr AllocateMemory(long sizeInBytes);

        IntPtr INativeMemoryProvider.AllocateBytes(long numberOfBytes)
        {
            var cashed = _free.Find(md => md.NumberOfBytes == numberOfBytes);

            if (cashed != null)
            {
                _free.Remove(cashed);
                _logger?.WriteStatus($"\t\t\t\tReUsing {numberOfBytes}");
                return cashed.Ptr;
            }

            var ptr = AllocateMemory(numberOfBytes);
            var descriptor = new MemoryDescriptor(ptr, numberOfBytes);
            _allocated.Add(descriptor);

            var stack = MemoryUtils.ParseStackTrace();

            var gigaBytes = numberOfBytes / (1024.0 * 1024 * 1024);

            _logger?.WriteError($"Allocating {gigaBytes:######0.0000} GiB ({numberOfBytes} bytes), ptr:{ptr} at {stack[7]}");

            return ptr;
        }


        void INativeMemoryProvider.ReleaseMemory(IntPtr ptr)
        {
            var disc = _allocated.Find(md => md.Ptr == ptr);
            if (disc == null)
                throw new InvalidOperationException("The memory was not allocated by this manager");

            var free = _free.Find(md => md.Ptr == ptr);
            if (free != null)
                throw new InvalidOperationException("Double memory free");

            _free.Add(disc);

            _logger?.WriteWarning($"\t\t\t\t Put in Free {disc.NumberOfBytes} Ptr:{disc.Ptr}");

            //ReleaseMemory(ptr);
            //_allocated.RemoveAll(d => d.Ptr == ptr);
        }

        public long GetAllocatedMemorySizeInBytes()
        {
            return _allocated.Sum(md => md.NumberOfBytes);
        }

        void IDisposable.Dispose()
        {
            foreach (var descriptor in _allocated)
                ReleaseMemory(descriptor.Ptr);

            _allocated.Clear();
        }
    }
}
