//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Cartesian.Core;

namespace Extreme.Cartesian.Magnetotellurics
{
    public class EScatteredCalculatedEventArgs : EventArgs
    {
        public AnomalyCurrent NormalFieldAtAnomaly { get; }
        public AnomalyCurrent EScattered { get; }

        public EScatteredCalculatedEventArgs(AnomalyCurrent normalFieldAtAnomaly, AnomalyCurrent eScattered)
        {
            NormalFieldAtAnomaly = normalFieldAtAnomaly;
            EScattered = eScattered;
        }
    }
}
