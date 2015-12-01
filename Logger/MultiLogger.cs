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
                 //   Console.WriteLine($"Write to {logger}");
                    logger.Write(logLevel, message);
                }
                catch (Exception ex)
                {
                //    Console.WriteLine($"{ex.Message} {ex.StackTrace}");
                    LastExceptions.Add(ex);
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
