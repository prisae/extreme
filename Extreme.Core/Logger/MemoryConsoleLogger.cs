//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Core.Logger
{
    public class MemoryConsoleLogger : ILogger
    {
        public void Write(int logLevel, string message)
        {
            Console.ForegroundColor = GetForegroundColor(logLevel);
            Console.WriteLine($"[memory] {message}");
            Console.ResetColor();
        }

        private ConsoleColor GetForegroundColor(int logLevel)
        {
            if (logLevel == (int)LogLevel.Warning)
                return ConsoleColor.Yellow;

            if (logLevel == (int)LogLevel.Error)
                return ConsoleColor.Red;

            return ConsoleColor.Green;
        }

        public void Dispose()
        {
        }
    }
}
