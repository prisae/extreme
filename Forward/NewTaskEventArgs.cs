using System;
using Extreme.Parallel;

namespace Extreme.Cartesian.Forward
{
    public class NewTaskEventArgs : EventArgs
    {
        public NewTaskEventArgs(ParallelTask parallel)
        {
            Parallel = parallel;
        }
        public ParallelTask Parallel { get; }
    }
}
