using System;

namespace Extreme.Parallel
{
    [Serializable]
    public class MpiException : Exception
    {
        private readonly int _error;

        public MpiException(string message, int error)
            : base(message)
        {
            _error = error;
        }

        public int Error
        {
            get { return _error; }
        }
    }
}
