//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ElectricTipper : Tipper
    {
        public ElectricTipper(Complex zx, Complex zy)
            : base(zx, zy)
        { }

        public ElectricTipper(Tipper tipper) : base(tipper.Zx, tipper.Zy)
        {
        }
    }
}
