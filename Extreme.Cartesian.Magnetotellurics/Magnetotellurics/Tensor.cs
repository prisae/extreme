//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class Tensor
    {
        public Tensor(Complex xx, Complex xy, Complex yx, Complex yy)
        {
            Xx = xx;
            Xy = xy;
            Yx = yx;
            Yy = yy;
        }

        public Complex Xx { get; }

        public Complex Xy { get; }

        public Complex Yx { get; }

        public Complex Yy { get; }
    }
}
