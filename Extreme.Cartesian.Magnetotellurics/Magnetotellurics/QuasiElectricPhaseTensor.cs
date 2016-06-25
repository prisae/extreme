//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Porvem.Magnetotellurics
{
    public class QuasiElectricPhaseTensor : PhaseTensor
    {
        public QuasiElectricPhaseTensor(double xx, double xy, double yx, double yy)
            : base(xx, xy, yx, yy)
        {
        }
    }
}
