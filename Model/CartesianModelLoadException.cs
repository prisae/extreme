using System;

namespace Extreme.Cartesian.Model
{
    [Serializable]
    public class CartesianModelLoadException : System.IO.IOException
    {
        public CartesianModelLoadException(string message)
            : base(message)
        {
        }

        public CartesianModelLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
