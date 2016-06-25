//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ElectricPhaseTensor : PhaseTensor
    {
        public ElectricPhaseTensor(double xx, double xy, double yx, double yy)
            : base(xx, xy, yx, yy)
        {
        }
    }
}
