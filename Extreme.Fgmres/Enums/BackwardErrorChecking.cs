//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Fgmres
{
    public enum BackwardErrorChecking
    {
        CheckArnoldiOnly = 0,
        CheckWithWarning = 1,
        CheckWithException = 2,
    }
}
