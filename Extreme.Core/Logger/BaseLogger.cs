using System;

namespace Extreme.Core.Logger
{
    public abstract class BaseLogger : ILogger
    {
        protected DateTime CreationTime { get; }

        protected BaseLogger()
        {
            CreationTime = DateTime.Now;
        }

        public abstract void Write(int logLevel, string message);

        protected string GetPrefix(int logLevel)
        {
            var ll = ((LogLevel)logLevel);

            if (ll.HasFlag(LogLevel.Warning))
                return "WRN: ";

            if (ll.HasFlag(LogLevel.Error))
                return "ERR: ";

            return string.Empty;
        }

        public virtual void Dispose()
        {
        }
    }
}
