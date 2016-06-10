using System.Numerics;

namespace Extreme.Fgmres
{
    public unsafe class DotProductRequestEventArgs : System.EventArgs
    {
		public long Length { get; }
        public int NumberOfDotProducts { get; }
        public Complex* X { get; }
        public Complex* Y { get; }
        public Complex* Result { get; }


        public DotProductRequestEventArgs(long length, Complex* x, Complex* y, Complex* result, int numberOfDotProducts)
        {
            Length = length;
            X = x;
            Y = y;
            Result = result;
            NumberOfDotProducts = numberOfDotProducts;
        }
    }
}
