using System;
using System.Collections;
using System.Collections.Generic;

namespace Extreme.Core
{
    public class MultiLogger : ILogger, IEnumerable<ILogger>
    {
        private readonly List<ILogger> _loggers;
        public List<Exception> LastExceptions { get; } = new List<Exception>();
        public bool HasExceptions => LastExceptions.Count != 0;

        public bool ReportLoggersExceptionsToConsole { get; } = true;

        public MultiLogger()
        {
            _loggers = new List<ILogger>();
        }

        public MultiLogger(params ILogger[] loggers)
            : this()
        {
            _loggers.AddRange(loggers);
        }

        public void Add(ILogger logger)
        {
            _loggers.Add(logger);
        }

        public void Write(int logLevel, string message)
        {
            LastExceptions.Clear();

            foreach (var logger in _loggers)
            {
                try
                {
                    logger.Write(logLevel, message);
                }
                catch (Exception ex)
                {
                    LastExceptions.Add(ex);
                }
            }

            if (ReportLoggersExceptionsToConsole && HasExceptions)
            {
                Console.WriteLine($"Logger exceptions, {LastExceptions.Count} items:");

                for (int i = 0; i < LastExceptions.Count; i++)
                {
                    Console.WriteLine($"{i+1} of {LastExceptions.Count}:\n" +
                                      $"{LastExceptions[i].Message}\n" +
                                      $"{LastExceptions[i].StackTrace}");
                }
            }
        }

        public IEnumerator<ILogger> GetEnumerator()
            => _loggers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Dispose()
            => _loggers.ForEach(l => l.Dispose());
    }
}
