using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FftWrap
{
    /// <summary>
    /// undocumented beyond-guru flags
    /// </summary>
    [Flags]
    public enum GuruFlags : uint
    {
        EstimatePatient = 1U << 7,
        BelievePcost = 1U << 8,
        NoDftR2Hc = 1U << 9,
        NoNonthreaded = 1U << 10,
        NoBuffering = 1U << 11,
        NoIndirectOp = 1U << 12,
        AllowLargeGeneric = 1U << 13, /* NO_LARGE_GENERIC is default */
        NoRankSplits = 1U << 14,
        NoVrankSplits = 1U << 15,
        NoVrecurse = 1U << 16,
        NoSimd = 1U << 17,
        NoSlow = 1U << 18,
        NoFixedRadixLargeN = 1U << 19,
        AllowPruning = 1U << 20,
    }
}
