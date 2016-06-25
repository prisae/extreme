//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

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
