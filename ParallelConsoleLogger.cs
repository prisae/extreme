using System;
using Extreme.Core.Logger;

namespace Extreme.Parallel.Logger
{
    public class ParallelConsoleLogger : BaseLogger
    {
        private readonly Mpi _mpi;
        private readonly string _name;
        private readonly int _rank;

        public ParallelConsoleLogger(Mpi mpi)
        {
            _mpi = mpi;
            if (mpi == null) throw new ArgumentNullException(nameof(mpi));

            _name = mpi.GetProcessorName();
            _rank = mpi.Rank;

            if (_mpi.IsMaster)
                Console.WriteLine($"Parallel logger started on [master{_name}] at {CreationTime}");

        }

        private void WriteStatus(string message)
        {
            if (_mpi.IsMaster)
            {
                var time = (DateTime.Now - CreationTime).TotalSeconds;

                Console.WriteLine($"[{time:########000.00} s] {message}");
            }
        }

        public override void Write(int logLevel, string message)
        {
            if (logLevel == (int)LogLevel.Status)
                WriteStatus(message);

            else
            {
                var prefix = this.GetPrefix(logLevel);
                Console.WriteLine("[{2}, {3}] {1} {4} {0}", message, DateTime.Now, _name, _rank, prefix);
            }
        }
    }
}
