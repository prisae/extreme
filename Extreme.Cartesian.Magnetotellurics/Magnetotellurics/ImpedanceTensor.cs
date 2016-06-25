//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ImpedanceTensor : Tensor
    {
        public static ImpedanceTensor Empty = new ImpedanceTensor(0, 0, 0, 0);
        public static ImpedanceTensor EqualOne = new ImpedanceTensor(1, 1, 1, 1);

        public ImpedanceTensor(Complex xx, Complex xy, Complex yx, Complex yy)
            : base (xx, xy, yx, yy)
        {
        }
    }
}
