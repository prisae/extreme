//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Core.Logger;

namespace Extreme.Parallel.Logger
{
    public class ParallelConsoleLogger : BaseLogger
    {
        private readonly Mpi _mpi;
        private readonly string _name;
        private readonly int _rank;
		private readonly bool _permitToWrite=false;
		public ParallelConsoleLogger(Mpi mpi,bool permit=false)
        {
            _mpi = mpi;
            if (mpi == null) throw new ArgumentNullException(nameof(mpi));

            _name = mpi.GetProcessorName();
            _rank = mpi.Rank;

			if (_mpi.IsMaster) {
				Console.WriteLine ($"Parallel logger started on [master {_name}] at {CreationTime}");
				_permitToWrite = true;
			} else {
				_permitToWrite = permit;
			}
        }

		protected virtual bool Filter(int logLevel) => false;

        public override void Write(int logLevel, string message)
        {
            if (_permitToWrite)
			{ 
				if (!Filter (logLevel)) {
					var time = (DateTime.Now - CreationTime).TotalSeconds;

					if (logLevel == (int)LogLevel.Status) {
						Console.WriteLine ($"[{time:########000.00} s] {message}");
					} else {
						var prefix = this.GetPrefix (logLevel);
						Console.WriteLine ($"[{time:########000.00} s] {prefix} {message}");
					}
				}
            }
        }
    }
}
