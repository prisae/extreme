using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FftWrap
{
    [Flags]
    public enum Flags : uint
    {
        Measure = 0U,
        DestroyInput = 1U << 0,
        Unaligned = 1U << 1,
        ConserveMemory = 1U << 2,
        Exhaustive = 1U << 3, /* NO_EXHAUSTIVE is default */
        PreserveInput = 1U << 4, /* cancels FFTW_DESTROY_INPUT */
        Patient = 1U << 5, /* IMPATIENT is default */
        Estimate = 1U << 6,
        WisdomOnly = 1U << 21,
    }
}
