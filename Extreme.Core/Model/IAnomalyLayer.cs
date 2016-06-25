//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Extreme.Core
{
    public interface IAnomalyLayer
    {
        decimal Depth { get; }
        decimal Thickness { get; }
    }
}
