using System;

namespace Extreme.Fgmres
{
    public class GmresIterationCompleteEventArgs : EventArgs
    {
        public int NumberOfIteration { get; }
        public double ArnoldiBackwardError { get; }

        public GmresIterationCompleteEventArgs(int numberOfIteration, double arnoldiBackwardError)
        {
            NumberOfIteration = numberOfIteration;
            ArnoldiBackwardError = arnoldiBackwardError;
        }
    }
}
