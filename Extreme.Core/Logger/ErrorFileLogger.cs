//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Core.Logger;

namespace Extreme.Core
{
    public class ErrorFileLogger : FileLogger
    {
        public ErrorFileLogger(string fileName, bool rewrite)
            : base(fileName, rewrite)
        {
        }

        protected override bool Filter(int logLevel)
            => !((LogLevel)logLevel).HasFlag(LogLevel.Error);
    }
}
