using System;
using System.Linq;
using System.Collections.Generic;
using Extreme.Cartesian.Core;

namespace Extreme.Cartesian.IntelMklFft
{
    public class IntelFourierTransform 
    {
        private readonly List<FftDescriptor> _availableDescriptors = new List<FftDescriptor>();
        private readonly object _sync = new object();

        public void Forward2D(int nx, int ny, IntPtr ptr)
        {
            var descriptor = GetDescriptor(nx, ny);
            IntelMklUnm.PerformForwardFft(descriptor, ptr);
        }

        public void Backward2D(int nx, int ny, IntPtr ptr)
        {
            var descriptor = GetDescriptor(nx, ny);
            IntelMklUnm.PerformBackwardFft(descriptor, ptr);
        }

        private IntPtr GetDescriptor(int nx, int ny)
        {
            lock (_sync)
            {
                var descriptor = _availableDescriptors.FirstOrDefault(d => d.Nx == nx && d.Ny == ny);

                if (descriptor != FftDescriptor.Empty)
                    return descriptor.Ptr;

                var ptr = IntelMklUnm.CreateDftiDescriptor(nx, ny);

                _availableDescriptors.Add(new FftDescriptor(nx, ny, ptr));

                return ptr;
            }
        }

        public void Dispose()
        {
            ReleaseAllDescriptors();
            GC.SuppressFinalize(this);
        }

        private void ReleaseAllDescriptors()
        {
            lock (_sync)
            {
                foreach (var descriptor in _availableDescriptors)
                {
                    var ptr = descriptor.Ptr;
                    IntelMklUnm.FreeDftiDescriptor(ref ptr);
                }

                _availableDescriptors.Clear();
            }
        }
    }
}
