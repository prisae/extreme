//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Core
{
    public interface ILogger : IDisposable
    {
        void Write(int logLevel, string message);
    }
}
