//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core.Logger
{
    public class EmptyLogger : ILogger
    {
        public void Write(int logLevvel, string message)
        {
        }

        public void Dispose()
        {
            }
    }
}
