using System;

namespace Extreme.Core
{
    public class ErrorFileLogger : FullFileLogger
    {
        public ErrorFileLogger(string fileName, bool rewrite)
            : base(fileName, rewrite)
        {
        }

        protected override bool Filter(int logLevel)
            => !((LogLevel)logLevel).HasFlag(LogLevel.Error);
    }
}
