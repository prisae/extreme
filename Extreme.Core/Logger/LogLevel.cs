//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Core.Logger
{
    [Flags]
    public enum LogLevel
    {
        Status = 0x00,
        Warning = 0x01,
        Error = 0x02,
    }
}
