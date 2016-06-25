//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ElectricTensor : Tensor
    {
        public ElectricTensor(Complex zxx, Complex zxy, Complex zyx, Complex zyy)
            : base (zxx, zxy, zyx, zyy)
        {
        }
    }
}
