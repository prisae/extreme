using System;

namespace Extreme.Fgmres
{
    public class MatrixVectorMultRequestEventArgs : EventArgs
    {
        public NativeVector X { get; }
        public NativeVector Result { get; }

        public MatrixVectorMultRequestEventArgs(NativeVector x, NativeVector result)
        {
            X = x;
            Result = result;
        }
    }
}
