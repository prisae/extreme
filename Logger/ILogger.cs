using System;

namespace Extreme.Core
{
    public interface ILogger : IDisposable
    {
        void Write(int logLevel, string message);
    }
}
