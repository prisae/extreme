using System;

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
