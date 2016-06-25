//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Fgmres
{
    public class RightPreconditionerRequestEventArgs : EventArgs
    {
        public NativeVector Src { get; }
        public NativeVector Dst { get; }

        public RightPreconditionerRequestEventArgs(NativeVector src, NativeVector dst)
        {
            Src = src;
            Dst = dst;
        }
    }
}
