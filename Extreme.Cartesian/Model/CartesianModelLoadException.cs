//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

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
