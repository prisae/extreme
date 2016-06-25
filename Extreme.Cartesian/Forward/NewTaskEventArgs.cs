//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
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
