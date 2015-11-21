using System;

namespace Extreme.Core
{
    public class ConsoleLogger : Logger
    {
        public ConsoleLogger() : this("")
        {
        }

        protected ConsoleLogger(string prefix)
        {
            Console.WriteLine($"{prefix} Console logger started at {CreationTime}");
        }

        public sealed override void Write(int logLevel, string message)
        {
            if (!Filter(logLevel))
            {
                var prefix = GetPrefix(logLevel);
                var time = (DateTime.Now - CreationTime).TotalSeconds;

                Console.ForegroundColor = GetForegroundColor(logLevel);
                Console.WriteLine($"[{time:########000.00} s] {prefix} {message}");
                Console.ResetColor();
            }
        }

        private ConsoleColor GetForegroundColor(int logLevel)
        {
            var ll = ((LogLevel)logLevel);

            if (ll.HasFlag(LogLevel.Warning))
                return ConsoleColor.Yellow;

            if (ll.HasFlag(LogLevel.Error))
                return ConsoleColor.Red;

            return ConsoleColor.Gray;
        }

        protected virtual bool Filter(int logLevel) => false;
    }
}
