//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public interface IProfiler
    {
        void Start(int code);
        void End(int code);
    }
}
