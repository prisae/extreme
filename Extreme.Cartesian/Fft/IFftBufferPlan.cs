using System.Numerics;

namespace Extreme.Cartesian.Fft
{
    public unsafe  interface IFftBufferPlan
    {
        Complex* Buffer1Ptr { get; }
        Complex* Buffer2Ptr { get; }
        int BufferLength { get; }
    }
}
