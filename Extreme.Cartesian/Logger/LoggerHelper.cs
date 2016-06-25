//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Core;
using Extreme.Core.Logger;

namespace Extreme.Cartesian.Logger
{
    public static class ForwardLoggerHelper
    {
        public static void WriteStatus(this ILogger logger, string message)
        {
            logger.Write((int)LogLevel.Status | (int)ForwardLogLevel.Forward, message);
        }

        public static void WriteWarning(this ILogger logger, string message)
        {
            logger.Write((int)LogLevel.Warning | (int)ForwardLogLevel.Forward, message);
        }

        public static void WriteError(this ILogger logger, string message)
        {
            logger.Write((int)LogLevel.Error | (int)ForwardLogLevel.Forward, message);
        }
    }
}
