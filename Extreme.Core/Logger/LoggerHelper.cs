//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core.Logger
{
    public static class LoggerHelper
    {
        public static void WriteStatus(this ILogger logger, string message)
        {
            logger.Write((int) LogLevel.Status, message);
        }

        public static void WriteWarning(this ILogger logger, string message)
        {
            logger.Write((int) LogLevel.Warning, message);
        }

        public static void WriteError(this ILogger logger, string message)
        {
            logger.Write((int) LogLevel.Error, message);
        }
    }

}
