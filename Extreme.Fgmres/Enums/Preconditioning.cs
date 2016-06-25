//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Fgmres
{
    public enum Preconditioning : long
    {
        None = 0,
        Left = 1,
        Right = 2,
        DoubleSide = 3,
    }
}
