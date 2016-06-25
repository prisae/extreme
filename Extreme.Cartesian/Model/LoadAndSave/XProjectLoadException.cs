//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

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
