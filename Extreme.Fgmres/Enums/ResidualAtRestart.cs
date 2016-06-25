//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Fgmres
{
    public enum ResidualAtRestart : long
    {
        ComputeTheTrue = 1,
        UseRecurrenceFormula = 0,
    }
}
