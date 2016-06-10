using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public interface IPlanCalculator
    {
        InnerResult CalculateForE(Transmitter transmitter, Receiver receiver);
        InnerResult CalculateForH(Transmitter transmitter, Receiver receiver);
    }
}
