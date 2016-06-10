﻿using Extreme.Core.Logger;

namespace Extreme.Core
{
    public class WarningFileLogger : FileLogger
    {
        public WarningFileLogger(string fileName, bool rewrite)
            : base(fileName, rewrite)
        {
        }

        protected override bool Filter(int logLevel)
            => !((LogLevel)logLevel).HasFlag(LogLevel.Warning);
    }
}
