using Extreme.Cartesian.Green.Tensor;

namespace Extreme.Cartesian.Forward
{
    public class AtoAGreenTensorCalculatedEventArgs : System.EventArgs
    {
        public GreenTensor GreenTensor { get; }

        public AtoAGreenTensorCalculatedEventArgs(GreenTensor greenTensor)
        {
            GreenTensor = greenTensor;
        }
    }
}
