using System;

namespace Extreme.Cartesian.Model
{
    [Serializable]
    public class XProjectLoadException : System.IO.IOException
    {
        public XProjectLoadException(string message)
            : base(message)
        {
        }

        public XProjectLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
